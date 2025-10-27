using FlatTrade;
using FlatTrade.Common.Types.Base;
using Microsoft.Extensions.Logging;
using StrategyEngine.Model;

namespace StrategyEngine
{
    internal class OrderProcessor(Api api, ILoggerFactory loggerFactory) : IOrderProcessor
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

        public async Task CancelOrder(CancelOrder cancelOrder)
        {
            var (resp, msg) = await _api.Order.CancelOrderAsync(cancelOrder.NorenOrderNumber);
            if (resp is null)
            {
                _logger.LogError("CancelOrder: Failed cancel order request [{cancelOrder.NorenOrderNumber}]. Error msg {msg}", cancelOrder.NorenOrderNumber, msg);
            }
        }

        public async Task ModifyOrder(ModifyOrder modifyOrder)
        {
        }

        public async Task CreateOrder(CreateOrder createOrder)
        {
            switch (createOrder.ProductType)
            {
                case ProductType.BracketOrder:
                    switch (createOrder.PriceType)
                    {
                        case PriceType.Market:
                            await CreateOrderBOMarketAsync(createOrder.TradingSymbol,
                                               createOrder.Quantity,
                                               createOrder.DifferentialSLPrice,
                                               createOrder.DifferentialProfitPrice,
                                               createOrder.DifferentialTrailingTicks,
                                               createOrder.MarketProtectionInPercent,                                               
                                               createOrder.TransactionType,
                                               createOrder.Exchange,
                                               createOrder.RetentionType,
                                               IsAmo());
                            break;
                        case PriceType.Limit:
                            await CreateOrderBOLimitAsync(createOrder.TradingSymbol,
                                                createOrder.Quantity,
                                                createOrder.LimitPrice,
                                                createOrder.DifferentialSLPrice,
                                                createOrder.DifferentialProfitPrice,
                                                createOrder.DifferentialTrailingTicks,
                                                createOrder.TransactionType,
                                                createOrder.Exchange,
                                                createOrder.RetentionType,
                                                IsAmo());
                            break;
                        case PriceType.StopLossMarket:
                            await CreateOrderBOSLLimitAsync(createOrder.TradingSymbol,
                                                createOrder.Quantity,
                                                createOrder.LimitPrice,
                                                createOrder.BoTriggerPrice,
                                                createOrder.DifferentialSLPrice,
                                                createOrder.DifferentialProfitPrice,
                                                createOrder.DifferentialTrailingTicks,
                                                createOrder.TransactionType,
                                                createOrder.Exchange,
                                                createOrder.RetentionType,
                                                IsAmo());
                            break;
                        case PriceType.StopLossLimit:
                        default:
                            _logger.LogWarning("CreateOrder:{ProductType} Unsupported/unknown PriceType {PriceType} ", createOrder.ProductType, createOrder.PriceType);
                            break;
                    }                    
                    break;
                case ProductType.IntraDay:
                case ProductType.Delivery:
                    switch(createOrder.PriceType)
                    {
                        case PriceType.Limit:
                            await CreateOrderLimitAsync(createOrder.TradingSymbol,
                            createOrder.Quantity,
                            createOrder.LimitPrice,
                            createOrder.TransactionType,
                            createOrder.ProductType == ProductType.IntraDay,
                            createOrder.Exchange,
                            createOrder.RetentionType,
                            IsAmo());

                            break;
                        case PriceType.StopLossLimit:
                            await CreateOrderSLLimitAsync(createOrder.TradingSymbol,
                            createOrder.Quantity,
                            createOrder.LimitPrice,
                            createOrder.TransactionType == TransactionType.Buy ?
                                    createOrder.LimitPrice + createOrder.DifferentialSLPrice :
                                    createOrder.LimitPrice - createOrder.DifferentialSLPrice,  //SL Trigger price
                            createOrder.TransactionType,
                            createOrder.ProductType == ProductType.IntraDay,
                            createOrder.Exchange,
                            createOrder.RetentionType,
                            IsAmo());

                            break;
                        case PriceType.Market:
                            await CreateOrderMarketAsync(createOrder.TradingSymbol,
                            createOrder.Quantity,
                            createOrder.MarketProtectionInPercent,
                            createOrder.TransactionType,
                            createOrder.ProductType == ProductType.IntraDay,
                            createOrder.Exchange,
                            createOrder.RetentionType,
                            IsAmo());

                            break;
                        case PriceType.StopLossMarket:
                            await CreateOrderSLMarketAsync(createOrder.TradingSymbol,
                            createOrder.Quantity,
                            createOrder.MarketProtectionInPercent,
                            createOrder.TransactionType == TransactionType.Buy ?
                                    createOrder.LimitPrice + createOrder.DifferentialSLPrice :
                                    createOrder.LimitPrice - createOrder.DifferentialSLPrice,  //SL Trigger price
                            createOrder.TransactionType,
                            createOrder.ProductType == ProductType.IntraDay,
                            createOrder.Exchange,
                            createOrder.RetentionType,
                            IsAmo());
                            break;
                        default:
                            _logger.LogWarning("CreateOrder:{ProductType} Unsupported/unknown PriceType {PriceType} ", createOrder.ProductType, createOrder.PriceType);
                            break;
                    }
                                                
                    break;
                case ProductType.HighLeverage:
                case ProductType.MargingTradeFacility:
                case ProductType.Normal:                    
                default:
                    _logger.LogWarning("CreateOrder: Unsupported/unknown ProductType {ProductType}", createOrder.ProductType);
                    break;
            }
        }

        private static bool IsAmo()
        {
            var curentDateTime = DateTime.Now;
            var tradingStartDateTime = curentDateTime.Date.AddHours(9).AddMinutes(15);
            var tradingEndDateTime = curentDateTime.Date.AddHours(15).AddMinutes(30);
            return (curentDateTime < tradingStartDateTime) || (curentDateTime >= tradingEndDateTime);
        }

        private async Task CreateOrderLimitAsync(string tradingSymbol, int qty, decimal prc,
                                                      TransactionType transactionType,
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
                TransactionType = transactionType,
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

        private async Task CreateOrderMarketAsync(string tradingSymbol, int qty, decimal marketProtectionInPecent,
                                                    TransactionType transactionType,
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
                Price = prc + (transactionType == TransactionType.Buy ? marketLimitPrice: -marketLimitPrice),
                TransactionType = transactionType,
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

        private async Task CreateOrderSLLimitAsync(string tradingSymbol, int qty, decimal prc, decimal triggerPrc, //Trigger Price ≤ Limit Price in case of buy
                                                    TransactionType transactionType,
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
                TransactionType = transactionType,
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

        private async Task CreateOrderSLMarketAsync(string tradingSymbol, int qty, decimal marketProtectionInPecent, decimal triggerPrc,
                                                      TransactionType transactionType,
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
                Price = prc + (transactionType == TransactionType.Buy ? marketLimitPrice : -marketLimitPrice),
                TriggerPrice = triggerPrc,
                TransactionType = transactionType,
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

        private async Task CreateOrderCOLimitAsync(string tradingSymbol, int qty, decimal prc, decimal differentialStopPrc,
                                                  TransactionType transactionType,
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
               TransactionType = transactionType,
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

        private async Task CreateOrderCOMarketAsync(string tradingSymbol, int qty, decimal differentialStopPrc, decimal marketProtectionInPecent,
                                                   TransactionType transactionType,
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
                Price = prc + (transactionType == TransactionType.Buy ? marketLimitPrice : -marketLimitPrice),
                TransactionType = transactionType,
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

        private async Task CreateOrderCOSLLimitAsync(string tradingSymbol, int qty, decimal prc, decimal differentialStopPrc, decimal triggerPrc,
                                                    TransactionType transactionType,
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
                TransactionType = transactionType,
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

        private async Task CreateOrderBOLimitAsync(string tradingSymbol, int qty, decimal prc, decimal differentialStopPrc, decimal differentialTargetPrc, int differentialTrailingStopTicks,
                                                  TransactionType transactionType,
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
                TransactionType = transactionType,
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
        private async Task CreateOrderBOMarketAsync( string tradingSymbol, int qty, decimal differentialStopPrc, decimal differentialTargetPrc, int differentialTrailingStopPrc, decimal marketProtectionInPecent,
                                                   TransactionType transactionType,
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
                Price = prc + (transactionType == TransactionType.Buy ? marketLimitPrice : -marketLimitPrice),
                TransactionType = transactionType,
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

        public async Task CreateOrderBOSLLimitAsync(string tradingSymbol, int qty, decimal prc, decimal triggerPrc, decimal differentialStopPrc, decimal differentialTargetPrc, int differentialTrailingStopPrc,
                                                    TransactionType transactionType,
                                                    Exchange exch = Exchange.NSE,
                                                    RetentionType retentionType = RetentionType.DAY,                                                    
                                                    bool amo = false)
        {

        }

    }
}
