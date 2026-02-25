using Upstox;
using Common.Helpers;
using Common.Types;
using Upstox.MarketInfoManager;
using Upstox.Types.Base;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using Utility = DailyRunner.Helpers.Utility;

namespace DailyRunner.Upstox
{
    internal class StocksOhlcvGenerator
    {
        private readonly ILogger<StocksOhlcvGenerator> _logger;
        private readonly Api _api;
        private readonly bool _enabled;
        private List<(Exchange, long, string)> _exchangeTokenSymbolTuple = [];

        private readonly DateTime _startDate;
        private readonly DateTime _endDate = DateTime.Now.Date.AddDays(1).ToLocalTime();

        private readonly int _defaultPastDaysIfStartDateIsMissing = 30;

        private readonly int _dateBatchSizeForOneMinutePrices = 10;
        private readonly int _dateBatchSizeForDailyPrices = 10;

        private readonly CsvWriter? _csvWriter_1;
        private readonly DbWriter? _dbWriter_1;

        private readonly CsvWriter? _csvWriter_1440;
        private readonly DbWriter? _dbWriter_1440;

        private readonly ConcurrentBag<Task?> _taskList = [];

        private readonly string _storedProcedureName_1 = "[dbo].[sp_UpsertStocksOhlcv_1]";
        private readonly string _storedProcedureName_1440 = "[dbo].[sp_UpsertStocksOhlcv_1440]";

        private readonly string _tvpTypeName = "[dbo].[TStocksOhlcv]";

        public StocksOhlcvGenerator(Api api, IConfiguration config, IEnumerable<(string, long, string, string)>? exchangeTokenSymbolTuple, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<StocksOhlcvGenerator>();
            _api = api;
            _enabled = Convert.ToBoolean(config["StocksOhlcvGenerator:Enabled"] ?? "false");
            if (!_enabled)
                return;

            if(exchangeTokenSymbolTuple is not null && exchangeTokenSymbolTuple.Any())
                _exchangeTokenSymbolTuple = exchangeTokenSymbolTuple.Select(t => {
                    if (Enum.TryParse<Exchange>(t.Item1, out Exchange exch))
                        return (exch, t.Item2, t.Item4);
                    return default;
                }).OrderBy(a => a.Item1).ThenBy(a => a.Item3).ToList();

            var dailyPricesEnabled = Convert.ToBoolean(config["StocksOhlcvGenerator:DailyPricesEnabled"] ?? "false");
            var oneMinutePricesEnabled = Convert.ToBoolean(config["StocksOhlcvGenerator:OneMinutePricesEnabled"] ?? "false");

            var writeToDbEnabled = Convert.ToBoolean(config["StocksOhlcvGenerator:WriteToDb:Enabled"] ?? "false");
            if (writeToDbEnabled)
            {
                var connectionString = config["Database:ConnectionString"] ?? string.Empty;
                var dbChannelCapacity = Convert.ToInt32(config["StocksOhlcvGenerator:WriteToDb:ChannelCapacity"] ?? "5000");
                var writeBatchSize = Convert.ToInt32(config["StocksOhlcvGenerator:WriteToDb:WriteBatchSizeInDb"] ?? "5000");

                if (oneMinutePricesEnabled)
                    _dbWriter_1 = new DbWriter(connectionString, writeBatchSize, dbChannelCapacity, nameof(StocksOhlcvGenerator) + "_1", loggerFactory);
                if (dailyPricesEnabled)
                    _dbWriter_1440 = new DbWriter(connectionString, writeBatchSize, dbChannelCapacity, nameof(StocksOhlcvGenerator) + "_1440", loggerFactory);
            }

            var writeToFileEnabled = Convert.ToBoolean(config["StocksOhlcvGenerator:WriteToFile:Enabled"] ?? "false");
            if (writeToFileEnabled)
            {
                var fileChannelCapacity = Convert.ToInt32(config["StocksOhlcvGenerator:WriteToFile:ChannelCapacity"] ?? "5000");
                var filePath = config["StocksOhlcvGenerator:WriteToFile:CsvFilePath"] ?? string.Empty;
                if (string.IsNullOrEmpty(filePath))
                    throw new ArgumentNullException("StocksOhlcvGenerator:WriteToFile:CsvFilePath is not configured.");

                filePath = Path.GetFullPath(filePath);
                if (oneMinutePricesEnabled)
                    _csvWriter_1 = new CsvWriter(Path.GetDirectoryName(filePath)! + "_1", fileChannelCapacity, nameof(StocksOhlcvGenerator) + "_1", loggerFactory);

                if (dailyPricesEnabled)
                    _csvWriter_1440 = new CsvWriter(Path.GetDirectoryName(filePath)! + "_1440", fileChannelCapacity, nameof(StocksOhlcvGenerator) + "_1440", loggerFactory);
            }

            _defaultPastDaysIfStartDateIsMissing = Convert.ToInt32(config["StocksOhlcvGenerator:DefaultPastDaysIfStartDateIsMissing"] ?? "30");
            _startDate = DateTime.Now.Date.GetBusinessDaysAgo(_defaultPastDaysIfStartDateIsMissing).Date;
            var startDateStringFromConfig = config["StocksOhlcvGenerator:StartDate"];
            if (!string.IsNullOrEmpty(startDateStringFromConfig))
                _startDate = DateTime.Parse(startDateStringFromConfig, CultureInfo.InvariantCulture).Date;

            _api = api;

            _dateBatchSizeForOneMinutePrices = Convert.ToInt32(config["StocksOhlcvGenerator:DateBatchSizeForOneMinutePrices"] ?? "1");
            _dateBatchSizeForDailyPrices = Convert.ToInt32(config["StocksOhlcvGenerator:DateBatchSizeForDailyPrices"] ?? "1");
        }

        public async Task GenerateAndLoad()
        {
            if (!_enabled)
            {
                _logger.LogInformation("StocksOhlcvGenerator is disabled in config.");
                return;
            }

            if (_exchangeTokenSymbolTuple is null || !_exchangeTokenSymbolTuple.Any())
            {
                _logger.LogWarning("NOK: No stocks tuple for the given exchanges. Unable to fetch OHLCV data.");
                return;
            }

            var stopWatch = new Stopwatch();
            try
            {
                stopWatch.Start();
                List<Task?> tasks = [];
                tasks.Add(GenerateOHLCVCsv_1());
                //tasks.Add(GenerateOHLCVCsv_1440());

                await Utility.WhenAllSafe([.. tasks]);
            }
            catch (AggregateException ae)
            {
                _logger.LogError("\n--- One or more tasks failed: ---");
                foreach (var ex in ae.Flatten().InnerExceptions)
                {
                    _logger.LogError(ex, "  Error: {ex.TypeName} - {ex.Message}", ex.GetType().Name, ex.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "\n--- An unexpected error occurred: ---");
                _logger.LogCritical("  Error: {ex.TypeName} - {ex.Message}", ex.GetType().Name, ex.Message);
            }
            finally
            {
                _taskList.Add(_csvWriter_1?.WriteComplete());
                _taskList.Add(_dbWriter_1?.WriteComplete());

                _taskList.Add(_csvWriter_1440?.WriteComplete()); // Signal completion
                _taskList.Add(_dbWriter_1440?.WriteComplete()); // Signal completion

                await Utility.WhenAllSafe([.. _taskList]);
                _logger.LogInformation("=========== [{StockOhlcv}] Done. Took : {msg}", nameof(StocksOhlcvGenerator), stopWatch.StopAndLog());
            }
        }
        
        private async Task GenerateOHLCVCsv_1440()
        {
            int cnt = 0;
            foreach (var exchangeTokenSymbolBatch in _exchangeTokenSymbolTuple.Chunk(150))
            {
                _taskList.Add(Task.Run(async () =>
                {
                    foreach (var exchangeTokenSymbol in exchangeTokenSymbolBatch)
                    {
                        var startDate = _startDate;
                        var endDate = startDate.AddDays(_dateBatchSizeForDailyPrices);
                        do
                        {                            
                            try
                            {
                                var (resp, msg) = await _api.MarketInfo.GetEodChartDataAsync(exchangeTokenSymbol.Item1, exchangeTokenSymbol.Item3, startDate, endDate);
                                msg = $"{exchangeTokenSymbol.Item1} {exchangeTokenSymbol.Item3}({exchangeTokenSymbol.Item2}) OHLCV data ({(int)ChartInterval.Daily} min) for start date {startDate} and end date {endDate}. {msg}";

                                if (Interlocked.Increment(ref cnt) % 500 == 0)
                                {
                                    _logger.LogInformation("Processed {exchangeTokenSymbol.Item1} {cnt}/{total} stocks for 1440 minute OHLCV data.", exchangeTokenSymbol.Item1, cnt, _exchangeTokenSymbolTuple.Count());
                                    //await Task.Delay(1000);
                                }

                                _logger.LogDebug("Called API for : {msg}", msg);
                                if (resp is null)
                                {
                                    _logger.LogError("NOK: {msg}", msg);
                                    continue;
                                }

                                var headerAndContents = GenerateHeaderAndContents(resp.Data.Candles, exchangeTokenSymbol.Item2);
                                if (_csvWriter_1440 is not null)
                                {
                                    string newFile = $"{exchangeTokenSymbol.Item1}_{exchangeTokenSymbol.Item3}({exchangeTokenSymbol.Item2})_{(int)ChartInterval.Daily}_{startDate:yyyyMMdd}.csv";
                                    newFile = Path.Combine(exchangeTokenSymbol.Item1.ToString(), exchangeTokenSymbol.Item3, newFile);

                                    var csvChannelObject = new CsvChannelObject
                                    {
                                        FileName = newFile,
                                        Header = headerAndContents.Item1,
                                        Records = headerAndContents.Item2
                                    };
                                    await _csvWriter_1440.WriteCsvAsync(csvChannelObject);
                                }

                                if (_dbWriter_1440 is not null)
                                {
                                    var dbChannelObject = new DbChannelObject
                                    {
                                        TvpName = _tvpTypeName,
                                        StoredProcedureName = _storedProcedureName_1440,
                                        Records = ToDataTable(headerAndContents.Item3)
                                    };
                                    await _dbWriter_1440.WriteDbAsync(dbChannelObject);
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "An error occurred while generating the CSV: {ex.Message}", ex.Message);
                            }
                            finally
                            {
                                startDate = endDate;
                                endDate = startDate.AddDays(_dateBatchSizeForOneMinutePrices);
                            }
                        } while (startDate <= _endDate );
                    }
                }));
            }
            await Utility.WhenAllSafe([.. _taskList]);
        }
        private async Task GenerateOHLCVCsv_1()
        {
            int cnt = 0;
            foreach (var exchangeTokenSymbolBatch in _exchangeTokenSymbolTuple.Chunk(150))
            {
                _taskList.Add(Task.Run(async () =>
                {
                    foreach (var exchangeTokenSymbol in exchangeTokenSymbolBatch)
                    {
                        var startDate = _startDate;
                        var endDate = startDate.AddDays(_dateBatchSizeForOneMinutePrices);
                        do
                        {
                            try
                            {
                                var (resp, msg) = await _api.MarketInfo.GetTimePriceDataAsync(exchangeTokenSymbol.Item1, exchangeTokenSymbol.Item3, startDate, endDate, ChartInterval.One);
                                msg = $"{exchangeTokenSymbol.Item1} {exchangeTokenSymbol.Item3}({exchangeTokenSymbol.Item2}) OHLCV data ({(int)ChartInterval.One} min) for start date {startDate} and end date {endDate}. {msg}";

                                if (Interlocked.Increment(ref cnt) % 500 == 0)
                                {
                                    _logger.LogInformation("Processed {exchangeTokenSymbol.Item1} {cnt}/{total} stocks for 1 minute OHLCV data.", exchangeTokenSymbol.Item1, cnt, _exchangeTokenSymbolTuple.Count());
                                    //await Task.Delay(1000);
                                }
                                
                                _logger.LogDebug("Calling API for : {msg}", msg);
                                if (resp is null || resp.Errors.Count != 0)
                                {
                                    _logger.LogError("NOK: {msg} : {error}", msg, JsonConvert.SerializeObject(resp));
                                    continue;
                                }

                                var headerAndContents = GenerateHeaderAndContents(resp.Data.Candles, exchangeTokenSymbol.Item2);
                                string newFile = $"{exchangeTokenSymbol.Item1}_{exchangeTokenSymbol.Item3}({exchangeTokenSymbol.Item2})_{(int)ChartInterval.One}_{startDate:yyyyMMdd}.csv";
                                newFile = Path.Combine(exchangeTokenSymbol.Item1.ToString(), exchangeTokenSymbol.Item3, newFile);

                                var csvChannelObject = new CsvChannelObject
                                {
                                    FileName = newFile,
                                    Header = headerAndContents.Item1,
                                    Records = headerAndContents.Item2
                                };
                                if (_csvWriter_1 is not null)
                                    await _csvWriter_1.WriteCsvAsync(csvChannelObject);
                                var dbChannelObject = new DbChannelObject
                                {
                                    TvpName = _tvpTypeName,
                                    StoredProcedureName = _storedProcedureName_1,
                                    Records = ToDataTable(headerAndContents.Item3)
                                };
                                if (_dbWriter_1 is not null)
                                    await _dbWriter_1.WriteDbAsync(dbChannelObject);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "An error occurred while generating the CSV: {ex.Message}", ex.Message);
                            }
                            finally
                            {
                                startDate = endDate;
                                endDate = startDate.AddDays(_dateBatchSizeForOneMinutePrices);
                            }
                        } while (startDate <= _endDate);
                    }
                }));
            }
            await Utility.WhenAllSafe([.. _taskList]);
        }      

        private static (string, IEnumerable<string>, IEnumerable<(long, PriceCandle)>) GenerateHeaderAndContents(IEnumerable<PriceCandle> objects, long token)
        {
            var joinedDataHeader = "Token,StartDateTime,Open,High,Low,Close,Volume";

            IEnumerable<(long, PriceCandle)> obj = objects.Select(val => (token, new PriceCandle
            {
                StartTimeStamp = val.StartTimeStamp,
                Open = val.Open,
                High = val.High,
                Low = val.Low,
                Close = val.Close,
                Volume = (long)val.Volume
            }));

            List<string> lines = [];

            foreach (var ohlcv in objects)
                lines.Add($"{token},{ohlcv.StartTimeStamp},{ohlcv.Open},{ohlcv.High},{ohlcv.Low},{ohlcv.Close},{ohlcv.Volume}");

            return (joinedDataHeader, lines, obj);
        }

        private static DataTable ToDataTable(IEnumerable<(long, PriceCandle)> objects)
        {
            var table = new DataTable();
            table.Columns.Add("Token", typeof(int));
            table.Columns.Add("StartDateTime", typeof(DateTime));
            table.Columns.Add("Open", typeof(decimal));
            table.Columns.Add("High", typeof(decimal));
            table.Columns.Add("Low", typeof(decimal));
            table.Columns.Add("Close", typeof(decimal));
            table.Columns.Add("Volume", typeof(decimal));

            foreach (var (token, obj) in objects)
                table.Rows.Add(token, obj.StartTimeStamp, obj.Open, obj.High, obj.Low, obj.Close, obj.Volume);

            return table;
        }
    }
}
