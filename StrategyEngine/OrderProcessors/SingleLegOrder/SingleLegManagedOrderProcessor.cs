using FlatTrade;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StrategyEngine.Model;
using StrategyEngine.RMS;
using StrategyEngine.Strategies;

namespace StrategyEngine.OrderProcessors.SingleLegOrder
{
    //Single leg MANAGED order processor automatically setup 2 more orders once the parent order is completed/rejected/cancelled
    // Leg-1 order for stop loss  (depands on input parameters)
    // Leg-2 order for target profit  (depands on input parameters)
    // Automatically manages the life of other order on the completion of one in above orders.
    // It has equivalent support from Broker: Check MultiLegOrderProcessor class which is straight and simple.
    // For that it needs to subscribe for order updates.

    class OrderHolder
    {
        public required OrderInfo BrokerParentOrder { get; set; }
        public required OrderInfo BrokerStopLossOrder { get; set; }
        public required OrderInfo BrokerTargetProfitOrder { get; set; }
    }
    internal class SingleLegManagedOrderProcessor(IConfiguration config, Api api,ILoggerFactory loggerFactory)
        : AsbtractOrderProcessor<SingleLegManagedOrderProcessor>(config, api, loggerFactory)

    {
        protected override string Name => $"{GetType().Name}";

        protected override Task OnUpdateInternal(StrategyOnOrderSnapshot input)
        {
            return base.OnUpdateInternal(input);
        }

        public override Task CancelOrder(CancelOrder cancelOrder)
        {
            throw new NotImplementedException();
        }

        public override Task CreateOrder(CreateOrder createOrder)
        {
            throw new NotImplementedException();
        }

        public override Task ModifyOrder(ModifyOrder modifyOrder)
        {
            throw new NotImplementedException();
        }
        
    }
}
