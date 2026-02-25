using Common.Helpers;
using Common.Types;
using FlatTrade;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StrategyEngine.Model;
using StrategyEngine.OrderProcessors;
using StrategyEngine.Strategies;
using System.Data;

namespace StrategyEngine.RMS
{
    internal abstract class AbstractRms<T> : IRms, IDisposable, IAsyncDisposable
    {
        protected ILoggerFactory _loggerFactory;
        protected ILogger<T> _logger;
        protected abstract string Name { get; }

        protected readonly IOrderProcessor OrderProcessor;
        protected readonly IConfiguration Config;
        protected readonly Api Api;
        
        protected bool _disposed = false;
        protected readonly DbWriter? _dbWriter;

        private readonly string _storedProcedureName = "[dbo].[sp_UpsertRmsRejection]";
        private readonly string _tvpTypeName = "[dbo].[TRmsRejection]";

        internal AbstractRms(IConfiguration config, Api api, IOrderProcessor orderProcessor, ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory ?? new LoggerFactory();
            _logger = _loggerFactory.CreateLogger<T>();
            Config = config;
            OrderProcessor = orderProcessor;
            Api = api;

            var writeToDbEnabled = Convert.ToBoolean(config["Rms:WriteToDb:Enabled"] ?? "false");
            if (writeToDbEnabled)
            {
                var connectionString = config["Database:ConnectionString"] ?? string.Empty;

                var dbChannelCapacity = Convert.ToInt32(config["Rms:WriteToDb:ChannelCapacity"] ?? "5000");
                var writeBatchSize = Convert.ToInt32(config["Rms:WriteToDb:WriteBatchSizeInDb"] ?? "5000");
                _dbWriter = new DbWriter(connectionString, writeBatchSize, dbChannelCapacity, nameof(AbstractRms<T>), _loggerFactory);
            }
        }

        protected async Task WriteToDb(StrategySignal signal, string rmsRuleName, string failureReason)
        {
            if (_dbWriter is null)
                return;

            var dbChannelObject = new DbChannelObject
            {
                TvpName = _tvpTypeName,
                StoredProcedureName = _storedProcedureName,
                Records = ToDataTable(signal, rmsRuleName, failureReason)
            };
            await _dbWriter.WriteDbAsync(dbChannelObject);
        }

        private static DataTable ToDataTable(StrategySignal signal, string rmsRuleName, string failureReason)
        {
            var table = new DataTable();
            table.Columns.Add("RmsId", typeof(string));
            table.Columns.Add("RmsRuleName", typeof(string));
            table.Columns.Add("StrategySignalId", typeof(string));
            table.Columns.Add("StrategyName", typeof(string));
            table.Columns.Add("SignalDate", typeof(DateTime));            
            table.Columns.Add("Reason", typeof(string));

            if (signal.OutputDecision is OutputDecision.Create create)
            {
                var order = create.Order;
                table.Rows.Add(Guid.NewGuid().ToString(),
                                rmsRuleName,
                                signal.StrategySignalId,
                                signal.StrategyName,
                                DateTime.Now,                                
                                failureReason
                                );
            }
            return table;
        }

        public abstract Task<bool> IsValidationSucceeded(StrategySignal signal);
        
        public async Task OnUpdate(object? obj)
        {
            if (obj is null)
                return;

            await OnUpdateInternal(obj);           
        }
        
        protected virtual Task OnUpdateInternal(object input)
        {
            return input switch
            {
                StrategyOnScripSnapshot scrip => OnUpdateInternal(scrip),
                StrategyOnHoldingSnapshot holding => OnUpdateInternal(holding),
                StrategyOnOrderSnapshot order => OnUpdateInternal(order),
                StrategyOnPositionSnapshot position => OnUpdateInternal(position),
                StrategyOnTradeSnapshot trade => OnUpdateInternal(trade),
                StrategyOnQuoteSnapshot quote => OnUpdateInternal(quote),
                StrategyOnTouchLineSnapshot touch => OnUpdateInternal(touch),
                _ => Task.CompletedTask
            };
        }

        // Default implementations — derived classes can override only what they need
        protected virtual Task OnUpdateInternal(StrategyOnCandleSnapshot input) => Task.CompletedTask;
        protected virtual Task OnUpdateInternal(StrategyOnScripSnapshot input) => Task.CompletedTask;
        protected virtual Task OnUpdateInternal(StrategyOnHoldingSnapshot input) => Task.CompletedTask;
        protected virtual Task OnUpdateInternal(StrategyOnOrderSnapshot input) => Task.CompletedTask;
        protected virtual Task OnUpdateInternal(StrategyOnPositionSnapshot input) => Task.CompletedTask;
        protected virtual Task OnUpdateInternal(StrategyOnTradeSnapshot input) => Task.CompletedTask;
        protected virtual Task OnUpdateInternal(StrategyOnQuoteSnapshot input) => Task.CompletedTask;
        protected virtual Task OnUpdateInternal(StrategyOnTouchLineSnapshot input) => Task.CompletedTask;


        public void Dispose()
        {
            DisposeAsyncCore().AsTask().GetAwaiter().GetResult(); // Safe sync fallback
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore().ConfigureAwait(false);
            GC.SuppressFinalize(this);
        }

        protected virtual async ValueTask DisposeAsyncCore()
        {
            if (_disposed)
                return;

            _disposed = true;

            // Dispose async resources
            if (_dbWriter is not null)
                await _dbWriter.WriteComplete().ConfigureAwait(false);
            _logger.LogInformation("{0}: Disposed gracefully", GetType().Name);
            // Dispose other sync-only resources here (e.g., timers, files)
            return;
        }
    }
}
