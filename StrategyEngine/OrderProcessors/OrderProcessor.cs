using FlatTrade;
using FlatTrade.Common.Types.Base;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StrategyEngine.Model;
using System.Collections.Concurrent;

namespace StrategyEngine.OrderProcessors
{
    internal class OrderProcessor(IConfiguration config, Api api, ILoggerFactory loggerFactory)
        : AsbtractOrderProcessor<OrderProcessor>(config, api, loggerFactory)
    {
        protected override string Name => $"{GetType().Name}";

        private ConcurrentDictionary<string, StrategyOnTouchLineSnapshot> _touchLineUpdates = [];

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

            if (exch == Exchange.NSE && !(retentionType == RetentionType.DAY || retentionType == RetentionType.IOC))
                return false;

            return true;
        }

        protected override Task OnUpdateInternal(StrategyOnTouchLineSnapshot input)
        {
            _touchLineUpdates.AddOrUpdate(input.updates.TradingSymbol, input, (_, existing) => { existing = input; return input; });
            return Task.CompletedTask;
        }

        private (decimal, decimal, int) GetLastTradePrice(string tradingSymbol)
        {
            if (_touchLineUpdates.TryGetValue(tradingSymbol, out StrategyOnTouchLineSnapshot? val) && val is not null)
            {
                return (val.updates.LastTradePrice, val.updates.TickSize, val.updates.PricePrecision);

            }
            _logger.LogWarning("{0}: LastTradePrice/TickSize/PricePrecision not found for [{1}], Returning default (0.0, 0.05, 2).", GetType().Name, tradingSymbol);
            return (0.0m, 0.05m, 2);
        }

        private decimal GetMarketLimitPriceWithProtection(string tradingSymbol, decimal marketProtection, TransactionType transactionType)
        {
            var (lastTradePrice, tickSize, pricePrecision) = GetLastTradePrice(tradingSymbol);
            var lastTradePriceWithMarketProtectionRounded = Math.Round((lastTradePrice * marketProtection) / (100 * tickSize)) * tickSize;
            var lastTradePriceWithMarketProtection = Math.Round(lastTradePriceWithMarketProtectionRounded, pricePrecision);
            return TransactionType.Buy == transactionType ?
                            lastTradePrice + lastTradePriceWithMarketProtection
                            : lastTradePrice - lastTradePriceWithMarketProtection;
        }

        public override async Task CancelOrder(CancelOrder cancelOrder)
        {
            var (resp, msg) = await Api.Order.CancelOrderAsync(cancelOrder.NorenOrderNumber);
            if (resp is null)
            {
                _logger.LogError("CancelOrder: Failed cancel order request [{cancelOrder.NorenOrderNumber}]. Error msg {msg}", cancelOrder.NorenOrderNumber, msg);
            }
        }

        public override async Task ModifyOrder(ModifyOrder modifyOrder)
        {
            switch (modifyOrder.ProductType)
            {
                case ProductType.BracketOrder:
                    switch (modifyOrder.PriceType)
                    {
                        case PriceType.Limit:
                            await ModifyOrderBOLimitAsync(modifyOrder.NorenOrderNumber, 
                                                modifyOrder.TradingSymbol,
                                                modifyOrder.Quantity,
                                                modifyOrder.LimitPrice,
                                                modifyOrder.DifferentialSLPrice,
                                                modifyOrder.DifferentialProfitPrice,
                                                modifyOrder.DifferentialTrailingTicks,
                                                modifyOrder.Exchange,
                                                modifyOrder.RetentionType);
                            break;
                        case PriceType.Market:
                            await ModifyOrderBOMarketAsync(modifyOrder.NorenOrderNumber,
                                               modifyOrder.TradingSymbol,
                                               modifyOrder.Quantity,
                                               modifyOrder.DifferentialSLPrice,
                                               modifyOrder.DifferentialProfitPrice,
                                               modifyOrder.DifferentialTrailingTicks,
                                               modifyOrder.TransactionType,
                                               modifyOrder.MarketProtectionInPercent,
                                               modifyOrder.Exchange,
                                               modifyOrder.RetentionType);
                            break;
                        case PriceType.StopLossLimit:
                            await ModifyOrderBOSLLimitAsync(modifyOrder.NorenOrderNumber,
                                                modifyOrder.TradingSymbol,
                                                modifyOrder.Quantity,
                                                modifyOrder.LimitPrice,
                                                modifyOrder.DifferentialSLPrice,
                                                modifyOrder.DifferentialProfitPrice,
                                                modifyOrder.DifferentialTrailingTicks,
                                                modifyOrder.Exchange,
                                                modifyOrder.RetentionType);
                            break;
                        case PriceType.StopLossMarket:
                        default:
                            _logger.LogWarning("ModifyOrder:{ProductType} Unsupported/unknown PriceType {PriceType} ", modifyOrder.ProductType, modifyOrder.PriceType);
                            break;
                    }
                    break;
                case ProductType.IntraDay:
                case ProductType.Delivery:
                    switch (modifyOrder.PriceType)
                    {
                        case PriceType.Limit:
                            await ModifyOrderLimitAsync(modifyOrder.NorenOrderNumber,
                            modifyOrder.TradingSymbol,
                            modifyOrder.Quantity,
                            modifyOrder.LimitPrice,
                            modifyOrder.Exchange,
                            modifyOrder.RetentionType);

                            break;
                        case PriceType.StopLossLimit:
                            await ModifyOrderSLLimitAsync(modifyOrder.NorenOrderNumber, 
                            modifyOrder.TradingSymbol,
                            modifyOrder.Quantity,
                            modifyOrder.LimitPrice,
                            modifyOrder.TriggerPrice,
                            modifyOrder.TransactionType,
                            modifyOrder.Exchange,
                            modifyOrder.RetentionType);

                            break;
                        case PriceType.Market:
                            await ModifyOrderMarketAsync(modifyOrder.NorenOrderNumber, 
                            modifyOrder.TradingSymbol,
                            modifyOrder.Quantity,
                            modifyOrder.TransactionType,
                            modifyOrder.MarketProtectionInPercent,
                            modifyOrder.Exchange,
                            modifyOrder.RetentionType);

                            break;
                        case PriceType.StopLossMarket:
                            await ModifyOrderSLMarketAsync(modifyOrder.NorenOrderNumber,
                            modifyOrder.TradingSymbol,
                            modifyOrder.Quantity,
                            modifyOrder.TriggerPrice,
                            modifyOrder.TransactionType,
                            modifyOrder.MarketProtectionInPercent,
                            modifyOrder.Exchange,
                            modifyOrder.RetentionType);
                            break;
                        default:
                            _logger.LogWarning("ModifyOrder:{ProductType} Unsupported/unknown PriceType {PriceType} ", modifyOrder.ProductType, modifyOrder.PriceType);
                            break;
                    }
                    break;
                case ProductType.HighLeverage:
                    switch (modifyOrder.PriceType)
                    {
                        case PriceType.Market:
                            await ModifyOrderCOMarketAsync(modifyOrder.NorenOrderNumber,
                                               modifyOrder.TradingSymbol,
                                               modifyOrder.Quantity,
                                               modifyOrder.DifferentialSLPrice,
                                               modifyOrder.MarketProtectionInPercent,
                                               modifyOrder.TransactionType,
                                               modifyOrder.Exchange,
                                               modifyOrder.RetentionType);
                            break;
                        case PriceType.Limit:
                            await ModifyOrderCOLimitAsync(modifyOrder.NorenOrderNumber, 
                                                modifyOrder.TradingSymbol,
                                                modifyOrder.Quantity,
                                                modifyOrder.LimitPrice,
                                                modifyOrder.DifferentialSLPrice,
                                                modifyOrder.Exchange,
                                                modifyOrder.RetentionType);
                            break;
                        case PriceType.StopLossLimit:
                            await ModifyOrderCOSLLimitAsync(modifyOrder.NorenOrderNumber, 
                                                modifyOrder.TradingSymbol,
                                                modifyOrder.Quantity,
                                                modifyOrder.LimitPrice,
                                                modifyOrder.TriggerPrice,
                                                modifyOrder.DifferentialSLPrice,
                                                modifyOrder.TransactionType,
                                                modifyOrder.Exchange,
                                                modifyOrder.RetentionType);
                            break;
                        case PriceType.StopLossMarket:
                        default:
                            _logger.LogWarning("ModifyOrder:{ProductType} Unsupported/unknown PriceType {PriceType} ", modifyOrder.ProductType, modifyOrder.PriceType);
                            break;
                    }
                    break;
                case ProductType.MargingTradeFacility:
                case ProductType.Normal:
                default:
                    _logger.LogWarning("ModifyOrder: Unsupported/unknown ProductType {ProductType}", modifyOrder.ProductType);
                    break;
            }
        }

        public override async Task CreateOrder(CreateOrder createOrder)
        {
            switch (createOrder.ProductType)
            {
                case ProductType.BracketOrder:
                    switch (createOrder.PriceType)
                    {
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
                        case PriceType.Market:
                            await CreateOrderBOMarketAsync(createOrder.TradingSymbol,
                                               createOrder.Quantity,
                                               createOrder.DifferentialSLPrice,
                                               createOrder.DifferentialProfitPrice,
                                               createOrder.DifferentialTrailingTicks,
                                               createOrder.TransactionType,
                                               createOrder.MarketProtectionInPercent,
                                               createOrder.Exchange,
                                               createOrder.RetentionType,
                                               IsAmo());
                            break;
                        case PriceType.StopLossLimit:
                            await CreateOrderBOSLLimitAsync(createOrder.TradingSymbol,
                                                createOrder.Quantity,
                                                createOrder.LimitPrice,
                                                createOrder.TriggerPrice,
                                                createOrder.DifferentialSLPrice,
                                                createOrder.DifferentialProfitPrice,
                                                createOrder.DifferentialTrailingTicks,
                                                createOrder.TransactionType,
                                                createOrder.Exchange,
                                                createOrder.RetentionType,
                                                IsAmo());
                            break;
                        case PriceType.StopLossMarket:
                        default:
                            _logger.LogWarning("CreateOrder:{ProductType} Unsupported/unknown PriceType {PriceType} ", createOrder.ProductType, createOrder.PriceType);
                            break;
                    }
                    break;
                case ProductType.IntraDay:
                case ProductType.Delivery:
                    switch (createOrder.PriceType)
                    {
                        case PriceType.Limit:
                            await CreateOrderLimitAsync(createOrder.TradingSymbol,
                            createOrder.Quantity,
                            createOrder.LimitPrice,
                            createOrder.TransactionType,
                            createOrder.ProductType,
                            createOrder.Exchange,
                            createOrder.RetentionType,
                            IsAmo());

                            break;
                        case PriceType.StopLossLimit:
                            await CreateOrderSLLimitAsync(createOrder.TradingSymbol,
                            createOrder.Quantity,
                            createOrder.LimitPrice,
                            createOrder.TriggerPrice,
                            createOrder.TransactionType,
                            createOrder.ProductType,
                            createOrder.Exchange,
                            createOrder.RetentionType,
                            IsAmo());

                            break;
                        case PriceType.Market:
                            await CreateOrderMarketAsync(createOrder.TradingSymbol,
                            createOrder.Quantity,
                            createOrder.TransactionType,
                            createOrder.MarketProtectionInPercent,
                            createOrder.ProductType,
                            createOrder.Exchange,
                            createOrder.RetentionType,
                            IsAmo());

                            break;
                        case PriceType.StopLossMarket:
                            await CreateOrderSLMarketAsync(createOrder.TradingSymbol,
                            createOrder.Quantity,
                            createOrder.TriggerPrice,
                            createOrder.TransactionType,
                            createOrder.MarketProtectionInPercent,
                            createOrder.ProductType,
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
                    switch (createOrder.PriceType)
                    {
                        case PriceType.Market:
                            await CreateOrderCOMarketAsync(createOrder.TradingSymbol,
                                               createOrder.Quantity,
                                               createOrder.DifferentialSLPrice,
                                               createOrder.MarketProtectionInPercent,
                                               createOrder.TransactionType,
                                               createOrder.Exchange,
                                               createOrder.RetentionType,
                                               IsAmo());
                            break;
                        case PriceType.Limit:
                            await CreateOrderCOLimitAsync(createOrder.TradingSymbol,
                                                createOrder.Quantity,
                                                createOrder.LimitPrice,
                                                createOrder.DifferentialSLPrice,
                                                createOrder.TransactionType,
                                                createOrder.Exchange,
                                                createOrder.RetentionType,
                                                IsAmo());
                            break;
                        case PriceType.StopLossLimit:
                            await CreateOrderCOSLLimitAsync(createOrder.TradingSymbol,
                                                createOrder.Quantity,
                                                createOrder.LimitPrice,
                                                createOrder.TriggerPrice,
                                                createOrder.DifferentialSLPrice,
                                                createOrder.TransactionType,
                                                createOrder.Exchange,
                                                createOrder.RetentionType,
                                                IsAmo());
                            break;
                        case PriceType.StopLossMarket:
                        default:
                            _logger.LogWarning("CreateOrder:{ProductType} Unsupported/unknown PriceType {PriceType} ", createOrder.ProductType, createOrder.PriceType);
                            break;
                    }
                    break;
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
            return curentDateTime < tradingStartDateTime || curentDateTime >= tradingEndDateTime;
        }

        private bool IsTriggerPriceValid(decimal triggerPrice, TransactionType transactionType, decimal price)
        {
            return transactionType == TransactionType.Buy ? triggerPrice < price : triggerPrice > price;
        }

        //==================================================================================================
        //==================================================================================================
        private async Task ModifyOrderLimitAsync(long norenOrderNumber, string tradingSymbol,
                                                     int qty, decimal prc,
                                                     Exchange exch = Exchange.NSE,
                                                     RetentionType retentionType = RetentionType.DAY)
        {
            if (!IsMISOrCNCOrderAllowedForExchangeAndRetentionTypeCombination(exch, retentionType))
            {
                _logger.LogError("Orders are not allowed for {exch} exchange with {retentionType} retention type. Converting to DAY", exch, retentionType);
                retentionType = RetentionType.DAY;
            }

            var orderRequest = new FlatTrade.OrderManager.ModifyOrderRequest
            {
                UserId = Api.Order.UserId,                
                Exchange = exch,
                NorenOrderNumber = norenOrderNumber,
                TradingSymbol = tradingSymbol,
                Price = prc,
                Quantity = qty,                
                PriceType = PriceType.Limit,
                RetentionType = retentionType
            };

            var (orderResp, mesg) = await Api.Order.ModifyOrderAsync(orderRequest);
            if (orderResp is null)
            {
                if (mesg != Constants.StatusOk)
                    _logger.LogError("Error while modifying order [{orderResp.NorenOrderNumber}] : {mesg}", norenOrderNumber, mesg);
                else
                    _logger.LogInformation("No order [{orderResp.NorenOrderNumber}] modified : {mesg}", norenOrderNumber, mesg);
            }
            else
            {
                _logger.LogInformation("{0}: Order modified [{orderResp.NorenOrderNumber}]", GetType().Name, orderResp.NorenOrderNumber);
            }
        }

        private async Task ModifyOrderMarketAsync( long norenOrderNumber, string tradingSymbol, int qty,
                                                    TransactionType transactionType,
                                                     decimal marketProtectionInPecent = 0.01m,
                                                    Exchange exch = Exchange.NSE,
                                                    RetentionType retentionType = RetentionType.DAY,
                                                    bool amo = false)
        {
            if (!IsMISOrCNCOrderAllowedForExchangeAndRetentionTypeCombination(exch, retentionType))
            {
                _logger.LogWarning("Orders are not allowed for {exch} exchange with {retentionType} retention type. Converting to DAY", exch, retentionType);
                retentionType = RetentionType.DAY;
            }

            var orderRequest = new FlatTrade.OrderManager.ModifyOrderRequest
            {
                UserId = Api.Order.UserId,
                NorenOrderNumber = norenOrderNumber,                
                Exchange = exch,
                TradingSymbol = tradingSymbol,
                Price = GetMarketLimitPriceWithProtection(tradingSymbol, marketProtectionInPecent, transactionType),
                Quantity = qty,
                MarketProtectionPercentage = marketProtectionInPecent,
                PriceType = PriceType.Limit,
                RetentionType = retentionType
            };

            var (orderResp, mesg) = await Api.Order.ModifyOrderAsync(orderRequest);
            if (orderResp is null)
            {
                if (mesg != Constants.StatusOk)
                    _logger.LogError("Error while modifying order [{orderResp.NorenOrderNumber}] : {mesg}", norenOrderNumber, mesg);
                else
                    _logger.LogInformation("No order [{orderResp.NorenOrderNumber}] modified : {mesg}", norenOrderNumber, mesg);
            }
            else
            {
                _logger.LogInformation("Order modified [{orderResp.NorenOrderNumber}]", orderResp.NorenOrderNumber);
            }
        }

        private async Task ModifyOrderSLLimitAsync(long norenOrderNumber, 
                                                    string tradingSymbol, int qty, 
                                                    decimal prc, decimal triggerPrc, //Trigger Price ≤ Limit Price in case of buy
                                                    TransactionType transactionType,
                                                    Exchange exch = Exchange.NSE,
                                                    RetentionType retentionType = RetentionType.DAY)
        {
            if (!IsMISOrCNCOrderAllowedForExchangeAndRetentionTypeCombination(exch, retentionType))
            {
                _logger.LogWarning("{0}: Orders are not allowed for [{exch}] exchange with [{retentionType}] retention type. Converting to DAY", GetType().Name, exch, retentionType);
                retentionType = RetentionType.DAY;
            }

            if (!IsTriggerPriceValid(triggerPrc, transactionType, prc))
            {
                _logger.LogError("{0}: For [{transactionType}/{tradingSymbol}] orders, trigger price should be [{1}] limit price. Discarding order.", GetType().Name, transactionType, tradingSymbol, transactionType == TransactionType.Buy ? "less than" : "greater than");
                return;
            }

            var orderRequest = new FlatTrade.OrderManager.ModifyOrderRequest
            {
                UserId = Api.Order.UserId,
                NorenOrderNumber = norenOrderNumber,
                Exchange = exch,
                TradingSymbol = tradingSymbol,
                Quantity = qty,
                Price = prc,
                TriggerPrice = triggerPrc,
                PriceType = PriceType.StopLossLimit,
                RetentionType = retentionType,
            };

            var (orderResp, mesg) = await Api.Order.ModifyOrderAsync(orderRequest);
            if (orderResp is null)
            {
                if (mesg != Constants.StatusOk)
                    _logger.LogError("Error while modifying order [{orderResp.NorenOrderNumber}] : {mesg}", norenOrderNumber, mesg);
                else
                    _logger.LogInformation("No order [{orderResp.NorenOrderNumber}] modified : {mesg}", norenOrderNumber, mesg);
            }
            else
            {
                _logger.LogInformation("Order modified [{orderResp.NorenOrderNumber}]", orderResp.NorenOrderNumber);
            }
        }

        private async Task ModifyOrderSLMarketAsync(long norenOrderNumber, string tradingSymbol, int qty,
                                                      decimal triggerPrc,
                                                      TransactionType transactionType,
                                                       decimal marketProtectionInPecent = 0.01m,
                                                      Exchange exch = Exchange.NSE,
                                                      RetentionType retentionType = RetentionType.DAY)
        {
            if (!IsMISOrCNCOrderAllowedForExchangeAndRetentionTypeCombination(exch, retentionType))
            {
                _logger.LogError("Orders are not allowed for {exch} exchange with {retentionType} retention type. Converting to DAY", exch, retentionType);
                retentionType = RetentionType.DAY;
            }

            var prc = GetMarketLimitPriceWithProtection(tradingSymbol, marketProtectionInPecent, transactionType);
            if (!IsTriggerPriceValid(triggerPrc, transactionType, prc))
            {
                _logger.LogError("{0}: For [{transactionType}/{tradingSymbol}] orders, trigger price should be [{1}] limit price. Discarding order.", GetType().Name, transactionType, tradingSymbol, transactionType == TransactionType.Buy ? "less than" : "greater than");
                return;
            }

            var orderRequest = new FlatTrade.OrderManager.ModifyOrderRequest
            {
                UserId = Api.Order.UserId,
                NorenOrderNumber = norenOrderNumber,
                Exchange = exch,
                TradingSymbol = tradingSymbol,
                Price = prc,
                Quantity = qty,
                TriggerPrice = triggerPrc,
                MarketProtectionPercentage = marketProtectionInPecent,
                PriceType = PriceType.StopLossLimit,
                RetentionType = retentionType,
            };

            var (orderResp, mesg) = await Api.Order.ModifyOrderAsync(orderRequest);
            if (orderResp is null)
            {
                if (mesg != Constants.StatusOk)
                    _logger.LogError("Error while modifying order [{orderResp.NorenOrderNumber}] : {mesg}", norenOrderNumber, mesg);
                else
                    _logger.LogInformation("No order [{orderResp.NorenOrderNumber}] modified : {mesg}", norenOrderNumber, mesg);
            }
            else
            {
                _logger.LogInformation("Order modified [{orderResp.NorenOrderNumber}]", orderResp.NorenOrderNumber);
            }
        }

        //==================================================================================================
        private async Task CreateOrderLimitAsync(string tradingSymbol, int qty, decimal prc,
                                                      TransactionType transactionType,
                                                      ProductType productType = ProductType.IntraDay,
                                                      Exchange exch = Exchange.NSE,
                                                      RetentionType retentionType = RetentionType.DAY,
                                                      bool amo = false)
        {
            if (!IsMISOrCNCOrderAllowedForExchangeAndRetentionTypeCombination(exch, retentionType))
            {
                _logger.LogError("{productType} orders are not allowed for {exch} exchange with {retentionType} retention type. Converting to DAY", productType, exch, retentionType);
                retentionType = RetentionType.DAY;
            }

            var orderRequest = new FlatTrade.OrderManager.PlaceOrderRequest
            {
                UserId = Api.Order.UserId,
                AccountId = Api.Order.AccountId,
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

            var (orderResp, mesg) = await Api.Order.PlaceOrderAsync(orderRequest);
            if (orderResp is null)
            {
                if (mesg != Constants.StatusOk)
                    _logger.LogError("Error while placing [{productType} Limit] order : {mesg}", productType, mesg);
                else
                    _logger.LogInformation("No order placed [{productType} Limit] : {mesg}", productType, mesg);
            }
            else
            {
                _logger.LogInformation("{0}: [{productType} Limit] order placed [{orderResp.NorenOrderNumber}]", GetType().Name, productType, orderResp.NorenOrderNumber);
            }
        }

        private async Task CreateOrderMarketAsync(string tradingSymbol, int qty,
                                                    TransactionType transactionType,
                                                     decimal marketProtectionInPecent = 0.01m,
                                                    ProductType productType = ProductType.IntraDay,
                                                    Exchange exch = Exchange.NSE,
                                                    RetentionType retentionType = RetentionType.DAY,
                                                    bool amo = false)
        {
            if (!IsMISOrCNCOrderAllowedForExchangeAndRetentionTypeCombination(exch, retentionType))
            {
                _logger.LogWarning("{productType} orders are not allowed for {exch} exchange with {retentionType} retention type. Converting to DAY", productType, exch, retentionType);
                retentionType = RetentionType.DAY;
            }

            var orderRequest = new FlatTrade.OrderManager.PlaceOrderRequest
            {
                UserId = Api.Order.UserId,
                AccountId = Api.Order.AccountId,
                Exchange = exch,
                TradingSymbol = tradingSymbol,
                Price = GetMarketLimitPriceWithProtection(tradingSymbol, marketProtectionInPecent, transactionType),
                Quantity = qty,
                MarketProtectionPercentage = marketProtectionInPecent,
                TransactionType = transactionType,
                PriceType = PriceType.Limit,
                ProductType = productType,
                RetentionType = retentionType,
                Amo = amo ? "Yes" : "No",
            };

            var (orderResp, mesg) = await Api.Order.PlaceOrderAsync(orderRequest);
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
                                                    ProductType productType,
                                                    Exchange exch = Exchange.NSE,
                                                    RetentionType retentionType = RetentionType.DAY,
                                                    bool amo = false)
        {
            if (!IsMISOrCNCOrderAllowedForExchangeAndRetentionTypeCombination(exch, retentionType))
            {
                _logger.LogWarning("{0}: [{productType}] orders are not allowed for [{exch}] exchange with [{retentionType}] retention type. Converting to DAY", GetType().Name, productType, exch, retentionType);
                retentionType = RetentionType.DAY;
            }

            if (!IsTriggerPriceValid(triggerPrc, transactionType, prc))
            {
                _logger.LogError("{0}: For [{transactionType}/{tradingSymbol}] orders, trigger price should be [{1}] limit price. Discarding order.", GetType().Name, transactionType, tradingSymbol, transactionType == TransactionType.Buy ? "less than" : "greater than");
                return;
            }

            var orderRequest = new FlatTrade.OrderManager.PlaceOrderRequest
            {
                UserId = Api.Order.UserId,
                AccountId = Api.Order.AccountId,
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

            var (orderResp, mesg) = await Api.Order.PlaceOrderAsync(orderRequest);
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

        private async Task CreateOrderSLMarketAsync(string tradingSymbol, int qty,
                                                      decimal triggerPrc,
                                                      TransactionType transactionType,
                                                       decimal marketProtectionInPecent = 0.01m,
                                                      ProductType productType = ProductType.IntraDay,
                                                      Exchange exch = Exchange.NSE,
                                                      RetentionType retentionType = RetentionType.DAY,
                                                      bool amo = false)
        {
            if (!IsMISOrCNCOrderAllowedForExchangeAndRetentionTypeCombination(exch, retentionType))
            {
                _logger.LogError("{productType} orders are not allowed for {exch} exchange with {retentionType} retention type. Converting to DAY", productType, exch, retentionType);
                retentionType = RetentionType.DAY;
            }

            var prc = GetMarketLimitPriceWithProtection(tradingSymbol, marketProtectionInPecent, transactionType);
            if (!IsTriggerPriceValid(triggerPrc, transactionType, prc))
            {
                _logger.LogError("{0}: For [{transactionType}/{tradingSymbol}] orders, trigger price should be [{1}] limit price. Discarding order.", GetType().Name, transactionType, tradingSymbol, transactionType == TransactionType.Buy ? "less than" : "greater than");
                return;
            }

            var orderRequest = new FlatTrade.OrderManager.PlaceOrderRequest
            {
                UserId = Api.Order.UserId,
                AccountId = Api.Order.AccountId,
                Exchange = exch,
                TradingSymbol = tradingSymbol,
                Price = prc,
                Quantity = qty,
                MarketProtectionPercentage = marketProtectionInPecent,
                TriggerPrice = triggerPrc,
                TransactionType = transactionType,
                PriceType = PriceType.StopLossLimit,
                ProductType = productType,
                RetentionType = retentionType,
                Amo = amo ? "Yes" : "No",
            };

            var (orderResp, mesg) = await Api.Order.PlaceOrderAsync(orderRequest);
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
        private async Task ModifyOrderCOLimitAsync(long norenOrderNumber, string tradingSymbol, 
                                                  int qty, decimal prc,
                                                  decimal differentialStopPrc,
                                                  Exchange exch = Exchange.NSE,
                                                  RetentionType retentionType = RetentionType.DAY // for BSE: DAY/EOS and FOR NSE: DAY only                                                  
                                                  )
        {
            if (!IsCOOrBOOrderAllowedForExchangeAndRetentionTypeCombination(exch, retentionType))
            {
                _logger.LogError("CO orders are not allowed for {exch} exchange with {retentionType} retention type. Converting to DAY", exch, retentionType);
                retentionType = RetentionType.DAY;
            }

            var orderRequest = new FlatTrade.OrderManager.ModifyOrderRequest
            {
                UserId = Api.Order.UserId,
                NorenOrderNumber = norenOrderNumber,
                Exchange = exch,
                BookLossPrice = differentialStopPrc,
                TradingSymbol = tradingSymbol,
                Quantity = qty,
                Price = prc,
                PriceType = PriceType.Limit,
                RetentionType = retentionType,
            };

            var (orderResp, mesg) = await Api.Order.ModifyOrderAsync(orderRequest);
            if (orderResp is null)
            {
                if (mesg != Constants.StatusOk)
                    _logger.LogError("Error while placing [CO Limit] order [{orderResp.NorenOrderNumber}] : {mesg}", norenOrderNumber, mesg);
                else
                    _logger.LogInformation("No order [{orderResp.NorenOrderNumber}] placed [CO Limit] : {mesg}", norenOrderNumber, mesg);
            }
            else
            {
                _logger.LogInformation("[CO Limit] order placed [{orderResp.NorenOrderNumber}]", orderResp.NorenOrderNumber);
            }
        }

        private async Task ModifyOrderCOMarketAsync(long norenOrderNumber, string tradingSymbol, int qty, decimal differentialStopPrc, decimal marketProtectionInPecent,
                                                   TransactionType transactionType,
                                                   Exchange exch = Exchange.NSE,
                                                   RetentionType retentionType = RetentionType.DAY // for BSE: DAY/EOS and FOR NSE: DAY only                                                   
                                                   )
        {
            if (!IsCOOrBOOrderAllowedForExchangeAndRetentionTypeCombination(exch, retentionType))
            {
                _logger.LogError("CO orders are not allowed for {exch} exchange with {retentionType} retention type. Converting to DAY", exch, retentionType);
                retentionType = RetentionType.DAY;
            }

            var orderRequest = new FlatTrade.OrderManager.ModifyOrderRequest
            {
                UserId = Api.Order.UserId,
                NorenOrderNumber = norenOrderNumber,
                Exchange = exch,
                BookLossPrice = differentialStopPrc,
                TradingSymbol = tradingSymbol,
                Quantity = qty,
                Price = GetMarketLimitPriceWithProtection(tradingSymbol, marketProtectionInPecent, transactionType),
                MarketProtectionPercentage = marketProtectionInPecent,
                PriceType = PriceType.Limit,
                RetentionType = retentionType,
            };

            var (orderResp, mesg) = await Api.Order.ModifyOrderAsync(orderRequest);
            if (orderResp is null)
            {
                if (mesg != Constants.StatusOk)
                    _logger.LogError("Error while modifying [CO Market] order [{orderResp.NorenOrderNumber}] : {mesg}", norenOrderNumber, mesg);
                else
                    _logger.LogInformation("No order [{orderResp.NorenOrderNumber}] modified [CO Market] : {mesg}", norenOrderNumber, mesg);
            }
            else
            {
                _logger.LogInformation("[CO Market] order modified [{orderResp.NorenOrderNumber}]", orderResp.NorenOrderNumber);
            }
        }

        private async Task ModifyOrderCOSLLimitAsync(long norenOrderNumber, string tradingSymbol, int qty, decimal prc,
                                                    decimal differentialStopPrc, decimal triggerPrc,
                                                    TransactionType transactionType,
                                                    Exchange exch = Exchange.NSE,
                                                    RetentionType retentionType = RetentionType.DAY // for BSE: DAY/EOS and FOR NSE: DAY only                                                    
                                                    )
        {
            if (!IsCOOrBOOrderAllowedForExchangeAndRetentionTypeCombination(exch, retentionType))
            {
                _logger.LogError("CO orders are not allowed for {exch} exchange with {retentionType} retention type. Converting to DAY", exch, retentionType);
                retentionType = RetentionType.DAY;
            }

            if (!IsTriggerPriceValid(triggerPrc, transactionType, prc))
            {
                _logger.LogError("{0}: For [{transactionType}/{tradingSymbol}] orders, trigger price should be [{1}] limit price. Discarding order.", GetType().Name, transactionType, tradingSymbol, transactionType == TransactionType.Buy ? "less than" : "greater than");
                return;
            }

            var orderRequest = new FlatTrade.OrderManager.ModifyOrderRequest
            {
                UserId = Api.Order.UserId,
                NorenOrderNumber = norenOrderNumber,
                Exchange = exch,
                BookLossPrice = differentialStopPrc,

                TradingSymbol = tradingSymbol,
                Quantity = qty,
                TriggerPrice = triggerPrc,
                Price = prc,
                PriceType = PriceType.StopLossLimit,

                RetentionType = retentionType,
            };

            var (orderResp, mesg) = await Api.Order.ModifyOrderAsync(orderRequest);
            if (orderResp is null)
            {
                if (mesg != Constants.StatusOk)
                    _logger.LogError("Error while modifying [CO SL Limit] order [{orderResp.NorenOrderNumber}] : {mesg}", norenOrderNumber, mesg);
                else
                    _logger.LogInformation("No order [{orderResp.NorenOrderNumber}] modified [CO SL Limit] : {mesg}", norenOrderNumber, mesg);
            }
            else
            {
                _logger.LogInformation("[CO SL Limit] order modified [{orderResp.NorenOrderNumber}]", orderResp.NorenOrderNumber);
            }
        }

        //==================================================================================================
        private async Task CreateOrderCOLimitAsync(string tradingSymbol, int qty, decimal prc,
                                                  decimal differentialStopPrc,
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

            var orderRequest = new FlatTrade.OrderManager.PlaceOrderRequest
            {
                UserId = Api.Order.UserId,
                AccountId = Api.Order.AccountId,
                Exchange = exch,
                BookLossPrice = differentialStopPrc,
                Amo = amo ? "Yes" : "No",
                TradingSymbol = tradingSymbol,
                Quantity = qty,
                Price = prc,
                TransactionType = transactionType,
                PriceType = PriceType.Limit,
                ProductType = ProductType.HighLeverage,
                RetentionType = retentionType,
            };

            var (orderResp, mesg) = await Api.Order.PlaceOrderAsync(orderRequest);
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

            var orderRequest = new FlatTrade.OrderManager.PlaceOrderRequest
            {
                UserId = Api.Order.UserId,
                AccountId = Api.Order.AccountId,
                Exchange = exch,
                BookLossPrice = differentialStopPrc,
                Amo = amo ? "Yes" : "No",
                TradingSymbol = tradingSymbol,
                Quantity = qty,
                Price = GetMarketLimitPriceWithProtection(tradingSymbol, marketProtectionInPecent, transactionType),
                TransactionType = transactionType,
                PriceType = PriceType.Limit,
                ProductType = ProductType.HighLeverage,
                RetentionType = retentionType,
            };

            var (orderResp, mesg) = await Api.Order.PlaceOrderAsync(orderRequest);
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

        private async Task CreateOrderCOSLLimitAsync(string tradingSymbol, int qty, decimal prc,
                                                    decimal differentialStopPrc, decimal triggerPrc,
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

            if (!IsTriggerPriceValid(triggerPrc, transactionType, prc))
            {
                _logger.LogError("{0}: For [{transactionType}/{tradingSymbol}] orders, trigger price should be [{1}] limit price. Discarding order.", GetType().Name, transactionType, tradingSymbol, transactionType == TransactionType.Buy ? "less than" : "greater than");
                return;
            }

            var orderRequest = new FlatTrade.OrderManager.PlaceOrderRequest
            {
                UserId = Api.Order.UserId,
                AccountId = Api.Order.AccountId,
                Exchange = exch,
                BookLossPrice = differentialStopPrc,
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

            var (orderResp, mesg) = await Api.Order.PlaceOrderAsync(orderRequest);
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

        //==================================================================================================
        //=====================================================================================================

        private async Task CreateOrderBOLimitAsync(string tradingSymbol, int qty,
                                                  decimal prc,
                                                  decimal differentialStopPrc,
                                                  decimal differentialTargetPrc,
                                                  decimal differentialTrailingStopTicks,
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
                UserId = Api.Order.UserId,
                AccountId = Api.Order.AccountId,
                Exchange = exch,
                BookLossPrice = differentialStopPrc,
                BookProfitPrice = differentialTargetPrc,
                TrailingPrice = differentialTrailingStopTicks,
                TradingSymbol = tradingSymbol,
                Quantity = qty,
                Price = prc,
                TransactionType = transactionType,
                PriceType = PriceType.Limit,
                ProductType = ProductType.BracketOrder,
                RetentionType = retentionType,
                Amo = amo ? "Yes" : "No",
            };

            var (orderResp, mesg) = await Api.Order.PlaceOrderAsync(orderRequest);
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
        private async Task CreateOrderBOMarketAsync(string tradingSymbol, int qty,
                                                    decimal differentialStopPrc,
                                                    decimal differentialTargetPrc,
                                                    decimal differentialTrailingStopPrc,
                                                   TransactionType transactionType,
                                                   decimal marketProtectionInPecent = 0.01m,
                                                   Exchange exch = Exchange.NSE,
                                                   RetentionType retentionType = RetentionType.DAY,
                                                   bool amo = false)
        {
            if (!IsCOOrBOOrderAllowedForExchangeAndRetentionTypeCombination(exch, retentionType))
            {
                _logger.LogError("BO orders are not allowed for {exch} exchange with {retentionType} retention type. Converting to DAY", exch, retentionType);
                retentionType = RetentionType.DAY;
            }

            var orderRequest = new FlatTrade.OrderManager.PlaceOrderRequest
            {
                UserId = Api.Order.UserId,
                AccountId = Api.Order.AccountId,
                Exchange = exch,
                BookLossPrice = differentialStopPrc,
                BookProfitPrice = differentialTargetPrc,
                TrailingPrice = differentialTrailingStopPrc,
                TradingSymbol = tradingSymbol,
                Quantity = qty,
                Price = GetMarketLimitPriceWithProtection(tradingSymbol, marketProtectionInPecent, transactionType),
                TransactionType = transactionType,
                PriceType = PriceType.Limit,
                ProductType = ProductType.BracketOrder,
                RetentionType = retentionType,
                Amo = amo ? "Yes" : "No",
            };

            var (orderResp, mesg) = await Api.Order.PlaceOrderAsync(orderRequest);
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

        public async Task CreateOrderBOSLLimitAsync(string tradingSymbol, int qty, decimal prc,
                                                    decimal triggerPrc,
                                                    decimal differentialStopPrc,
                                                    decimal differentialTargetPrc,
                                                    decimal differentialTrailingStopPrc,
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

            if (!IsTriggerPriceValid(triggerPrc, transactionType, prc))
            {
                _logger.LogError("{0}: For [{transactionType}/{tradingSymbol}] orders, trigger price should be [{1}] limit price. Discarding order.", GetType().Name, transactionType, tradingSymbol, transactionType == TransactionType.Buy ? "less than" : "greater than");
                return;
            }

            var orderRequest = new FlatTrade.OrderManager.PlaceOrderRequest
            {
                UserId = Api.Order.UserId,
                AccountId = Api.Order.AccountId,
                Exchange = exch,
                BookLossPrice = differentialStopPrc,
                BookProfitPrice = differentialTargetPrc,
                TrailingPrice = differentialTrailingStopPrc,
                TradingSymbol = tradingSymbol,
                Quantity = qty,
                TriggerPrice = triggerPrc,
                Price = prc,
                TransactionType = transactionType,
                PriceType = PriceType.StopLossLimit,
                ProductType = ProductType.BracketOrder,
                RetentionType = retentionType,
                Amo = amo ? "Yes" : "No",
            };

            var (orderResp, mesg) = await Api.Order.PlaceOrderAsync(orderRequest);
            if (orderResp is null)
            {
                if (mesg != Constants.StatusOk)
                    _logger.LogError("Error while placing [BO SL Limit] order : {mesg}", mesg);
                else
                    _logger.LogInformation("No order placed [BO SL Limit] : {mesg}", mesg);
            }
            else
            {
                _logger.LogInformation("[BO SL Limit] order placed [{orderResp.NorenOrderNumber}]", orderResp.NorenOrderNumber);
            }
        }

        //=====================================================================================================
        private async Task ModifyOrderBOLimitAsync(long norenOrderNumber,
                                                  string tradingSymbol,
                                                  int qty,
                                                  decimal prc,
                                                  decimal differentialStopPrc,
                                                  decimal differentialTargetPrc,
                                                  decimal differentialTrailingStopTicks,
                                                  Exchange exch = Exchange.NSE,
                                                  RetentionType retentionType = RetentionType.DAY // for BSE: DAY/EOS and FOR NSE: DAY only                                                  
                                                  )
        {
            if (!IsCOOrBOOrderAllowedForExchangeAndRetentionTypeCombination(exch, retentionType))
            {
                _logger.LogError("BO orders are not allowed for {exch} exchange with {retentionType} retention type. Converting to DAY", exch, retentionType);
                retentionType = RetentionType.DAY;
            }

            var orderRequest = new FlatTrade.OrderManager.ModifyOrderRequest
            {
                UserId = Api.Order.UserId,
                NorenOrderNumber = norenOrderNumber,
                Exchange = exch,
                TradingSymbol = tradingSymbol,
                Quantity = qty,
                Price = prc,
                BookLossPrice = differentialStopPrc,
                BookProfitPrice = differentialTargetPrc,
                TrailingPrice = differentialTrailingStopTicks,
                               
                PriceType = PriceType.Limit,                
                RetentionType = retentionType
            };

            var (orderResp, mesg) = await Api.Order.ModifyOrderAsync(orderRequest);
            if (orderResp is null)
            {
                if (mesg != Constants.StatusOk)
                    _logger.LogError("Error while modifying [BO Limit] order [{orderResp.NorenOrderNumber}] : {mesg}", norenOrderNumber, mesg);
                else
                    _logger.LogInformation("No order [{orderResp.NorenOrderNumber}] modified [BO Limit] : {mesg}", norenOrderNumber, mesg);
            }
            else
            {
                _logger.LogInformation("[BO Limit] order modified [{orderResp.NorenOrderNumber}]", orderResp.NorenOrderNumber);
            }
        }
        private async Task ModifyOrderBOMarketAsync(long norenOrderNumber, string tradingSymbol, int qty,
                                                    decimal differentialStopPrc,
                                                    decimal differentialTargetPrc,
                                                    decimal differentialTrailingStopPrc,
                                                    TransactionType transactionType,
                                                   decimal marketProtectionInPecent = 0.01m,
                                                   Exchange exch = Exchange.NSE,
                                                   RetentionType retentionType = RetentionType.DAY)
        {
            if (!IsCOOrBOOrderAllowedForExchangeAndRetentionTypeCombination(exch, retentionType))
            {
                _logger.LogError("BO orders are not allowed for {exch} exchange with {retentionType} retention type. Converting to DAY", exch, retentionType);
                retentionType = RetentionType.DAY;
            }

            var orderRequest = new FlatTrade.OrderManager.ModifyOrderRequest
            {
                UserId = Api.Order.UserId,

                NorenOrderNumber = norenOrderNumber,
                Exchange = exch,
                BookLossPrice = differentialStopPrc,
                BookProfitPrice = differentialTargetPrc,
                TrailingPrice = differentialTrailingStopPrc,
                MarketProtectionPercentage = marketProtectionInPecent,
                TradingSymbol = tradingSymbol,
                Quantity = qty,
                Price = GetMarketLimitPriceWithProtection(tradingSymbol, marketProtectionInPecent, transactionType),
                PriceType = PriceType.Limit,
                RetentionType = retentionType
            };

            var (orderResp, mesg) = await Api.Order.ModifyOrderAsync(orderRequest);
            if (orderResp is null)
            {
                if (mesg != Constants.StatusOk)
                    _logger.LogError("Error while modifying [BO Limit] order [{orderResp.NorenOrderNumber}]: {mesg}", norenOrderNumber, mesg);
                else
                    _logger.LogInformation("No order [{orderResp.NorenOrderNumber}] modified [BO Limit] : {mesg}", norenOrderNumber, mesg);
            }
            else
            {
                _logger.LogInformation("[BO Limit] order modified [{orderResp.NorenOrderNumber}]", orderResp.NorenOrderNumber);
            }
        }

        public async Task ModifyOrderBOSLLimitAsync(long norenOrderNumber, string tradingSymbol, 
                                                    int qty, decimal prc,
                                                    decimal differentialStopPrc,
                                                    decimal differentialTargetPrc,
                                                    decimal differentialTrailingStopPrc,
                                                    Exchange exch = Exchange.NSE,
                                                    RetentionType retentionType = RetentionType.DAY)
        {
            if (!IsCOOrBOOrderAllowedForExchangeAndRetentionTypeCombination(exch, retentionType))
            {
                _logger.LogError("BO orders are not allowed for {exch} exchange with {retentionType} retention type. Converting to DAY", exch, retentionType);
                retentionType = RetentionType.DAY;
            }

            var orderRequest = new FlatTrade.OrderManager.ModifyOrderRequest
            {
                UserId = Api.Order.UserId,

                Exchange = exch,
                BookLossPrice = differentialStopPrc,
                BookProfitPrice = differentialTargetPrc,
                TrailingPrice = differentialTrailingStopPrc,
                TradingSymbol = tradingSymbol,
                Quantity = qty,
                Price = prc,
                PriceType = PriceType.Limit,
                RetentionType = retentionType
            };

            var (orderResp, mesg) = await Api.Order.ModifyOrderAsync(orderRequest);
            if (orderResp is null)
            {
                if (mesg != Constants.StatusOk)
                    _logger.LogError("Error while modifying [BO SL Limit] order [{orderResp.NorenOrderNumber}] : {mesg}", norenOrderNumber, mesg);
                else
                    _logger.LogInformation("No order [{orderResp.NorenOrderNumber}] modified [BO SL Limit] : {mesg}", norenOrderNumber, mesg);
            }
            else
            {
                _logger.LogInformation("[BO SL Limit] order modified [{orderResp.NorenOrderNumber}]", orderResp.NorenOrderNumber);
            }
        }
    }
    //==================================================================================================
    //==================================================================================================
}
