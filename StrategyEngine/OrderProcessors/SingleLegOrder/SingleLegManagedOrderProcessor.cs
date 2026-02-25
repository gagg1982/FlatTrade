using FlatTrade;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StrategyEngine.Model;

namespace StrategyEngine.OrderProcessors.SingleLegOrder
{
    //Single leg MANAGED order processor automatically setup 2 more orders once the parent order is completed/rejected/cancelled
    // Leg-1 order for stop loss  (depands on input parameters)
    // Leg-2 order for target profit  (depands on input parameters)
    // Automatically manages the life of other order on the completion of one in above orders.
    // It has equivalent support from Broker: Check MultiLegOrderProcessor class which is straight and simple.
    // For that it needs to subscribe for order updates.
    
    internal class SingleLegManagedOrderProcessor(IConfiguration config, Api api, ILoggerFactory loggerFactory) :
        SingleLegBaseManagedOrderProcessor(config, api, loggerFactory)

    {
        ExchangeOrderProcessor _exchangeOrderProcessor = new ExchangeOrderProcessor(config, api, loggerFactory);

        protected override string Name => $"{GetType().Name}";       

        public override async Task CancelOrder(string strategyName, CancelOrder cancelOrder)
        {
            await base.CancelOrder(strategyName, cancelOrder);
            await _exchangeOrderProcessor.CancelOrder(strategyName, cancelOrder);
        }

        public override async Task CreateOrder(string strategyName, CreateOrder createOrder)
        {
            await base.CreateOrder(strategyName, createOrder);
            await _exchangeOrderProcessor.CreateOrder(strategyName, createOrder);
        }

        public override async Task ModifyOrder(string strategyName, ModifyOrder modifyOrder)
        {
            await base.ModifyOrder(strategyName, modifyOrder);
            await _exchangeOrderProcessor.ModifyOrder(strategyName, modifyOrder);
        }
    }
}
