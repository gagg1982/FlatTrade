
using FlatTrade;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StrategyEngine.Model;

namespace StrategyEngine.OrderProcessors.SingleLegOrder
{
    //Single leg UNMANAGED order processor only triggers the order it is requested for.
    // No other orders gets placed
    // It doesnt need any order update subscription and hence it is the reponsibility of caller to close the open positions if any.
    internal class SingleLegUnmanagedOrderProcessor(IConfiguration config, Api api, ILoggerFactory loggerFactory)
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
