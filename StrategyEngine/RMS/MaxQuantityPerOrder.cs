using FlatTrade;
using FlatTrade.Common.Helpers;
using FlatTrade.Common.Types.Base;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StrategyEngine.Model;
using StrategyEngine.OrderProcessors;

namespace StrategyEngine.RMS
{
    // Dont allow the new order to be placed if order breaches the threshold quantity
    // Cancel the existing order if the threshold breaches somehow (might be manually).
    internal class MaxQuantityPerOrder : AbstractRms<MaxQuantityPerOrder>
    {
        protected override string Name => $"{GetType().Name}_RmsRule";

        private readonly bool _enabled = false;
        private int _maxQuantityPerOrder = 100;
        private List<Task> _tasks = [];
        private readonly ConcurrentSortedList<long> _cancelledOrders = new();

        public MaxQuantityPerOrder(IConfiguration config, Api api, IOrderProcessor orderProcessor, ILoggerFactory loggerFactory)
            : base(config, api, orderProcessor, loggerFactory)
        {
            var rule = DynamicConfigHelper.GetRule(config, "RmsRules", GetType().Name );

            if (rule is null)
            {
                _logger.LogWarning("RMS:Rule [{0}] not configured in config ", GetType().Name);
                return;
            }
            _enabled = rule.GetBool("Enabled");
            if(!_enabled)
            {
                _logger.LogWarning("RMS:Rule [{0}] not enabled in config ", GetType().Name);
            }

            _maxQuantityPerOrder = rule.GetInt("ThresholdQuantity");
        }
        public override Task<bool> IsValidationSucceeded(StrategySignal strategySignal)
        {
            if (!_enabled)
                return Task.FromResult(true);

            if (strategySignal.OutputDecision.OrderEventType == OrderEventType.CreateOrder)                
            {
                if (strategySignal.OutputDecision.CreateOrder!.Quantity > _maxQuantityPerOrder)
                {
                    _logger.LogWarning("RMS:Rule [{0}] Validation Failed: Order quantity [{1}] > Max quantity allowed [{2}]",
                        Name,
                        strategySignal.OutputDecision.CreateOrder!.Quantity,
                        _maxQuantityPerOrder);
                    return Task.FromResult(false);
                }
            }
            else if (strategySignal.OutputDecision.OrderEventType == OrderEventType.ModifyOrder)
            {
                if (strategySignal.OutputDecision.ModifyOrder!.Quantity > _maxQuantityPerOrder)
                {
                    _logger.LogWarning("RMS:Rule [{0}] Validation Failed: Order quantity [{1}] > Max quantity allowed [{2}]",
                        Name,
                        strategySignal.OutputDecision.CreateOrder!.Quantity,
                        _maxQuantityPerOrder);
                    return Task.FromResult(false);
                }
            }
            return Task.FromResult(true);
        }

        protected override Task OnUpdateInternal(StrategyOnOrderSnapshot input)
        {
            //check during order execution if the orderStatus is Open or OpenAmo,
            //and quantity is more than _maxQuantityPerOrder, then cancel that order.
            if (!_enabled)
                return Task.CompletedTask;

            if((input.orderInfo.OrderStatus == OrderStatus.Open ||
               input.orderInfo.OrderStatus == OrderStatus.AmoOpen) &&
               input.orderInfo.Quantity > _maxQuantityPerOrder)
            {
                _cancelledOrders.Add(input.orderInfo.NorenOrderNumber);
                _logger.LogInformation("RMS:Rule [{0}] Order [{1}] cancellation request submitted during update.", Name, input.orderInfo.NorenOrderNumber);
                _tasks.Add(OrderProcessor.CancelOrder(new CancelOrder { NorenOrderNumber = input.orderInfo.NorenOrderNumber }));
                return Task.CompletedTask;
            }

            if ((input.orderInfo.OrderStatus == OrderStatus.Cancelled || 
                input.orderInfo.OrderStatus == OrderStatus.AmoCancelled) &&
                _cancelledOrders.Remove(input.orderInfo.NorenOrderNumber))
            {
                _logger.LogInformation("RMS:Rule [{0}] Order [{1}] cancelled during update.", Name, input.orderInfo.NorenOrderNumber);
                return Task.CompletedTask;
            }

            foreach (var norenOrderNumber in _cancelledOrders.Snapshot())
            {
                _tasks.Add(OrderProcessor.CancelOrder(new CancelOrder { NorenOrderNumber = norenOrderNumber }));
            }

            if(_cancelledOrders.Snapshot().Contains(input.orderInfo.NorenOrderNumber))
            {
                _logger.LogWarning("RMS:Rule [{0}] Order [{1}] cancellation not completed during update. Order status [{2}]. Discarding and not retrying.", Name, input.orderInfo.NorenOrderNumber, input.orderInfo.OrderStatus);
                _cancelledOrders.Remove(input.orderInfo.NorenOrderNumber);                
            }
            return Task.CompletedTask;
        }
    }
}
