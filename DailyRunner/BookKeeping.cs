using Common.Helpers;
using Common.Types;
using DailyRunner.Helpers;
using FlatTrade;
using FlatTrade.HoldingsManager;
using FlatTrade.MarketInfoManager;
using FlatTrade.OrderManager;
using FlatTrade.TradeManager;
using FlatTrade.Types.Base;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using static Common.Helpers.DataReaderHelper;

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
        public decimal DailyClose { get; set; }
    }

    internal class BrokerageAndTaxes 
    {
        public string TradingSymbol { get; set; } = string.Empty;
     
        [Transform(typeof(EnumTransformer<Exchange>))]
        public Exchange Exchange { get; set; }
        public decimal BrokerageAmount { get; set; }
        public decimal ClearingMemberAmount { get; set; }
        public string ExchangeOrderNumber { get; set; } = string.Empty;
        public DateTime FillDateTime { get; set; }
        public long FillId { get; set; }        
        public decimal FillPrice { get; set; }
        public long FillQuantity { get; set; }
        public decimal Gst { get; set; }
        public decimal InvestorProtectionFundTrustAmount { get; set; }
        public long NorenOrderNumber { get; set; }
        public long SnoOrderNumber { get; set; }
        public DateTime NorenTime { get; set; }

        [Transform(typeof(EnumTransformer<ProductType>))]
        public ProductType ProductType { get; set; }
        public string Remarks { get; set; } = string.Empty;
        public decimal SebiCharges { get; set; }
        public decimal ExchangeCharges { get; set; }
        public decimal SecurityTransactionTax { get; set; }
        public decimal StampDuty { get; set; }
        public long Token { get; set; }
        public decimal TotalCharges { get; set; }

        [Transform(typeof(EnumTransformer<TransactionType>))]
        public TransactionType TransactionType { get; set; }                      
        public string Url { get; set; } = string.Empty;
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

        private readonly string _equityGetTradeBookProcedureName = "[dbo].[sp_GetTradeBook]";
        private readonly string _equityUpsertBrokerageAndTaxesStoredProcedureName = "[dbo].[sp_UpsertBrokerageAndTaxes]";
        private readonly string _equityBrokerageAndTaxesTvpTypeName = "[dbo].[TBrokerageAndTaxes]";
        private readonly DateTime _equityBrokerageAndtaxesStartDate = DateTime.MinValue;

        private readonly ConcurrentBag<Task?> _taskList = [];
        private readonly CsvWriter? _csvWriter;
        private readonly DbWriter? _dbWriter;

        private readonly DbReader? _dbReader;

        public BookKeeping(Api api, IConfiguration config, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<BookKeeping>();
            _api = api;
            _enabled = Convert.ToBoolean(config["BookKeeping:Enabled"] ?? "false");
            if (!_enabled)
                return;

            var connectionString = config["Database:ConnectionString"] ?? string.Empty;
            _dbReader = new(connectionString, loggerFactory);

            var writeToDbEnabled = Convert.ToBoolean(config["BookKeeping:WriteToDb:Enabled"] ?? "false");
            if (writeToDbEnabled)
            {            
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
                    throw new InvalidDataException("BookKeeping:WriteToFile:CsvFilePath is not configured.");

                filePath = Path.GetFullPath(filePath);
                _csvWriter = new CsvWriter(Path.GetDirectoryName(filePath)!, fileChannelCapacity, nameof(BookKeeping), loggerFactory);
            }

            var startDateStringFromConfig = config["BookKeeping:CalculateBrokerageAndTaxesFrom"];
            if (!string.IsNullOrEmpty(startDateStringFromConfig))
                _equityBrokerageAndtaxesStartDate = DateTime.Parse(startDateStringFromConfig, CultureInfo.InvariantCulture).Date;            
            
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
                if (mesg != Constants.StatusOk && !mesg.Contains("no data"))
                    _logger.LogError("Error while fetching orders : {mesg}", mesg);
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
                if (mesg != Constants.StatusOk && !mesg.Contains("no data"))
                    _logger.LogError("Error while fetching trades : {mesg}", mesg);
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
                if (mesg != Constants.StatusOk && !mesg.Contains("no data"))
                    _logger.LogError("Error while fetching order history : {mesg}", mesg);
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

            var brokerageAndTaxesResponse = await UpsertBrokerageAndTaxesAsync(tradeBookResponse ?? []);
            _logger.LogInformation("[BookKeeping-7] Fetched {brokerageAndTaxesResponse} (From Db + CurrrentDay) trades to calculate tax and brokerage.", brokerageAndTaxesResponse.Count());
            if (brokerageAndTaxesResponse is not null && brokerageAndTaxesResponse.Any())
                _taskList.Add(WriteBrokerageResponse(brokerageAndTaxesResponse));
        }

        private async Task<IEnumerable<TradeBookResponse>> GetTradeBookFromDbAsync()
        {
            var parameters = new Dictionary<string, object?>
            {
                { "@StartDate", _equityBrokerageAndtaxesStartDate }
            };

            // Call the SP and map result
            return await _dbReader!.ExecuteStoredProcedure(_equityGetTradeBookProcedureName, parameters!,
                                                            reader => DataReaderHelper.MapReaderTo<TradeBookResponse>(reader));
        }

        private async Task<IEnumerable<BrokerageAndTaxes>> UpsertBrokerageAndTaxesAsync(IEnumerable<TradeBookResponse> currentTrades)
        {
            var tradesInfo = await GetTradeBookFromDbAsync();
            var result = currentTrades
                        .Concat(tradesInfo)
                        .GroupBy(x => new { x.Token, x.Exchange, x.FillDateTime, x.FillId, x.NorenOrderNumber, x.SnoOrderNumber, x.ExchangeOrderNumber, x.ExchangeTime })
                        .Select(g => g.First());

            var tasks = result.Select(trade =>
            Task.Run(async () =>
            {
                var (fees, msg) = await _api.MarketInfo.GetBrokerageAsync(
                                                                            trade.TransactionType,
                                                                            trade.Exchange,
                                                                            trade.ProductType,
                                                                            trade.TradingSymbol,
                                                                            trade.FillPrice,
                                                                            trade.FillQuantity);
                if (fees is null)
                {
                    _logger.LogError("Unable to get brokerage and tax information for {0}/{1}({2}), fill id {3}. Error: {msg}", trade.Exchange, trade.TradingSymbol, trade.Token, trade.FillId, msg);
                    return default;
                }

                return new BrokerageAndTaxes
                {                    
                    TradingSymbol = trade.TradingSymbol,
                    Exchange = trade.Exchange,
                    BrokerageAmount = fees.BrokerageAmount,
                    ClearingMemberAmount = fees.ClearingMemberAmount,
                    ExchangeOrderNumber = trade.ExchangeOrderNumber,
                    FillDateTime = trade.FillDateTime,
                    FillId = trade.FillId,
                    FillPrice = trade.Price,
                    FillQuantity = trade.Quantity,
                    Gst = fees.Gst,
                    InvestorProtectionFundTrustAmount = fees.InvestorProtectionFundTrustAmount,
                    NorenOrderNumber = trade.NorenOrderNumber,
                    SnoOrderNumber = trade.SnoOrderNumber,
                    NorenTime = trade.NorenTime,
                    ProductType = trade.ProductType,
                    Remarks = fees.Remarks,
                    SebiCharges = fees.SebiCharges,
                    ExchangeCharges = fees.ExchangeCharges,
                    SecurityTransactionTax = fees.SecurityTransactionTax,
                    StampDuty = fees.StampDuty,
                    Token = trade.Token,
                    TotalCharges = fees.TotalCharges,
                    TransactionType = trade.TransactionType,
                    Url = fees.Url
                };
            })
            );

            var results =  await Task.WhenAll(tasks);
            return results.Where(r => r != null)!;
        }
        private async Task WriteBrokerageResponse(IEnumerable<BrokerageAndTaxes> dataSet)
        {
            var (header, resultSet) = Utility.ToCsv(dataSet);

            if (_csvWriter is not null)
            {
                var csvResult = new CsvChannelObject
                {
                    FileName = $"Fees\\BrokerageAndTaxes_{DateTime.Now:yyyyMMdd}.csv",
                    Header = header,
                    Records = resultSet
                };
                await _csvWriter.WriteCsvAsync(csvResult);
            }

            if (_dbWriter is not null)
            {
                var dbResult = new DbChannelObject
                {
                    TvpName = _equityBrokerageAndTaxesTvpTypeName,
                    StoredProcedureName = _equityUpsertBrokerageAndTaxesStoredProcedureName,
                    Records = Utility.ToDataTable(dataSet)
                };
                await _dbWriter.WriteDbAsync(dbResult);
            }
        }

        private async Task<IEnumerable<PositionBookResponse>> GetPositionBookFromServerAsync()
        {
            var (resp, mesg) = await _api.Trade.GetPositionBookAsync();
            if (resp is null)
            {
                if (mesg != Constants.StatusOk && !mesg.Contains("no data"))
                    _logger.LogError("Error while fetching positions : {mesg}", mesg);
            }
            return resp ?? [];
        }

        private async Task<IEnumerable<HoldingsResponse>> GetHoldingsFromServerAsync()
        {
            var (resp, mesg) = await _api.Holdings.GetHoldingsAsync();
            if (resp is null)
            {
                if (mesg != Constants.StatusOk && !mesg.Contains("no data"))
                    _logger.LogError("Error while fetching holdings : {mesg}", mesg);
            }
            return resp ?? [];
        }

        private async Task<IEnumerable<MarginEquityResponse>> GetMarginEquitiesFromServerAsync()
        {
            var (resp, mesg) = await _api.MarketInfo.GetMarginCalculatorEquitiesAsync();
            if (resp is null)
            {
                if (mesg != Constants.StatusOk && mesg.Contains("no data"))
                    _logger.LogError("Error while fetching equity margin : {mesg}", mesg);                
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

                    DailyClose = h.DailyClose,
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