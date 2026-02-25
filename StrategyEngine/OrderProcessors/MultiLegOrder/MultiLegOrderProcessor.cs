using FlatTrade;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StrategyEngine.Model;
using StrategyEngine.OrderProcessors.SingleLegOrder;

namespace StrategyEngine.OrderProcessors.MultiLegOrder
{
    internal class MultiLegOrderProcessor(IConfiguration config, Api api, ILoggerFactory loggerFactory)
        : AsbtractOrderProcessor<SingleLegManagedOrderProcessor>(config, api, loggerFactory)
    {
        protected override string Name => $"{GetType().Name}";

        public override Task CancelOrder(string strategyName, CancelOrder cancelOrder)
        {
            throw new NotImplementedException();
        }

        public override Task CreateOrder(string strategyName, CreateOrder createOrder)
        {
            throw new NotImplementedException();
        }

        public override Task ModifyOrder(string strategyName, ModifyOrder modifyOrder)
        {
            throw new NotImplementedException();
        }    
    }
}
