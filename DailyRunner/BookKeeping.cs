using DailyRunner.Helpers;
using FlatTrade;
using FlatTrade.Common.Helpers;
using FlatTrade.Common.Types.Base;
using FlatTrade.HoldingsManager;
using FlatTrade.MarketInfoManager;
using FlatTrade.OrderManager;
using FlatTrade.TradeManager;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Data;
using System.Diagnostics;

namespace DailyRunner
{
    internal class FlattenedHoldings
    {
        public long HoldingQuantity { get; set; }
        public long NonPoaDisplayQuantity { get; set; }
        public long NonPoaDisplayT1Quantity { get; set; }
        public long BeneficiaryQuantity { get; set; }
        public long BrokerEquityPledgedAsCollateralQuantity { get; set; }
        public long BrokerForexPledgedAsCollateralQuantity { get; set; }
        public long BrokerAllMarketPledgedAsCollateralQuantity { get; set; }
        public long BrokerDerivativeMarketPledgedAsCollateralQuantity { get; set; }
        public long BuyTodaySellTommorrowQuantity { get; set; }
        public long HoldingQuantityUsedToday { get; set; }
        public long DpHoldingQuantity { get; set; }
        public decimal AvgPriceUploadedAlongWithHoldings { get; set; }
        public decimal HairCutPercOnPledgedSecurities { get; set; }
        public decimal TodaySellAmount { get; set; }
        public ProductName ProductDisplayName { get; set; }
        public ProductType ProductType { get; set; }
        public long TradeQuantity { get; set; }
        public long ExchangePendingInstructionDoneQuantity { get; set; }        
        public string Exchange1 { get; set; } = string.Empty;
        public string TradingSymbol1 { get; set; } = string.Empty;
        public long Token1 { get; set; }
        public string Exchange2 { get; set; } = string.Empty;
        public string TradingSymbol2 { get; set; } = string.Empty;
        public long Token2 { get; set; }
    }
    internal class BookKeeping
    {
        private readonly ILogger<BookKeeping> _logger;
        private readonly Api _api;
        private readonly bool _enabled;

        private readonly string _orderBookStoredProcedureName = "[dbo].[sp_UpsertOrderBook]";
        private readonly string _orderBookTvpTypeName = "[dbo].[TOrderBook]";

        private readonly string _tradeBookStoredProcedureName = "[dbo].[sp_UpsertTradeBook]";
        private readonly string _tradeBookTvpTypeName = "[dbo].[TTradeBook]";

        private readonly string _singleOrderHistoryStoredProcedureName = "[dbo].[sp_UpsertSingleOrderHistory]";
        private readonly string _singleOrderHistoryTvpTypeName = "[dbo].[TSingleOrderHistory]";

        private readonly string _equityMarginStoredProcedureName = "[dbo].[sp_UpsertMarginEquity]";
        private readonly string _equityMarginTvpTypeName = "[dbo].[TMarginEquity]";

        private readonly string _equityHoldingsStoredProcedureName = "[dbo].[sp_UpsertHoldings]";
        private readonly string _equityHoldingsTvpTypeName = "[dbo].[THoldings]";

        private readonly string _equityPositionBookStoredProcedureName = "[dbo].[sp_UpsertPositions]";
        private readonly string _equityPositionBookTvpTypeName = "[dbo].[TPositions]";        

        private readonly ConcurrentBag<Task?> _taskList = [];
        private readonly CsvWriter? _csvWriter;
        private readonly DbWriter? _dbWriter;

        public BookKeeping(Api api, IConfiguration config, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<BookKeeping>();
            _api = api;
            _enabled = Convert.ToBoolean(config["BookKeeping:Enabled"] ?? "false");
            if (!_enabled)
                return;

            var writeToDbEnabled = Convert.ToBoolean(config["BookKeeping:WriteToDb:Enabled"] ?? "false");
            if (writeToDbEnabled)
            {
                var connectionString = config["Database:ConnectionString"] ?? string.Empty;
                var dbChannelCapacity = Convert.ToInt32(config["BookKeeping:WriteToDb:ChannelCapacity"] ?? "5000");
                var writeBatchSize = Convert.ToInt32(config["BookKeeping:WriteToDb:WriteBatchSizeInDb"] ?? "5000");

                _dbWriter = new DbWriter(connectionString, writeBatchSize, dbChannelCapacity, nameof(BookKeeping), loggerFactory);
            }

            var writeToFileEnabled = Convert.ToBoolean(config["BookKeeping:WriteToFile:Enabled"] ?? "false");
            if (writeToFileEnabled)
            {
                var fileChannelCapacity = Convert.ToInt32(config["BookKeeping:WriteToFile:ChannelCapacity"] ?? "5000");
                var filePath = config["BookKeeping:WriteToFile:CsvFilePath"] ?? string.Empty;
                if (string.IsNullOrEmpty(filePath))
                    throw new ArgumentNullException("BookKeeping:WriteToFile:CsvFilePath is not configured.");

                filePath = Path.GetFullPath(filePath);
                _csvWriter = new CsvWriter(Path.GetDirectoryName(filePath)!, fileChannelCapacity, nameof(BookKeeping), loggerFactory);
            }

            _api = api;
        }
        public async Task GenerateAndLoad()
        {
            if (!_enabled)
            {
                _logger.LogInformation("BookKeeping is disabled in config.");
                return;
            }

            var stopWatch = new Stopwatch();
            try
            {
                stopWatch.Start();

                _taskList.Add(GenerateBookKeeping());
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

                _logger.LogInformation("=========== [{BookKeeping}] Done. Took : {msg}", nameof(BookKeeping), stopWatch.StopAndLog());
            }
        }

        private async Task<IEnumerable<OrderBookResponse>> GetOrderBookFromServerAsync()
        {

            var (orderBook, mesg) = await _api.Order.GetOrderBookAsync();
            if (orderBook is null)
            {
                if (mesg != Constants.StatusOk)
                    _logger.LogError("Error while fetching orders : {mesg}", mesg);
                else
                    _logger.LogInformation("No orders found: {mesg}", mesg);
            }
            return orderBook ?? [];
        }

        private async Task WriteOrderBookResponse(IEnumerable<OrderBookResponse> dataSet)
        {
            var (header, resultSet) = Utility.ToCsv(dataSet);           

            if (_csvWriter is not null)
            {
                var csvResult = new CsvChannelObject
                {
                    FileName = $"OrderBook\\OrderBook_{DateTime.Now:yyyyMMdd}.csv",
                    Header = header,
                    Records = resultSet
                };
                await _csvWriter.WriteCsvAsync(csvResult);
            }
             
            if (_dbWriter is not null)
            {
                var dbResult = new DbChannelObject
                {
                    TvpName = _orderBookTvpTypeName,
                    StoredProcedureName = _orderBookStoredProcedureName,
                    Records = Utility.ToDataTable(dataSet)
                };
                await _dbWriter.WriteDbAsync(dbResult);
            }
        }

        private async Task<IEnumerable<TradeBookResponse>> GetTradeBookFromServerAsync()
        {

            var (tradeBook, mesg) = await _api.Trade.GetTradeBookAsync();
            if (tradeBook is null)
            {
                if (mesg != Constants.StatusOk)
                    _logger.LogError("Error while fetching trades : {mesg}", mesg);
                else
                    _logger.LogInformation("No trades found: {mesg}", mesg);
            }
            return tradeBook ?? [];
        }

        private async Task WriteTradeBookResponse(IEnumerable<TradeBookResponse> dataSet)
        {
            var (header, resultSet) = Utility.ToCsv(dataSet);

            if (_csvWriter is not null)
            {
                var csvResult = new CsvChannelObject
                {
                    FileName = $"TradeBook\\TradeBook_{DateTime.Now:yyyyMMdd}.csv",
                    Header = header,
                    Records = resultSet
                };
                await _csvWriter.WriteCsvAsync(csvResult);
            }
                         
            if (_dbWriter is not null)
            {
                var dbResult = new DbChannelObject
                {
                    TvpName = _tradeBookTvpTypeName,
                    StoredProcedureName = _tradeBookStoredProcedureName,
                    Records = Utility.ToDataTable(dataSet)
                };
                await _dbWriter.WriteDbAsync(dbResult);
            }
        }

        public async Task<IEnumerable<SingleOrderHistoryResponse>> GetSingleOrderHistoryFromServerAsync(long norenOrderNumber)
        {
            var (orderHistory, mesg) = await _api.Order.GetSingleOrderHistoryAsync(norenOrderNumber);
            if (orderHistory is null)
            {
                if (mesg != Constants.StatusOk)
                    _logger.LogError("Error while fetching order history : {mesg}", mesg);
                else
                    _logger.LogInformation("No order history found: {mesg}", mesg);
            }
            return orderHistory ?? [];
        }

        private async Task WriteSingleOrderHistory(IEnumerable<SingleOrderHistoryResponse> dataSet)
        {
            var (header, resultSet) = Utility.ToCsv(dataSet);
            
            if (_csvWriter is not null)
            {
                var csvResult = new CsvChannelObject
                {
                    FileName = $"SingleOrderHistory\\SingleOrderHistory_{DateTime.Now:yyyyMMdd}.csv",
                    Header = header,
                    Records = resultSet
                };
                await _csvWriter.WriteCsvAsync(csvResult);
            }

            if (_dbWriter is not null)
            {
                var dbResult = new DbChannelObject
                {
                    TvpName = _singleOrderHistoryTvpTypeName,
                    StoredProcedureName = _singleOrderHistoryStoredProcedureName,
                    Records = Utility.ToDataTable(dataSet)
                };
                await _dbWriter.WriteDbAsync(dbResult);
            }
        }

        private async Task GenerateBookKeeping()
        {
            var orderBookResponse = await GetOrderBookFromServerAsync();
            _logger.LogInformation("[BookKeeping-1] Fetched {orderBookResponseCount} orders from the order book.", orderBookResponse.Count());

            List<SingleOrderHistoryResponse> singleOrderHistoryList = [];            
            if (orderBookResponse is not null && orderBookResponse.Any())
            {
                _taskList.Add(WriteOrderBookResponse(orderBookResponse));

                var tasks = orderBookResponse
                    .Select(order => GetSingleOrderHistoryFromServerAsync(order.NorenOrderNumber)).ToList();

                // Run all tasks concurrently
                var results = await Task.WhenAll(tasks);

                // Flatten results and add to your list
                foreach (var singleOrderHistory in results)
                {
                    if (singleOrderHistory != null && singleOrderHistory.Any())
                        singleOrderHistoryList.AddRange(singleOrderHistory);
                }
            }

            _logger.LogInformation("[BookKeeping-2] Fetched {orderHistoryCount} single history orders.", singleOrderHistoryList.Count);
            if (singleOrderHistoryList is not null && singleOrderHistoryList.Count != 0)
                _taskList.Add(WriteSingleOrderHistory(singleOrderHistoryList));


            var tradeBookResponse = await GetTradeBookFromServerAsync();
            _logger.LogInformation("[BookKeeping-3] Fetched {tradeBookResponseCount} trades from trade book.", tradeBookResponse.Count());
            if (tradeBookResponse is not null && tradeBookResponse.Any())
                _taskList.Add(WriteTradeBookResponse(tradeBookResponse));


            var marginEquitiesResponse = await GetMarginEquitiesFromServerAsync();
            _logger.LogInformation("[BookKeeping-4] Fetched {marginEquitiesResponse} margin symbols from margin equity calculator webpage.", marginEquitiesResponse.Count());
            if (marginEquitiesResponse is not null && marginEquitiesResponse.Any())
                _taskList.Add(WriteMarginEquitiesResponse(marginEquitiesResponse));

            var holdingResponse = await GetHoldingsFromServerAsync();
            _logger.LogInformation("[BookKeeping-5] Fetched {holdingResponseCount} holdings.", holdingResponse.Count());
            if (holdingResponse is not null && holdingResponse.Any())
                _taskList.Add(WriteHoldingsResponse(holdingResponse));

            var positionBookResponse = await GetPositionBookFromServerAsync();
            _logger.LogInformation("[BookKeeping-6] Fetched {positionBookResponse} positions.", positionBookResponse.Count());
            if (positionBookResponse is not null && positionBookResponse.Any())
                _taskList.Add(WritePositionBookResponse(positionBookResponse));
        }

        private async Task<IEnumerable<PositionBookResponse>> GetPositionBookFromServerAsync()
        {
            //var currDateTime = DateTime.UtcNow.ToLocalTime();
            //var StartHourOfCurrentDate = currDateTime.Date;
            //var EndHourOfCurrentDate = StartHourOfCurrentDate.Date.AddHours(9);
            //StartHourOfCurrentDate = EndHourOfCurrentDate.Date.AddHours(-10).Date.AddMinutes(50);

            //if (currDateTime >= StartHourOfCurrentDate && currDateTime <= EndHourOfCurrentDate)
            //{
            //    _logger.LogWarning("Position book is not allowed to execute between {StartHourOfCurrentDate} and {EndHourOfCurrentDate}.", StartHourOfCurrentDate, EndHourOfCurrentDate);
            //    return [];
            //}

            var (resp, mesg) = await _api.Trade.GetPositionBookAsync();
            if (resp is null)
            {
                if (mesg != Constants.StatusOk)
                    _logger.LogError("Error while fetching positions : {mesg}", mesg);
                else
                    _logger.LogInformation("No positions found: {mesg}", mesg);
            }
            return resp ?? [];
        }

        private async Task<IEnumerable<HoldingsResponse>> GetHoldingsFromServerAsync()
        {
            var (resp, mesg) = await _api.Holdings.GetHoldingsAsync();
            if (resp is null)
            {
                if (mesg != Constants.StatusOk)
                    _logger.LogError("Error while fetching holdings : {mesg}", mesg);
                else
                    _logger.LogInformation("No holdings found: {mesg}", mesg);
            }
            return resp ?? [];
        }

        private async Task<IEnumerable<MarginEquityResponse>> GetMarginEquitiesFromServerAsync()
        {
            var (resp, mesg) = await _api.MarketInfo.GetMarginCalculatorEquitiesAsync();
            if (resp is null)
            {
                if (mesg != Constants.StatusOk)
                    _logger.LogError("Error while fetching equity margin : {mesg}", mesg);
                else
                    _logger.LogInformation("No equity margin found: {mesg}", mesg);
            }
            return resp ?? [];
        }

        private async Task WriteMarginEquitiesResponse(IEnumerable<MarginEquityResponse> dataSet)
        {
            var (header, resultSet) = Utility.ToCsv(dataSet);

            if (_csvWriter is not null)
            {
                var csvResult = new CsvChannelObject
                {
                    FileName = $"Margin\\EquityMargin_{DateTime.Now:yyyyMMdd}.csv",
                    Header = header,
                    Records = resultSet
                };
                await _csvWriter.WriteCsvAsync(csvResult);
            }

            if (_dbWriter is not null)
            {
                var dbResult = new DbChannelObject
                {
                    TvpName = _equityMarginTvpTypeName,
                    StoredProcedureName = _equityMarginStoredProcedureName,
                    Records = Utility.ToDataTable(dataSet)
                };
                await _dbWriter.WriteDbAsync(dbResult);
            }
        }

        public static IEnumerable<FlattenedHoldings> ConvertHoldingsToCsvPivoted(IEnumerable<HoldingsResponse> holdings)
        {
            // Determine maximum number of ExchangeSymbolResponse per Holding
            //int maxSymbols = holdings.Any() ? holdings.Max(h => h.ExchangeSymbolResponse?.Count ?? 0) : 0;

            // Create flattened structure
            return [..holdings.Select(h =>
            {
                return new FlattenedHoldings
                {
                    HoldingQuantity = h.HoldingQuantity,
                    NonPoaDisplayQuantity = h.NonPoaDisplayQuantity,
                    NonPoaDisplayT1Quantity = h.NonPoaDisplayT1Quantity,
                    BeneficiaryQuantity = h.BeneficiaryQuantity,
                    BrokerEquityPledgedAsCollateralQuantity = h.BrokerEquityPledgedAsCollateralQuantity,
                    BrokerForexPledgedAsCollateralQuantity = h.BrokerForexPledgedAsCollateralQuantity,
                    BrokerAllMarketPledgedAsCollateralQuantity = h.BrokerAllMarketPledgedAsCollateralQuantity,
                    BrokerDerivativeMarketPledgedAsCollateralQuantity = h.BrokerDerivativeMarketPledgedAsCollateralQuantity,
                    BuyTodaySellTommorrowQuantity = h.BuyTodaySellTommorrowQuantity,
                    HoldingQuantityUsedToday = h.HoldingQuantityUsedToday,
                    DpHoldingQuantity = h.DpHoldingQuantity,
                    AvgPriceUploadedAlongWithHoldings = h.AvgPriceUploadedAlongWithHoldings,
                    HairCutPercOnPledgedSecurities = h.HairCutPercOnPledgedSecurities,
                    TodaySellAmount = h.TodaySellAmount,
                    ProductDisplayName = h.ProductDisplayName,
                    ProductType = h.ProductType,
                    TradeQuantity = h.TradeQuantity,
                    ExchangePendingInstructionDoneQuantity = h.ExchangePendingInstructionDoneQuantity,

                    Exchange1 = h.ExchangeSymbolResponse.Count != 0 ? h.ExchangeSymbolResponse[0].Exchange.ToString() : string.Empty,
                    TradingSymbol1 = h.ExchangeSymbolResponse.Count != 0 ? h.ExchangeSymbolResponse[0].TradingSymbol: string.Empty,
                    Token1 = h.ExchangeSymbolResponse.Count != 0 ? h.ExchangeSymbolResponse[0].Token : 0,

                    Exchange2 = h.ExchangeSymbolResponse.Count != 0 ? h.ExchangeSymbolResponse[1].Exchange.ToString() : string.Empty,
                    TradingSymbol2 = h.ExchangeSymbolResponse.Count != 0 ? h.ExchangeSymbolResponse[1].TradingSymbol : string.Empty,
                    Token2 = h.ExchangeSymbolResponse.Count != 0 ? h.ExchangeSymbolResponse[1].Token : 0,
                };
            })];
        }

        private async Task WriteHoldingsResponse(IEnumerable<HoldingsResponse> dataSet)
        {
            var newDataSet = ConvertHoldingsToCsvPivoted(dataSet);
            var (header, resultSet) = Utility.ToCsv(newDataSet);

            if (_csvWriter is not null)
            {
                var csvResult = new CsvChannelObject
                {
                    FileName = $"Holdings\\Holdings_{DateTime.Now:yyyyMMdd}.csv",
                    Header = header,
                    Records = resultSet
                };
                await _csvWriter.WriteCsvAsync(csvResult);
            }

            if (_dbWriter is not null)
            {
                var dbResult = new DbChannelObject
                {
                    TvpName = _equityHoldingsTvpTypeName,
                    StoredProcedureName = _equityHoldingsStoredProcedureName,
                    Records = Utility.ToDataTable(newDataSet)
                };
                await _dbWriter.WriteDbAsync(dbResult);
            }
        }

        private async Task WritePositionBookResponse(IEnumerable<PositionBookResponse> dataSet)
        {
            var (header, resultSet) = Utility.ToCsv(dataSet);

            if (_csvWriter is not null)
            {
                var csvResult = new CsvChannelObject
                {
                    FileName = $"Positions\\Positions_{DateTime.Now:yyyyMMdd}.csv",
                    Header = header,
                    Records = resultSet
                };
                await _csvWriter.WriteCsvAsync(csvResult);
            }

            if (_dbWriter is not null)
            {
                var dbResult = new DbChannelObject
                {
                    TvpName = _equityPositionBookTvpTypeName,
                    StoredProcedureName = _equityPositionBookStoredProcedureName,
                    Records = Utility.ToDataTable(dataSet)
                };
                await _dbWriter.WriteDbAsync(dbResult);
            }
        }
    }
}