using Common.Helpers;
using Common.Types;
using FlatTrade;
using FlatTrade.Types.Base;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StrategyEngine.BrokerData;
using StrategyEngine.Model;
using StrategyEngine.OrderProcessors.SingleLegOrder;
using System.Data;

namespace StrategyEngine.Strategies
{
    internal abstract class AbstractStrategy<T> : IStrategy, IDisposable, IAsyncDisposable
    {
        protected ILoggerFactory _loggerFactory;
        protected ILogger<T> _logger;
        protected abstract string Name { get; }

        protected readonly IConfiguration Config;
        protected readonly Api Api;

        protected bool _disposed = false;
        private readonly DbWriter? _dbWriter;
        
        private readonly string _storedProcedureName = "[dbo].[sp_UpsertStrategySignals]";
        private readonly string _tvpTypeName = "[dbo].[TStrategySignals]";
        
        internal AbstractStrategy(IConfiguration config, Api api, ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory ?? new LoggerFactory();
            _logger = _loggerFactory.CreateLogger<T>();
            Config = config;
            Api = api;

            var writeToDbEnabled = Convert.ToBoolean(config["Strategy:WriteToDb:Enabled"] ?? "false");
            if (writeToDbEnabled)
            {
                var connectionString = config["Database:ConnectionString"] ?? string.Empty;                

                var dbChannelCapacity = Convert.ToInt32(config["Strategy:WriteToDb:ChannelCapacity"] ?? "5000");
                var writeBatchSize = Convert.ToInt32(config["Strategy:WriteToDb:WriteBatchSizeInDb"] ?? "5000");
                _dbWriter = new DbWriter(connectionString, writeBatchSize, dbChannelCapacity, nameof(AbstractStrategy<T>), _loggerFactory);
            }
        }

        public async Task<StrategySignal?> Process(object? obj)
        {
            if (obj is null)
                return default;

            var signal = await ProcessInternal(obj);
            if(signal is not null && _dbWriter is not null)
            {                               
                var dbChannelObject = new DbChannelObject
                {
                    TvpName = _tvpTypeName,
                    StoredProcedureName = _storedProcedureName,
                    Records = ToDataTable(signal!)
                };
                await _dbWriter.WriteDbAsync(dbChannelObject);                
            }
            return signal;
            //await ProcessInternal((dynamic)obj!);
        }
        
        protected virtual async Task<StrategySignal?> ProcessInternal(object input)
        {
            return input switch
            {
                StrategyOnScripSnapshot scrip => await ProcessInternal(scrip),
                StrategyOnHoldingSnapshot holding => await ProcessInternal(holding),
                StrategyOnOrderSnapshot order => await ProcessInternal(order),
                StrategyOnPositionSnapshot position => await ProcessInternal(position),
                StrategyOnTradeSnapshot trade => await ProcessInternal(trade),
                StrategyOnQuoteSnapshot quote => await ProcessInternal(quote),
                StrategyOnTouchLineSnapshot touch => await ProcessInternal(touch),
                _ => default
            };
        }

        // Default implementations — derived classes can override only what they need
        protected virtual Task<StrategySignal?> ProcessInternal(StrategyOnCandleSnapshot input) => Task.FromResult<StrategySignal?>(default);
        protected virtual Task<StrategySignal?> ProcessInternal(StrategyOnScripSnapshot input) => Task.FromResult<StrategySignal?>(default);
        protected virtual Task<StrategySignal?> ProcessInternal(StrategyOnHoldingSnapshot input) => Task.FromResult<StrategySignal?>(default);
        protected virtual Task<StrategySignal?> ProcessInternal(StrategyOnOrderSnapshot input) => Task.FromResult<StrategySignal?>(default);
        protected virtual Task<StrategySignal?> ProcessInternal(StrategyOnPositionSnapshot input) => Task.FromResult<StrategySignal?>(default);
        protected virtual Task<StrategySignal?> ProcessInternal(StrategyOnTradeSnapshot input) => Task.FromResult<StrategySignal?>(default);
        protected virtual Task<StrategySignal?> ProcessInternal(StrategyOnQuoteSnapshot input) => Task.FromResult<StrategySignal?>(default);
        protected virtual Task<StrategySignal?> ProcessInternal(StrategyOnTouchLineSnapshot input) => Task.FromResult<StrategySignal?>(default);

        protected bool AnyPendingOrder(string tradingSymbol, long token) =>        
                                        GlobalDataSet.Data.TryGetValue(tradingSymbol, out Details? details)
                                        && details is not null
                                        && details.OpenOrders.ContainsKey(token);
        
        protected bool AnyPendingPosition(string tradingSymbol, ProductType productType) =>
                                        GlobalDataSet.Data.TryGetValue(tradingSymbol, out Details? details)
                                        && details is not null
                                        && details.OpenPositions.ContainsKey(productType);

        private static DataTable ToDataTable(StrategySignal signal)
        {
            var table = new DataTable();
            table.Columns.Add("SignalId", typeof(string));
            table.Columns.Add("StrategyName", typeof(string)); 
            table.Columns.Add("Comments", typeof(string));
            table.Columns.Add("SignalDate", typeof(DateTime));
            table.Columns.Add("TradingSymbol", typeof(string));
            table.Columns.Add("Token", typeof(int));
            table.Columns.Add("Exchange", typeof(string));
            table.Columns.Add("ProductType", typeof(string));
            table.Columns.Add("PriceType", typeof(string));
            table.Columns.Add("RetentionType", typeof(string));
            table.Columns.Add("TransactionType", typeof(string));
            table.Columns.Add("LimitPrice", typeof(decimal));
            table.Columns.Add("Quantity", typeof(int));
            table.Columns.Add("DifferentialProfitPrice", typeof(decimal));
            table.Columns.Add("DifferentialSLPrice", typeof(decimal));
            table.Columns.Add("DifferentialTrailingTicks", typeof(decimal));
            table.Columns.Add("TriggerPrice", typeof(decimal));
            table.Columns.Add("MarketProtectionInPercent", typeof(decimal));

            if (signal.OutputDecision is OutputDecision.Create create)
            {
                var order = create.Order;
                table.Rows.Add(signal.StrategySignalId.ToString(),
                                signal.StrategyName,
                                signal.DecisionMakingInputs.DecisionMakingRemarks,
                                order.InternalOrderId.ToString(),
                                DateTime.Now,
                                order.TradingSymbol,
                                order.Token,
                                order.Exchange.ToString(),
                                order.ProductType.ToString(),
                                order.PriceType.ToString(),
                                order.RetentionType.ToString(),
                                order.TransactionType.ToString(),
                                order.LimitPrice,
                                order.Quantity,
                                order.DifferentialProfitPrice,
                                order.DifferentialSLPrice,
                                order.DifferentialTrailingTicks,
                                order.TriggerPrice,
                                order.MarketProtectionInPercent
                                );
            }
            return table;
        }

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
