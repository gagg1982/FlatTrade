using FlatTrade;
using FlatTrade.Common.Helpers;
using FlatTrade.Common.Types;
using FlatTrade.Common.Types.Base;
using FlatTrade.MarketInfoManager;
using FlatTrade.SubscriptionManager;
using FlatTrade.SubscriptionManager.Quote;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using Utility = DailyRunner.Helpers.Utility;

namespace DailyRunner
{
    internal class StocksOhlcvGenerator
    {
        private readonly ILogger<StocksOhlcvGenerator> _logger;
        private readonly Api _api;
        private readonly bool _enabled;
        private List<(Exchange, long, string)> _exchangeTokenSymbolTuple = [];

        private List<QuoteSubscriptionRequestAck> _placeHolderToGenerateCurrentDayDailyCandles = [];
        private List<(Exchange, long, string)> _placeHolderForExchangeTokenSymbolTuple = [];

        private readonly string _filePath = string.Empty;

        private readonly DateTime _startDate;
        private readonly DateTime _endDate = DateTime.Now.ToLocalTime();

        private readonly int _defaultStartDateIfMissing = 30;

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

        public StocksOhlcvGenerator(Api api, IConfiguration config, IEnumerable<(Exchange, long, string)>? exchangeTokenSymbolTuple, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<StocksOhlcvGenerator>();
            _api = api;
            _enabled = Convert.ToBoolean(config["StocksOhlcvGenerator:Enabled"] ?? "false");
            if (!_enabled)
                return;

            if(exchangeTokenSymbolTuple is not null && exchangeTokenSymbolTuple.Any())
                _exchangeTokenSymbolTuple = exchangeTokenSymbolTuple.OrderBy(a => a.Item1).ThenBy(a => a.Item3).ToList();
            _placeHolderForExchangeTokenSymbolTuple = _exchangeTokenSymbolTuple.ToList();

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

            _defaultStartDateIfMissing = Convert.ToInt32(config["StocksOhlcvGenerator:DefaultStartDateIfMissing"] ?? "30");
            _startDate = DateTime.Now.Date.AddDays(-1 * _defaultStartDateIfMissing).Date;
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
                tasks.Add(SubscribeCurrentDateOHLCVCsv_1440());
                tasks.Add(GenerateOHLCVCsv_1());
                tasks.Add(GenerateOHLCVCsv_1440());

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


        private async Task SubscribeCurrentDateOHLCVCsv_1440()
        {            
            int chunkSize = 10;
            int cnt = 0;
            var selection = _placeHolderForExchangeTokenSymbolTuple.Select(t => new KeyValuePair<Exchange, long>(t.Item1, t.Item2));
            _api.Subscription.QuoteSubscription.OnSubscriptionEvents = OnQuoteUpdates;
            foreach (var exchangeTokenSymbolBatch in selection.Chunk(chunkSize))
            {
                ++cnt;
                var resp = await _api.Subscription.QuoteSubscription.SubscribeAsync(exchangeTokenSymbolBatch);
                if (!resp)
                    _logger.LogError("SubscribeCurrentDateOHLCVCsv_1440: Unable to subscribe (count={chunkSize}) : '{batchData}'", chunkSize, JsonConvert.SerializeObject(exchangeTokenSymbolBatch));
            }
            _logger.LogInformation("SubscribeCurrentDateOHLCVCsv_1440: Total subscriptions {cnt}", cnt * chunkSize);
        }

        private async Task UnSubscribeCurrentDateOHLCVCsv_1440(Exchange exchange, long token, string tradingSymbol)
        {
            _placeHolderForExchangeTokenSymbolTuple.Remove((exchange, token, tradingSymbol));
            if(_placeHolderForExchangeTokenSymbolTuple.Count % 500 == 0)
                _logger.LogInformation("UnSubscribeCurrentDateOHLCVCsv_1440: Remaining {0}", _placeHolderForExchangeTokenSymbolTuple.Count);
            await _api.Subscription.QuoteSubscription.UnsubscribeAsync([new KeyValuePair<Exchange, long>(exchange, token)]);
        }

        private async Task GenerateOhlcvFromQuotes(QuoteSubscriptionRequestAck obj)
        {
            _placeHolderToGenerateCurrentDayDailyCandles.Add(obj);
            if (_placeHolderToGenerateCurrentDayDailyCandles.Count % 500 == 0)
                _logger.LogInformation("GenerateOhlcvFromQuotes: Current day OHLCV received {0}", _placeHolderToGenerateCurrentDayDailyCandles.Count());
            
            if (_placeHolderForExchangeTokenSymbolTuple is null || !_placeHolderForExchangeTokenSymbolTuple.Any())
            {
                var headerAndContents = GenerateHeaderAndContents(_placeHolderToGenerateCurrentDayDailyCandles);
                if (_csvWriter_1440 is not null)
                {
                    string newFile = $"CurrentDayOHLCV_{(int)ChartInterval.Daily}_{DateTime.Now:yyyyMMdd}.csv";                    

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
        }

        private async Task OnQuoteUpdates(object? _, SubscriptionType subscriptionType, string rawMessage, object? subscriptionObject)
        {
            var msg = string.Format("Message processed: '{0}'", rawMessage);

            switch (subscriptionType)
            {
                case SubscriptionType.ConnectAck:
                    _logger.LogInformation("[OnQuoteUpdates-ConnectAck] {msg}", msg);
                    await SubscribeCurrentDateOHLCVCsv_1440();
                    break;
                case SubscriptionType.SubscribeQuoteAck:
                    //_logger.LogInformation("[OnQuoteUpdates-SubscribeQuoteAck] {msg}", msg);
                    var obj = (QuoteSubscriptionRequestAck)subscriptionObject!;
                    await UnSubscribeCurrentDateOHLCVCsv_1440(obj.Exchange, obj.Token, obj.TradingSymbol);
                    await GenerateOhlcvFromQuotes(obj);
                    break;
                case SubscriptionType.UnsubscribeQuoteAck:
                    _logger.LogDebug("[OnQuoteUpdates-UnSubscribeQuoteLineAck] {msg}", msg);                    
                    break;
                case SubscriptionType.SubscribeQuoteUpdates:
                    //_logger.LogDebug("[OnQuoteUpdates-SubscribeQuoteUpdates] {msg}", msg);                    
                    break;
                default:
                    _logger.LogWarning("[OnQuoteUpdates]: unknown message type '{type}' Msg '{msg}'", subscriptionType, msg);
                    break;
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
                                    //Thread.Sleep(1000);
                                }

                                _logger.LogDebug("Called API for : {msg}", msg);
                                if (resp is null || !resp.Any())
                                {
                                    _logger.LogError("NOK: {msg}", msg);
                                    continue;
                                }

                                var headerAndContents = GenerateHeaderAndContents(resp, exchangeTokenSymbol.Item2);
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
                                    //Thread.Sleep(1000);
                                }
                                
                                _logger.LogDebug("Calling API for : {msg}", msg);
                                if (resp is null || !resp.Any())
                                {
                                    _logger.LogError("NOK: {msg}", msg);
                                    continue;
                                }

                                var headerAndContents = GenerateHeaderAndContents(resp, exchangeTokenSymbol.Item2);
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

        private static (string, IEnumerable<string>, IEnumerable<(long, PriceCandle)>) GenerateHeaderAndContents(IEnumerable<QuoteSubscriptionRequestAck> objects)
        {
            var joinedDataHeader = "Token,StartDateTime,Open,High,Low,Close,Volume";

            IEnumerable<(long, PriceCandle)> obj = objects.Where(val => val.LastTradeDateTime != DateTime.MinValue &&
                                                            val.DayClosePrice != decimal.MinValue &&
                                                            val.DayHighPrice != decimal.MinValue &&
                                                            val.DayLowPrice != decimal.MaxValue &&
                                                            val.DayOpenPrice != decimal.MinValue)
                                             .Select(val => (val.Token, new PriceCandle
                                                             {
                                                                 StartTimeStamp = val.LastTradeDateTime.Date,
                                                                 Open = val.DayOpenPrice,
                                                                 High = val.DayHighPrice,
                                                                 Low = val.DayLowPrice,
                                                                 Close = val.LastTradePrice != decimal.MinValue ? val.LastTradePrice: val.DayClosePrice,
                                                                 Volume = val.DayVolume
                                                             }));

            List<string> lines = [];

            foreach (var ohlcv in objects)
                lines.Add($"{ohlcv.Token},{ohlcv.LastTradeDateTime.Date},{ohlcv.DayOpenPrice},{ohlcv.DayHighPrice},{ohlcv.DayLowPrice},{ohlcv.DayClosePrice},{ohlcv.DayVolume}");

            return (joinedDataHeader, lines, obj);
        }

        private static (string, IEnumerable<string>, IEnumerable<(long, PriceCandle)>) GenerateHeaderAndContents(IEnumerable<EodChartDataResponse> objects, long token)
        {
            var joinedDataHeader = "Token,StartDateTime,Open,High,Low,Close,Volume";

            List<string> lines = [];
            IEnumerable<(long,PriceCandle)> obj = objects.Select(val => (token, new PriceCandle { StartTimeStamp = val.StartDateTime,
                                                                                          Open= val.OpenPrice,
                                                                                          High= val.HighPrice,
                                                                                          Low = val.LowPrice,
                                                                                          Close = val.ClosePrice,
                                                                                          Volume = (long)val.Volume }));
            foreach (var ohlcv in objects)
                lines.Add($"{token},{ohlcv.StartDateTime},{ohlcv.OpenPrice},{ohlcv.HighPrice},{ohlcv.LowPrice},{ohlcv.ClosePrice},{ohlcv.Volume}");

            return (joinedDataHeader, lines, obj);
        }

        private static (string, IEnumerable<string>, IEnumerable<(long, PriceCandle)>) GenerateHeaderAndContents(IEnumerable<TimePriceDataResponse> objects, long token)
        {
            var joinedDataHeader = "Token,StartDateTime,Open,High,Low,Close,Volume";

            IEnumerable<(long, PriceCandle)> obj = objects.Select(val => (token, new PriceCandle
            {
                StartTimeStamp = val.StartDateTime,
                Open = val.OpenPrice,
                High = val.HighPrice,
                Low = val.LowPrice,
                Close = val.ClosePrice,
                Volume = (long)val.Volume
            }));        
                        
            List<string> lines = [];
            
            foreach (var ohlcv in objects)
                lines.Add($"{token},{ohlcv.StartDateTime},{ohlcv.OpenPrice},{ohlcv.HighPrice},{ohlcv.LowPrice},{ohlcv.ClosePrice},{ohlcv.Volume}");

            return (joinedDataHeader, lines, obj);
        }

        private static DataTable ToDataTable(IEnumerable<(long,PriceCandle)> objects)
        {
            var table = new DataTable();
            table.Columns.Add("Token", typeof(int));
            table.Columns.Add("StartDateTime", typeof(DateTime));
            table.Columns.Add("Open", typeof(decimal));
            table.Columns.Add("High", typeof(decimal));
            table.Columns.Add("Low", typeof(decimal));
            table.Columns.Add("Close", typeof(decimal));
            table.Columns.Add("Volume", typeof(decimal));

            foreach (var (token,obj) in objects)
                table.Rows.Add(token, obj.StartTimeStamp, obj.Open, obj.High, obj.Low, obj.Close, obj.Volume);

            return table;
        }
    }
}
