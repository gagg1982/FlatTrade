using Upstox;
using Common.Helpers;
using Upstox.MarketInfoManager;
using Upstox.Types;
using Upstox.Types.Base;
using Common.Types;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using static Common.Helpers.DataReaderHelper;
using Utility = DailyRunner.Helpers.Utility;

namespace DailyRunner.Upstox
{
    internal class MissingStockInstrumentsObject
    {
        public DateTime MissingTradeDate { get; set; }
        public long Token { get; set; }
        public string TradingSymbol { get; set; } = string.Empty;

        [Transform(typeof(EnumTransformer<Exchange>))]
        public Exchange ExchangeCode { get; set; }
    }

    internal class FillMissingOhlcv
    {
        private readonly ILogger<FillMissingOhlcv> _logger;
        private readonly Api _api;
        private readonly bool _enabled;
        private readonly DateTime _startDate;

        private readonly int _defaultPastDaysIfStartDateIsMissing = 200;

        private readonly CsvWriter? _csvWriter_1;
        private readonly DbWriter? _dbWriter_1;

        private readonly DbReader? _dbReader;

        private readonly CsvWriter? _csvWriter_1440;
        private readonly DbWriter? _dbWriter_1440;

        private readonly ConcurrentBag<Task?> _taskList = [];

        private readonly string _storedProcedureName_1 = "[dbo].[sp_UpsertStocksOhlcv_1]";
        private readonly string _storedProcedureName_1440 = "[dbo].[sp_UpsertStocksOhlcv_1440]";

        private readonly string _tvpTypeName = "[dbo].[TStocksOhlcv]";

        private readonly string _storedProcedureNameForMissingCandles_1 = "[dbo].[sp_SanityOnOneMinuteCandles_MissingDates]";
        private readonly string _storedProcedureNameForMissingCandles_1440 = "[dbo].[sp_SanityOnDailyCandles_MissingDates]";

        public FillMissingOhlcv(Api api, IConfiguration config, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<FillMissingOhlcv>();
            _api = api;
            _enabled = Convert.ToBoolean(config["FillMissingOhlcv:Enabled"] ?? "false");
            if (!_enabled)
                return;

            var connectionString = config["Database:ConnectionString"] ?? string.Empty;
            _dbReader = new(connectionString, loggerFactory);

            var dailyPricesEnabled = Convert.ToBoolean(config["FillMissingOhlcv:DailyPricesEnabled"] ?? "false");
            var oneMinutePricesEnabled = Convert.ToBoolean(config["FillMissingOhlcv:OneMinutePricesEnabled"] ?? "false");

            var writeToDbEnabled = Convert.ToBoolean(config["FillMissingOhlcv:WriteToDb:Enabled"] ?? "false");
            if (writeToDbEnabled)
            {
                var dbChannelCapacity = Convert.ToInt32(config["FillMissingOhlcv:WriteToDb:ChannelCapacity"] ?? "5000");
                var writeBatchSize = Convert.ToInt32(config["FillMissingOhlcv:WriteToDb:WriteBatchSizeInDb"] ?? "5000");

                if (oneMinutePricesEnabled)
                    _dbWriter_1 = new DbWriter(connectionString, writeBatchSize, dbChannelCapacity, nameof(FillMissingOhlcv) + "_1", loggerFactory);
                if (dailyPricesEnabled)
                    _dbWriter_1440 = new DbWriter(connectionString, writeBatchSize, dbChannelCapacity, nameof(FillMissingOhlcv) + "_1440", loggerFactory);
            }

            var writeToFileEnabled = Convert.ToBoolean(config["FillMissingOhlcv:WriteToFile:Enabled"] ?? "false");
            if (writeToFileEnabled)
            {
                var fileChannelCapacity = Convert.ToInt32(config["FillMissingOhlcv:WriteToFile:ChannelCapacity"] ?? "5000");
                var filePath = config["FillMissingOhlcv:WriteToFile:CsvFilePath"] ?? string.Empty;
                if (string.IsNullOrEmpty(filePath))
                    throw new ArgumentNullException("FillMissingOhlcv:WriteToFile:CsvFilePath is not configured.");

                filePath = Path.GetFullPath(filePath);
                if (oneMinutePricesEnabled)
                    _csvWriter_1 = new CsvWriter(Path.GetDirectoryName(filePath)! + "_1", fileChannelCapacity, nameof(FillMissingOhlcv) + "_1", loggerFactory);

                if (dailyPricesEnabled)
                    _csvWriter_1440 = new CsvWriter(Path.GetDirectoryName(filePath)! + "_1440", fileChannelCapacity, nameof(FillMissingOhlcv) + "_1440", loggerFactory);
            }

            _defaultPastDaysIfStartDateIsMissing = Convert.ToInt32(config["FillMissingOhlcv:DefaultPastDaysIfStartDateIsMissing"] ?? "200");
            _startDate = DateTime.Now.Date.AddDays(-1 * _defaultPastDaysIfStartDateIsMissing).Date;
            var startDateStringFromConfig = config["FillMissingOhlcv:StartDate"];
            if (!string.IsNullOrEmpty(startDateStringFromConfig))
                _startDate = DateTime.Parse(startDateStringFromConfig, CultureInfo.InvariantCulture).Date;

            _api = api;
        }

        private async Task<List<MissingStockInstrumentsObject>> GetMissingStockOhlcvInstruments(DateTime startDateTime, string spName)
        {
            var parameters = new Dictionary<string, object?>
            {
                { "@StartDateForOHLCVSanity", startDateTime },
                { "@TradingSymbol",  DBNull.Value }
            };

            // Call the SP and map result
            return await _dbReader!.ExecuteStoredProcedure(spName, parameters!, reader => MapReaderTo<MissingStockInstrumentsObject>(reader));
        }

        public async Task GenerateAndLoad()
        {
            if (!_enabled)
            {
                _logger.LogInformation("FillMissingOhlcv is disabled in config.");
                return;
            }
            
            var stopWatch = new Stopwatch();
            try
            {
                stopWatch.Start();
                List<Task?> tasks = [];

                tasks.Add(FillMissingOHLCVCsv_1());
                tasks.Add(FillMissingOHLCVCsv_1440());

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
                _logger.LogInformation("=========== [{FillMissingOhlcv}] Done. Took : {msg}", nameof(FillMissingOhlcv), stopWatch.StopAndLog());
            }
        }
       
        private async Task FillMissingOHLCVCsv_1440()
        {
            var missingList = await GetMissingStockOhlcvInstruments(_startDate, _storedProcedureNameForMissingCandles_1440);
            _logger.LogInformation("Total {count} missing stocks found for daily OHLCV data.", missingList.Count());
            int cnt = 0;
            foreach (var missingBatch in missingList.Chunk(50))
            {
                _taskList.Add(Task.Run(async () =>
                {
                    foreach (var missing in missingBatch)
                    {
                        var startDate = missing.MissingTradeDate.Date.AddDays(-1);
                        var endDate = missing.MissingTradeDate.Date.AddDays(1);
                        try
                        {
                            var (resp, msg) = await _api.MarketInfo.GetEodChartDataAsync(missing.ExchangeCode, missing.TradingSymbol, startDate, endDate);
                            
                            if (Interlocked.Increment(ref cnt) % 500 == 0)
                            {
                                _logger.LogInformation("Processed {cnt}/{total} stocks for 1440 minute OHLCV data.", cnt, missingList.Count());
                                //Task.Delay(1000);
                            }

                            if (resp is null)
                            {
                                msg = $"{missing.ExchangeCode} {missing.TradingSymbol}({missing.Token}) OHLCV data ({(int)ChartInterval.Daily} min) for start date {startDate} and end date {endDate}. {msg}";
                                _logger.LogError("NOK: {msg}", msg);
                                continue;
                            }

                            var headerAndContents = GenerateHeaderAndContents(resp, missing.Token);
                            if (_csvWriter_1440 is not null)
                            {
                                string newFile = $"{missing.ExchangeCode}_{missing.TradingSymbol}({missing.Token})_{(int)ChartInterval.Daily}_{startDate:yyyyMMdd}.csv";
                                newFile = Path.Combine(missing.ExchangeCode.ToString(), missing.TradingSymbol, newFile);

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
                    }
                }));
            }
            await Utility.WhenAllSafe([.. _taskList]);
        }
        private async Task FillMissingOHLCVCsv_1()
        {
            var missingList = await GetMissingStockOhlcvInstruments(_startDate, _storedProcedureNameForMissingCandles_1);
            _logger.LogInformation("Total missing stocks for 1 minute OHLCV data: {count}", missingList.Count);
            int cnt = 0;
            foreach (var missingBatch in missingList.Chunk(50))
            {
                _taskList.Add(Task.Run(async () =>
                {
                    foreach (var missing in missingBatch)
                    {
                        //fetching 3 days data, 1 previous, 1 current, 1 next day
                        var startDate = missing.MissingTradeDate.Date.AddDays(-1);
                        var endDate = missing.MissingTradeDate.Date.AddDays(1);
                        try
                        {
                            var (resp, msg) = await _api.MarketInfo.GetTimePriceDataAsync(missing.ExchangeCode, missing.TradingSymbol, startDate, endDate, ChartInterval.One);
                           
                            if (Interlocked.Increment(ref cnt) % 500 == 0)
                            {
                                _logger.LogInformation("Processed {cnt}/{total} stocks for 1 minute OHLCV data.", cnt, missingList.Count());
                                //Task.Delay(1000);
                            }

                            _logger.LogDebug("Calling API for : {msg}", msg);
                            if (resp is null)
                            {
                                msg = $"{missing.ExchangeCode} {missing.TradingSymbol}({missing.ExchangeCode}) OHLCV data ({(int)ChartInterval.One} min) for start date {startDate} and end date {endDate}. {msg}";
                                _logger.LogError("NOK: {msg}", msg);
                                continue;
                            }

                            var headerAndContents = GenerateHeaderAndContents(resp, missing.Token);
                            string newFile = $"{missing.ExchangeCode}_{missing.TradingSymbol}({missing.Token})_{(int)ChartInterval.One}_{startDate:yyyyMMdd}.csv";
                            newFile = Path.Combine(missing.ExchangeCode.ToString(), missing.TradingSymbol, newFile);

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
                    }
                }));
            }
            await Utility.WhenAllSafe([.. _taskList]);
        }

        private static (string, IEnumerable<string>, IEnumerable<(long, PriceCandle)>) GenerateHeaderAndContents(TimePriceDataResponse objects, long token)
        {
            var joinedDataHeader = "Token,StartDateTime,Open,High,Low,Close,Volume";

            IEnumerable<(long, PriceCandle)> obj = objects.Data.Candles.Select(val => (token, new PriceCandle
            {
                StartTimeStamp = val.StartTimeStamp,
                Open = val.Open,
                High = val.High,
                Low = val.Low,
                Close = val.Close,
                Volume = val.Volume
            }));

            List<string> lines = [];

            foreach (var ohlcv in objects.Data.Candles)
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
