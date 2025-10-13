using FlatTrade;
using FlatTrade.Common.Types.Base;
using Microsoft.Extensions.Logging;

namespace StrategyEngine
{
    internal class OrderProcessor(Api api, ILoggerFactory loggerFactory)
    {
        private readonly Api _api = api;
        private readonly ILogger<OrderProcessor> _logger = loggerFactory.CreateLogger<OrderProcessor>();

        private static bool IsCOOrBOOrderAllowedForExchangeAndRetentionTypeCombination(Exchange exch, RetentionType retentionType)
        {
            if (exch == Exchange.BSE && !(retentionType == RetentionType.DAY || retentionType == RetentionType.EOS))
                return false;

            if (exch == Exchange.NSE && retentionType != RetentionType.DAY)
                return false;

            return true;
        }

        private static bool IsMISOrCNCOrderAllowedForExchangeAndRetentionTypeCombination(Exchange exch, RetentionType retentionType)
        {
            if (exch == Exchange.BSE && !(retentionType == RetentionType.DAY || retentionType == RetentionType.EOS))            
                return false;

            if (exch == Exchange.NSE && !(retentionType == RetentionType.DAY || retentionType == RetentionType.IOC) )
                return false;

            return true;
        }

        public async Task CreateOrderLimitAsync(string tradingSymbol, int qty, decimal prc,
                                                      bool isBuy,
                                                      bool isIntraDay,
                                                      Exchange exch = Exchange.NSE,
                                                      RetentionType retentionType = RetentionType.DAY,
                                                      bool amo = false)
        {
            var productType = isIntraDay ? ProductType.IntraDay : ProductType.Delivery;
            if (!IsMISOrCNCOrderAllowedForExchangeAndRetentionTypeCombination(exch, retentionType))
            {
                _logger.LogError("{productType} orders are not allowed for {exch} exchange with {retentionType} retention type. Converting to DAY", productType, exch, retentionType);
                retentionType = RetentionType.DAY;
            }

            var orderRequest = new FlatTrade.OrderManager.PlaceOrderRequest
            {
                UserId = _api.Order.UserId,
                AccountId = _api.Order.AccountId, 
                Exchange = exch,
                TradingSymbol = tradingSymbol,
                Quantity = qty,
                Price = prc,
                TransactionType = isBuy ? TransactionType.Buy : TransactionType.Sell,
                PriceType = PriceType.Limit,
                ProductType = productType,
                RetentionType = retentionType,
                Amo = amo ? "Yes" : "No",
            };

            var (orderResp, mesg) = await _api.Order.PlaceOrderAsync(orderRequest);
            if (orderResp is null)
            {
                if (mesg != Constants.StatusOk)
                    _logger.LogError("Error while placing [{productType} Limit] order : {mesg}", productType, mesg);
                else
                    _logger.LogInformation("No order placed [{productType} Limit] : {mesg}", productType, mesg);
            }
            else
            {
                _logger.LogInformation("[{productType} Limit] order placed [{orderResp.NorenOrderNumber}]", productType, orderResp.NorenOrderNumber);
            }
        }

        public async Task CreateOrderMarketAsync(string tradingSymbol, int qty, decimal marketProtectionInPecent,
                                                    bool isBuy,
                                                    bool isIntraDay,
                                                    Exchange exch = Exchange.NSE,
                                                    RetentionType retentionType = RetentionType.DAY,
                                                    bool amo = false)
        {
            var productType = isIntraDay ? ProductType.IntraDay : ProductType.Delivery;
            if (!IsMISOrCNCOrderAllowedForExchangeAndRetentionTypeCombination(exch, retentionType))
            {
                _logger.LogError("{productType} orders are not allowed for {exch} exchange with {retentionType} retention type. Converting to DAY", productType, exch, retentionType);
                retentionType = RetentionType.DAY;
            }

            var prc = 0.0m; // market price  // need to fetch latest prices somehow.
            var marketLimitPrice = prc * (marketProtectionInPecent / 100); // need to round to ticks

            var orderRequest = new FlatTrade.OrderManager.PlaceOrderRequest
            {
                UserId = _api.Order.UserId,
                AccountId = _api.Order.AccountId,
                Exchange = exch,
                TradingSymbol = tradingSymbol,
                Quantity = qty,
                Price = prc + (isBuy ? marketLimitPrice: -marketLimitPrice),
                TransactionType = isBuy ? TransactionType.Buy : TransactionType.Sell,
                PriceType = PriceType.Limit,
                ProductType = productType,
                RetentionType = retentionType,
                Amo = amo ? "Yes" : "No",
            };

            var (orderResp, mesg) = await _api.Order.PlaceOrderAsync(orderRequest);
            if (orderResp is null)
            {
                if (mesg != Constants.StatusOk)
                    _logger.LogError("Error while placing [{productType} Market] order : {mesg}", productType, mesg);
                else
                    _logger.LogInformation("No order placed [{productType} Market] : {mesg}", productType, mesg);
            }
            else
            {
                _logger.LogInformation("[{productType} Market] order placed [{orderResp.NorenOrderNumber}]", productType, orderResp.NorenOrderNumber);
            }
        }

        public async Task CreateOrderSLLimitAsync(string tradingSymbol, int qty, decimal prc, decimal triggerPrc, //Trigger Price ≤ Limit Price in case of buy
                                                    bool isBuy,
                                                    bool isIntraDay,
                                                    Exchange exch = Exchange.NSE,
                                                    RetentionType retentionType = RetentionType.DAY,
                                                    bool amo = false)
        {
            var productType = isIntraDay ? ProductType.IntraDay : ProductType.Delivery;
            if (!IsMISOrCNCOrderAllowedForExchangeAndRetentionTypeCombination(exch, retentionType))
            {
                _logger.LogError("{productType} orders are not allowed for {exch} exchange with {retentionType} retention type. Converting to DAY", productType, exch, retentionType);
                retentionType = RetentionType.DAY;
            }

            var orderRequest = new FlatTrade.OrderManager.PlaceOrderRequest
            {
                UserId = _api.Order.UserId,
                AccountId = _api.Order.AccountId,
                Exchange = exch,
                TradingSymbol = tradingSymbol,
                Quantity = qty,
                Price = prc,
                TriggerPrice = triggerPrc,
                TransactionType = isBuy ? TransactionType.Buy : TransactionType.Sell,
                PriceType = PriceType.StopLossLimit,
                ProductType = productType,
                RetentionType = retentionType,
                Amo = amo ? "Yes" : "No",
            };

            var (orderResp, mesg) = await _api.Order.PlaceOrderAsync(orderRequest);
            if (orderResp is null)
            {
                if (mesg != Constants.StatusOk)
                    _logger.LogError("Error while placing [{productType} SL Limit] order : {mesg}", productType, mesg);
                else
                    _logger.LogInformation("No order placed [{productType} SL Limit] : {mesg}", productType, mesg);
            }
            else
            {
                _logger.LogInformation("[{productType} SL Limit] order placed [{orderResp.NorenOrderNumber}]", productType, orderResp.NorenOrderNumber);
            }
        }

        public async Task CreateOrderSLMarketAsync(string tradingSymbol, int qty, decimal marketProtectionInPecent, decimal triggerPrc,
                                                      bool isBuy,
                                                      bool isIntraDay,
                                                      Exchange exch = Exchange.NSE,
                                                      RetentionType retentionType = RetentionType.DAY,
                                                      bool amo = false)
        {
            var productType = isIntraDay ? ProductType.IntraDay : ProductType.Delivery;
            if (!IsMISOrCNCOrderAllowedForExchangeAndRetentionTypeCombination(exch, retentionType))
            {
                _logger.LogError("{productType} orders are not allowed for {exch} exchange with {retentionType} retention type. Converting to DAY", productType, exch, retentionType);
                retentionType = RetentionType.DAY;
            }

            var prc = 0.0m; // market price  // need to fetch latest prices somehow.
            var marketLimitPrice = prc * (marketProtectionInPecent / 100); // need to round to ticks

            var orderRequest = new FlatTrade.OrderManager.PlaceOrderRequest
            {
                UserId = _api.Order.UserId,
                AccountId = _api.Order.AccountId,
                Exchange = exch,
                TradingSymbol = tradingSymbol,
                Quantity = qty,
                Price = prc + (isBuy ? marketLimitPrice : -marketLimitPrice),
                TriggerPrice = triggerPrc,
                TransactionType = isBuy ? TransactionType.Buy : TransactionType.Sell,
                PriceType = PriceType.Limit,
                ProductType = productType,
                RetentionType = retentionType,
                Amo = amo ? "Yes" : "No",
            };

            var (orderResp, mesg) = await _api.Order.PlaceOrderAsync(orderRequest);
            if (orderResp is null)
            {
                if (mesg != Constants.StatusOk)
                    _logger.LogError("Error while placing [{productType} SL Market] order : {mesg}", productType, mesg);
                else
                    _logger.LogInformation("No order placed [{productType} SL Market] : {mesg}", productType, mesg);
            }
            else
            {
                _logger.LogInformation("[{productType} SL Market] order placed [{orderResp.NorenOrderNumber}]", productType, orderResp.NorenOrderNumber);
            }
        }

        //==================================================================================================
        //==================================================================================================

        public async Task CreateOrderCOLimitAsync(string tradingSymbol, int qty, decimal prc, decimal differentialStopPrc,
                                                  bool isBuy,
                                                  Exchange exch = Exchange.NSE,
                                                  RetentionType retentionType = RetentionType.DAY, // for BSE: DAY/EOS and FOR NSE: DAY only                                                  
                                                  bool amo = false)
        {
            if (!IsCOOrBOOrderAllowedForExchangeAndRetentionTypeCombination(exch, retentionType))
            {
                _logger.LogError("CO orders are not allowed for {exch} exchange with {retentionType} retention type. Converting to DAY", exch, retentionType);
                retentionType = RetentionType.DAY;
            }           

            var orderRequest = new FlatTrade.OrderManager.PlaceCOOrderRequest
            {
               UserId = _api.Order.UserId,
               AccountId = _api.Order.AccountId,
               Exchange = exch,
               BookLossProfit = differentialStopPrc,
               Amo = amo ? "Yes" : "No",
               TradingSymbol = tradingSymbol,
               Quantity = qty,
               Price = prc,
               TransactionType = isBuy ? TransactionType.Buy : TransactionType.Sell,
               PriceType = PriceType.Limit,
               ProductType = ProductType.HighLeverage,
               RetentionType = retentionType,
            };

            var (orderResp, mesg) = await _api.Order.PlaceCOOrderAsync(orderRequest);
            if (orderResp is null)
            {
                if (mesg != Constants.StatusOk)
                    _logger.LogError("Error while placing [CO Limit] order : {mesg}", mesg);
                else
                    _logger.LogInformation("No order placed [CO Limit] : {mesg}", mesg);
            }
            else
            {
                _logger.LogInformation("[CO Limit] order placed [{orderResp.NorenOrderNumber}]", orderResp.NorenOrderNumber);
            }
        }

        public async Task CreateOrderCOMarketAsync(string tradingSymbol, int qty, decimal differentialStopPrc, decimal marketProtectionInPecent,
                                                   bool isBuy,
                                                   Exchange exch = Exchange.NSE,
                                                   RetentionType retentionType = RetentionType.DAY, // for BSE: DAY/EOS and FOR NSE: DAY only                                                   
                                                   bool amo = false)
        {
            if (!IsCOOrBOOrderAllowedForExchangeAndRetentionTypeCombination(exch, retentionType))
            {
                _logger.LogError("CO orders are not allowed for {exch} exchange with {retentionType} retention type. Converting to DAY", exch, retentionType);
                retentionType = RetentionType.DAY;
            }

            var prc = 0.0m; // market price  // need to fetch latest prices somehow.
            var marketLimitPrice = prc * (marketProtectionInPecent / 100); // need to round to ticks

            var orderRequest = new FlatTrade.OrderManager.PlaceCOOrderRequest
            {
                UserId = _api.Order.UserId,
                AccountId = _api.Order.AccountId,
                Exchange = exch,
                BookLossProfit = differentialStopPrc,
                Amo = amo ? "Yes" : "No",
                TradingSymbol = tradingSymbol,
                Quantity = qty,
                Price = prc + (isBuy ? marketLimitPrice : -marketLimitPrice),
                TransactionType = isBuy ? TransactionType.Buy : TransactionType.Sell,
                PriceType = PriceType.Limit,
                ProductType = ProductType.HighLeverage,
                RetentionType = retentionType,
            };

            var (orderResp, mesg) = await _api.Order.PlaceCOOrderAsync(orderRequest);
            if (orderResp is null)
            {
                if (mesg != Constants.StatusOk)
                    _logger.LogError("Error while placing [CO Market] order : {mesg}", mesg);
                else
                    _logger.LogInformation("No order placed [CO Market] : {mesg}", mesg);
            }
            else
            {
                _logger.LogInformation("[CO Market] order placed [{orderResp.NorenOrderNumber}]", orderResp.NorenOrderNumber);
            }
        }

        public async Task CreateOrderCOSLLimitAsync(string tradingSymbol, int qty, decimal prc, decimal differentialStopPrc, decimal triggerPrc,
                                                    bool isBuy,
                                                    Exchange exch = Exchange.NSE,
                                                    RetentionType retentionType = RetentionType.DAY, // for BSE: DAY/EOS and FOR NSE: DAY only                                                    
                                                    bool amo = false)
        {
            if (!IsCOOrBOOrderAllowedForExchangeAndRetentionTypeCombination(exch, retentionType))
            {
                _logger.LogError("CO orders are not allowed for {exch} exchange with {retentionType} retention type. Converting to DAY", exch, retentionType);
                retentionType = RetentionType.DAY;
            }

            var orderRequest = new FlatTrade.OrderManager.PlaceCOOrderRequest
            {
                UserId = _api.Order.UserId,
                AccountId = _api.Order.AccountId,
                Exchange = exch,
                BookLossProfit = differentialStopPrc,
                Amo = amo ? "Yes" : "No",
                TradingSymbol = tradingSymbol,
                Quantity = qty,
                TriggerPrice = triggerPrc,
                Price = prc,
                TransactionType = isBuy ? TransactionType.Buy : TransactionType.Sell,
                PriceType = PriceType.StopLossLimit,
                ProductType = ProductType.HighLeverage,
                RetentionType = retentionType,
            };

            var (orderResp, mesg) = await _api.Order.PlaceCOOrderAsync(orderRequest);
            if (orderResp is null)
            {
                if (mesg != Constants.StatusOk)
                    _logger.LogError("Error while placing [CO SL Limit] order : {mesg}", mesg);
                else
                    _logger.LogInformation("No order placed [CO SL Limit] : {mesg}", mesg);
            }
            else
            {
                _logger.LogInformation("[CO SL Limit] order placed [{orderResp.NorenOrderNumber}]", orderResp.NorenOrderNumber);
            }
        }

        //=====================================================================================================

        public async Task CreateOrderBOLimitAsync(string tradingSymbol, int qty, decimal prc, decimal differentialStopPrc, decimal differentialTargetPrc, int differentialTrailingStopTicks,
                                                  bool isBuy,
                                                  Exchange exch = Exchange.NSE,
                                                  RetentionType retentionType = RetentionType.DAY, // for BSE: DAY/EOS and FOR NSE: DAY only                                                  
                                                  bool amo = false)
        {
            if (!IsCOOrBOOrderAllowedForExchangeAndRetentionTypeCombination(exch, retentionType))
            {
                _logger.LogError("BO orders are not allowed for {exch} exchange with {retentionType} retention type. Converting to DAY", exch, retentionType);
                retentionType = RetentionType.DAY;
            }

            var orderRequest = new FlatTrade.OrderManager.PlaceOrderRequest
            {
                UserId = _api.Order.UserId,
                AccountId = _api.Order.AccountId,
                Exchange = exch,
                BookLossProfit = differentialStopPrc,
                Amo = amo ? "Yes" : "No",
                TradingSymbol = tradingSymbol,
                Quantity = qty,
                Price = prc,
                TransactionType = isBuy ? TransactionType.Buy : TransactionType.Sell,
                PriceType = PriceType.Limit,
                ProductType = ProductType.HighLeverage,
                RetentionType = retentionType,
            };

            var (orderResp, mesg) = await _api.Order.PlaceOrderAsync(orderRequest);
            if (orderResp is null)
            {
                if (mesg != Constants.StatusOk)
                    _logger.LogError("Error while placing [BO Limit] order : {mesg}", mesg);
                else
                    _logger.LogInformation("No order placed [BO Limit] : {mesg}", mesg);
            }
            else
            {
                _logger.LogInformation("[BO Limit] order placed [{orderResp.NorenOrderNumber}]", orderResp.NorenOrderNumber);
            }
        }
        public async Task CreateOrderBOMarketAsync( string tradingSymbol, int qty, decimal differentialStopPrc, decimal differentialTargetPrc, int differentialTrailingStopPrc, decimal marketProtectionInPecent,
                                                   bool isBuy,
                                                   Exchange exch = Exchange.NSE,
                                                   RetentionType retentionType = RetentionType.DAY,
                                                   bool amo = false)
        {
            if (!IsCOOrBOOrderAllowedForExchangeAndRetentionTypeCombination(exch, retentionType))
            {
                _logger.LogError("BO orders are not allowed for {exch} exchange with {retentionType} retention type. Converting to DAY", exch, retentionType);
                retentionType = RetentionType.DAY;
            }
            var prc = 0.0m; // market price  // need to fetch latest prices somehow.
            var marketLimitPrice = prc * (marketProtectionInPecent / 100); // need to round to ticks

            var orderRequest = new FlatTrade.OrderManager.PlaceOrderRequest
            {
                UserId = _api.Order.UserId,
                AccountId = _api.Order.AccountId,
                Exchange = exch,
                BookLossProfit = differentialStopPrc,
                Amo = amo ? "Yes" : "No",
                TradingSymbol = tradingSymbol,
                Quantity = qty,
                Price = prc + (isBuy ? marketLimitPrice : -marketLimitPrice),
                TransactionType = isBuy ? TransactionType.Buy : TransactionType.Sell,
                PriceType = PriceType.Limit,
                ProductType = ProductType.HighLeverage,
                RetentionType = retentionType,
            };

            var (orderResp, mesg) = await _api.Order.PlaceOrderAsync(orderRequest);
            if (orderResp is null)
            {
                if (mesg != Constants.StatusOk)
                    _logger.LogError("Error while placing [BO Limit] order : {mesg}", mesg);
                else
                    _logger.LogInformation("No order placed [BO Limit] : {mesg}", mesg);
            }
            else
            {
                _logger.LogInformation("[BO Limit] order placed [{orderResp.NorenOrderNumber}]", orderResp.NorenOrderNumber);
            }
        }

        //public async Task CreateOrderBOSLLimitAsync(Exchange exch, string tradingSymbol, int qty, decimal prc, decimal differentialStopPrc, decimal differentialTargetPrc, int differentialTrailingStopPrc, decimal triggerPrc,
        //                                            bool isBuy,
        //                                            bool amo = false);

    }
}
