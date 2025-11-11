using FlatTrade;
using FlatTrade.Common.Types.Base;
using FlatTrade.MarketInfoManager;
using FlatTrade.OrderManager;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StrategyEngine.Model;
using StrategyEngine.OrderProcessors;
using System.Collections.Concurrent;
using System.Diagnostics.Eventing.Reader;

namespace StrategyEngine.RMS
{
    internal class SufficientBalance : AbstractRms<SufficientBalance>
    {
        private readonly bool _enabled = false;
        private readonly decimal _bufferReservedFromInitialCashAllocation = 0;
        private readonly decimal _initialCashAllocated = 0;
        private readonly string _userId = string.Empty;
        private readonly string _accountId = string.Empty;
        private ConcurrentDictionary<long, decimal> _totalTradeFees = new();
        protected override string Name => $"{GetType().Name}_RmsRule";

        public SufficientBalance(IConfiguration config, Api api, IOrderProcessor orderProcessor, ILoggerFactory loggerFactory)
        :base(config, api, orderProcessor, loggerFactory)
        {
            var rule = DynamicConfigHelper.GetRule(config, "RmsRules", GetType().Name);

            if (rule is null)
            {
                _logger.LogWarning("RMS:Rule [{0}] not configured in config ", GetType().Name);
                return;
            }
            _enabled = rule.GetBool("Enabled");
            if (!_enabled)
            {
                _logger.LogWarning("RMS:Rule [{0}] not enabled in config ", GetType().Name);
            }

            _initialCashAllocated = rule.GetInt("InitialCashAllocation");
            _bufferReservedFromInitialCashAllocation = rule.GetDecimal("BufferReservedFromInitialCashAllocation") * _initialCashAllocated / 100;

            _userId = Api.Order.UserId;
            _accountId = Api.Order.AccountId;            
        }

        public override async Task<bool> IsValidationSucceeded(StrategySignal strategySignal)
        {
            if (!_enabled)
                return true;            

            OrderMarginRequest? orderMarginRequest = null;
            if (strategySignal.OutputDecision.OrderEventType == OrderEventType.CreateOrder)
            {
                if (!ValidateProductTypePriceTypeTransactionTypeTriggerAndLimitPriceCombination(strategySignal.OutputDecision.CreateOrder!.TransactionType,
                                                                            strategySignal.OutputDecision.CreateOrder!.ProductType,
                                                                            strategySignal.OutputDecision.CreateOrder!.PriceType,
                                                                            strategySignal.OutputDecision.CreateOrder!.TriggerPrice,
                                                                            strategySignal.OutputDecision.CreateOrder!.LimitPrice))
                    return false;

                orderMarginRequest = CreateOrderMarginRequest(strategySignal.OutputDecision.CreateOrder!);
            }
            else if (strategySignal.OutputDecision.OrderEventType == OrderEventType.ModifyOrder)
            {
                if (!ValidateProductTypePriceTypeTransactionTypeTriggerAndLimitPriceCombination(strategySignal.OutputDecision.ModifyOrder!.TransactionType,
                                                                            strategySignal.OutputDecision.ModifyOrder!.ProductType,
                                                                            strategySignal.OutputDecision.ModifyOrder!.PriceType,
                                                                            strategySignal.OutputDecision.ModifyOrder!.TriggerPrice,
                                                                            strategySignal.OutputDecision.ModifyOrder!.LimitPrice))
                    return false;
                orderMarginRequest = ModifyOrderMarginRequest(strategySignal.OutputDecision.ModifyOrder!);
            }
            else
            {
                return true;
            }

            if (orderMarginRequest is null)
                return false;

            var (orderMargin, msg) = await Api.Order.GetOrderMarginAsync(orderMarginRequest);                                                   
            if (orderMargin is null)
            {                
                _logger.LogWarning("RMS:Rule [{0}] Validation Failed: Unable to get OrderMargin info. Error: {1}", Name, msg);
                return false;
            }

            decimal totalCharges = 0;
            if (strategySignal.OutputDecision.OrderEventType == OrderEventType.CreateOrder)
            {
                var (brokerageResponse, msg1) = await Api.MarketInfo.GetBrokerageAsync(strategySignal.OutputDecision.CreateOrder!.TransactionType,
                                                                           strategySignal.OutputDecision.CreateOrder!.Exchange,
                                                                           strategySignal.OutputDecision.CreateOrder!.ProductType,
                                                                           strategySignal.OutputDecision.CreateOrder!.TradingSymbol,
                                                                           strategySignal.OutputDecision.CreateOrder!.LimitPrice,
                                                                           strategySignal.OutputDecision.CreateOrder!.Quantity);
                if (brokerageResponse is null)
                {
                    _logger.LogWarning("RMS:Rule [{0}] Validation Failed: Unable to get brokerage info. Error: {1}", Name, msg1);
                    return false;
                }
                totalCharges = brokerageResponse.TotalCharges;
            }

            var totalTradeFees = _totalTradeFees.Values.Sum();

            var availableAfterOrderFullFillment = (_initialCashAllocated - _bufferReservedFromInitialCashAllocation) - (orderMargin.TotalMarginUsed + totalCharges + totalTradeFees);
            if (availableAfterOrderFullFillment < 0 )
            {
                _logger.LogWarning("RMS:Rule [{0}] Validation Failed: Insufficient balance for order. Required: {1}, Available: {2}", Name, orderMargin.OrderMargin + totalCharges, _initialCashAllocated - _bufferReservedFromInitialCashAllocation - orderMargin.MarginUsedPreviously - totalTradeFees);
                return false;
            }
            return true;
        }

        private bool ValidateProductTypePriceTypeTransactionTypeTriggerAndLimitPriceCombination(TransactionType transactionType, 
                                                                            ProductType productType,
                                                                            PriceType priceType, 
                                                                            decimal triggerPrice,
                                                                            decimal limitPrice)
        {
            switch (priceType)
            {
                case PriceType.StopLossLimit:
                    if (transactionType == TransactionType.Buy)
                    {
                        if (triggerPrice >= limitPrice)
                        {
                            _logger.LogError("RMS:Rule [{0}] Validation Failed. For PriceType [{1}], TransactionType [{2}], TriggerPrice [{3}] should be less than LimitPrice [{4}]",
                                                                                    Name,
                                                                                    priceType,
                                                                                    transactionType,
                                                                                    triggerPrice,
                                                                                    limitPrice);
                            return false;
                        }
                    }
                    else
                    {
                        if (triggerPrice <= limitPrice)
                        {
                            _logger.LogError("RMS:Rule [{0}] Validation Failed. For PriceType [{1}], TransactionType [{2}], TriggerPrice [{3}] should be greater than LimitPrice [{4}]",
                                                                                    Name,
                                                                                    priceType,
                                                                                    transactionType,
                                                                                    triggerPrice,
                                                                                    limitPrice);
                            return false;
                        }
                    }
                    break;
                case PriceType.StopLossMarket:
                    if (productType == ProductType.BracketOrder || productType == ProductType.HighLeverage)
                    {
                        _logger.LogError("RMS:Rule [{0}] Validation Failed. For PriceType [{1}], Invalid ProductType [{2}] is given",
                                                                                       Name,
                                                                                       priceType,
                                                                                       productType);
                        return false;
                    }

                    if (transactionType == TransactionType.Buy)
                    {
                        if (triggerPrice <= limitPrice)
                        {
                            _logger.LogError("RMS:Rule [{0}] Validation Failed. For PriceType [{1}], TransactionType [{2}], TriggerPrice [{3}] should be greater than LimitPrice [{4}]",
                                                                                    Name,
                                                                                    priceType,
                                                                                    transactionType,
                                                                                    triggerPrice,
                                                                                    limitPrice);
                            return false;
                        }
                    }
                    else
                    {
                        if (triggerPrice >= limitPrice)
                        {
                            _logger.LogError("RMS:Rule [{0}] Validation Failed. For PriceType [{1}], TransactionType [{2}], TriggerPrice [{3}] should be less than LimitPrice [{4}]",
                                                                                    Name,
                                                                                    priceType,
                                                                                    transactionType,
                                                                                    triggerPrice,
                                                                                    limitPrice);
                            return false;
                        }
                    }
                    break;
                default:
                    break;
            }
            return true;
        }
        private OrderMarginRequest? CreateOrderMarginRequest(CreateOrder createOrder)
        {
            switch(createOrder.ProductType)
            {
                case ProductType.IntraDay:
                case ProductType.Delivery:
                    return new OrderMarginRequest
                    {
                        AccountId = _accountId,
                        UserId = _userId,
                        Exchange = createOrder!.Exchange,
                        TradingSymbol = createOrder!.TradingSymbol,
                        Quantity = createOrder!.Quantity,
                        Price = createOrder!.PriceType == PriceType.Market ||
                                                                createOrder!.PriceType == PriceType.StopLossMarket ?
                                                                                     0 : createOrder!.LimitPrice,
                        ProductType = createOrder!.ProductType,
                        TransactionType = createOrder!.TransactionType,
                        PriceType = createOrder!.PriceType,
                        TriggerPrice = createOrder!.PriceType == PriceType.StopLossLimit ||
                                                                       createOrder!.PriceType == PriceType.StopLossMarket ?
                                                                                 createOrder!.TriggerPrice : 0,
                    };                
                case ProductType.BracketOrder:
                    return new OrderMarginRequest
                    {
                        AccountId = _accountId,
                        UserId = _userId,
                        Exchange = createOrder!.Exchange,
                        TradingSymbol = createOrder!.TradingSymbol,
                        Quantity = createOrder!.Quantity,
                        Price = createOrder!.PriceType == PriceType.Market ? 0 : createOrder!.LimitPrice,
                        ProductType = createOrder!.ProductType,
                        TransactionType = createOrder!.TransactionType,
                        PriceType = createOrder!.PriceType,
                        TriggerPrice = createOrder!.PriceType == PriceType.StopLossLimit ?
                                                createOrder!.TriggerPrice : 0,
                        BookLossProfit =  createOrder!.DifferentialSLPrice 
                    };
                case ProductType.HighLeverage:
                    return new OrderMarginRequest
                    {
                        AccountId = _accountId,
                        UserId = _userId,
                        Exchange = createOrder!.Exchange,
                        TradingSymbol = createOrder!.TradingSymbol,
                        Quantity = createOrder!.Quantity,
                        Price = createOrder!.PriceType == PriceType.Market ? 0 : createOrder!.LimitPrice,
                        ProductType = createOrder!.ProductType,
                        TransactionType = createOrder!.TransactionType,
                        PriceType = createOrder!.PriceType,
                        TriggerPrice = createOrder!.PriceType == PriceType.StopLossLimit ?
                                                createOrder!.TriggerPrice : 0,
                        BookLossProfit = createOrder!.DifferentialSLPrice
                    };
                default:
                    _logger.LogError("RMS:Rule {0} Validation Failed. Unknown ProductType [{1}]", Name, createOrder!.ProductType);
                    break;
            };            
            return default;
        }

        private OrderMarginRequest? ModifyOrderMarginRequest(ModifyOrder modifyOrder)
        {
            if (modifyOrder.RemainingOriginalQuantity == 0 || modifyOrder.RemainingOriginalLimitPrice == 0.0m)
            {
                _logger.LogError("RMS:Rule {0} Validation Failed. ModifyOrder received with invalid RemainingOriginalQuantity [{1}], RemainingOriginalLimitPrice [{2}]", Name, modifyOrder.RemainingOriginalQuantity, modifyOrder.RemainingOriginalLimitPrice);
                return default;
            }
            
            var orderMarginRequest = CreateOrderMarginRequest(modifyOrder);
            if (orderMarginRequest is null)
                return default;
            
            orderMarginRequest.RemainingOriginalPriceFromModify = modifyOrder.RemainingOriginalLimitPrice;
            orderMarginRequest.RemaningOriginalQuantityFromModify = modifyOrder.RemainingOriginalQuantity;

            return orderMarginRequest;
        }

        protected override async Task OnUpdateInternal(StrategyOnOrderSnapshot input)
        {
            if (!_enabled)
                return;

            var (brokerageResponse, msg) = await Api.MarketInfo.GetBrokerageAsync(input.orderInfo.TransactionType,
                                                                           input.orderInfo.Exchange,
                                                                           input.orderInfo.ProductType,
                                                                           input.orderInfo.TradingSymbol,
                                                                           input.orderInfo.Price,
                                                                           (long)input.orderInfo.Quantity);
            if (brokerageResponse is null)
            {
                _logger.LogWarning("RMS:Rule [{0}] Fees calculation Failed: Unable to get brokerage info during order [{1}] update. Error: {2}", Name, input.orderInfo.NorenOrderNumber, msg);
                return;
            }
            if (input.orderInfo.OrderStatus == OrderStatus.Rejected ||
                input.orderInfo.OrderStatus == OrderStatus.Cancelled ||
                input.orderInfo.OrderStatus == OrderStatus.AmoCancelled )
                _totalTradeFees.Remove(input.orderInfo.NorenOrderNumber, out decimal _);
            else if (input.orderInfo.OrderStatus == OrderStatus.Completed ||
                     input.orderInfo.OrderStatus == OrderStatus.Open ||
                     input.orderInfo.OrderStatus == OrderStatus.AmoOpen)
                _totalTradeFees.AddOrUpdate(input.orderInfo.NorenOrderNumber, brokerageResponse.TotalCharges, (_, existing) =>
                {
                    existing = brokerageResponse.TotalCharges;
                    return existing;
                });
        }
    }
}
