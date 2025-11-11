using FlatTrade;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StrategyEngine.Model;
using StrategyEngine.OrderProcessors.SingleLegOrder;

namespace StrategyEngine.OrderProcessors.BackTesting
{
    internal class BackTestingOrderProcessor(IConfiguration config, Api api, ILoggerFactory loggerFactory)
        : AsbtractOrderProcessor<SingleLegManagedOrderProcessor>(config, api, loggerFactory)
    {
        protected override string Name => $"{GetType().Name}";

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
