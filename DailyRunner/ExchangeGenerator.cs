using DailyRunner.Helpers;
using FlatTrade;
using FlatTrade.Common.Helpers;
using FlatTrade.Common.Types.Base;
using FlatTrade.UserManager;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Diagnostics;

namespace DailyRunner
{
    internal class ExchangeGenerator
    {
        private readonly ILogger<ExchangeGenerator> _logger;
        private readonly Api _api;
        private readonly bool _enabled;

        private readonly CsvWriter? _csvWriter;
        private readonly DbWriter? _dbWriter;

        private readonly string _storedProcedureName = "[dbo].[sp_UpsertExchanges]";
        private readonly string _tvpTypeName = "[dbo].[TExchanges]";

        private readonly List<Task?> _taskList = [];
        public ExchangeGenerator(Api api, IConfiguration config, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ExchangeGenerator>();
            _api = api;
            _enabled = Convert.ToBoolean(config["ExchangeGenerator:Enabled"] ?? "false");
            if (!_enabled)
                return;

            var writeToDbEnabled = Convert.ToBoolean(config["ExchangeGenerator:WriteToDb:Enabled"] ?? "false");
            if (writeToDbEnabled)
            {
                var connectionString = config["Database:ConnectionString"] ?? string.Empty;
                var dbChannelCapacity = Convert.ToInt32(config["ExchangeGenerator:WriteToDb:ChannelCapacity"] ?? "5");
                var writeBatchSize = Convert.ToInt32(config["ExchangeGenerator:WriteToDb:WriteBatchSizeInDb"] ?? "5000");
                _dbWriter = new DbWriter(connectionString, writeBatchSize, dbChannelCapacity, nameof(ExchangeGenerator), loggerFactory);
            }

            var writeToFileEnabled = Convert.ToBoolean(config["ExchangeGenerator:WriteToFile:Enabled"] ?? "false");
            if (writeToFileEnabled)
            {
                var fileChannelCapacity = Convert.ToInt32(config["ExchangeGenerator:WriteToFile:ChannelCapacity"] ?? "5");
                var filePath = config["ExchangeGenerator:WriteToFile:CsvFilePath"] ?? string.Empty;
                if (string.IsNullOrEmpty(filePath))
                    throw new ArgumentNullException(nameof(filePath), "CSV file path is not configured.");

                filePath = Path.GetFullPath(filePath);
                _csvWriter = new CsvWriter(Path.GetDirectoryName(filePath!)!, fileChannelCapacity, nameof(ExchangeGenerator), loggerFactory);
            }
        }
        public async Task GenerateAndLoad()
        {
            if (!_enabled)
            {
                _logger.LogInformation("ExchangeGenerator is disabled in config.");
                return;
            }

            var stopWatch = new Stopwatch();
            try
            {
                stopWatch.Start();
                var (resp, msg) = await _api.User.GetUserDetailsAsync();
                msg = $"Exchange information from server. {msg}";
                if (resp is null || resp.Status != Constants.StatusOk)
                {
                    _logger.LogError("NOK: {msg}", msg);
                    return;
                }

                var headerAndContents = GenerateHeaderAndContents(resp);
                
                if (_csvWriter is not null)
                {
                    var csvChannelObject = new CsvChannelObject
                    {
                        FileName = $"Exchanges_{DateTime.Now:yyyyMMdd}.csv",
                        Header = headerAndContents.Item1,
                        Records = headerAndContents.Item2
                    };
                    await _csvWriter.WriteCsvAsync(csvChannelObject);
                }
                 
                if (_dbWriter is not null)
                {
                    var dbChannelObject = new DbChannelObject
                    {
                        TvpName = _tvpTypeName,
                        StoredProcedureName = _storedProcedureName,
                        Records = ToDataTable(headerAndContents.Item3)
                    };
                    await _dbWriter.WriteDbAsync(dbChannelObject);
                }
                    
            }
            catch (AggregateException ae)
            {
                _logger.LogError("\n--- One or more tasks failed: ---");
                foreach (var ex in ae.Flatten().InnerExceptions)
                {
                    _logger.LogError(ex, "  Error: {ex.GetType().Name} - {ex.Message}", ex.GetType().Name, ex.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "\n--- An unexpected error occurred: ---");
                _logger.LogCritical("  Error: {ex.GetType().Name} - {ex.Message}", ex.GetType().Name, ex.Message);
            }
            finally
            {
                await Utility.WhenAllSafe([.._taskList]);

                _taskList.Clear();
                _taskList.Add(_csvWriter?.WriteComplete()); // Signal completion
                _taskList.Add(_dbWriter?.WriteComplete()); // Signal completion

                await Utility.WhenAllSafe([.._taskList]);

                _logger.LogInformation("=========== [{Exchanges}] Done. Took : {msg}", nameof(ExchangeGenerator), stopWatch.StopAndLog());
            }
        }

        private static (string, IEnumerable<string>, IEnumerable<(string, string)>) GenerateHeaderAndContents(UserDetailsResponse obj)
        {
            var csvHeader = "Code,Description";
            List<string> lines = [];
            List<(string, string)> Objects = [];
            foreach (var exch in obj.Exchanges)
            {
                var description = exch switch// (exch)
                {
                    Exchange.NSE => "National Stock Exchange of India",
                    Exchange.BSE => "Bombay Stock Exchange",
                    Exchange.MCX => "Multi Commodity Exchange of India",
                    Exchange.NFO => "National Futures and Options Exchange",
                    Exchange.CDS => "Central Depository System",
                    Exchange.BFO => "Bombay Stock Exchange - Futures and Options segment",
                    Exchange.BCD => "Bloomberg All Commodity Longer Dated Strategy K-1 Free ETF",
                    _ => "Unknown Exchange" // Default for 'Other' or any unhandled enum value
                };
                lines.Add($"{exch},{description}");
                Objects.Add((exch.ToString(), description));
            }
            return (csvHeader, lines, Objects);
        }

        private static DataTable ToDataTable(IEnumerable<(string, string)> objects)
        {
            var table = new DataTable();
            table.Columns.Add("Code", typeof(string));
            table.Columns.Add("Description", typeof(string));

            foreach (var obj in objects)
                table.Rows.Add(obj.Item1, obj.Item2);

            return table;
        }
    }
}
