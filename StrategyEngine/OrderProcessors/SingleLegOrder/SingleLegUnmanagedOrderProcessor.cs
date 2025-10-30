
using StrategyEngine.Model;

namespace StrategyEngine.OrderProcessors.SingleLegOrder
{
    //Single leg UNMANAGED order processor only triggers the order it is requested for.
    // No other orders gets placed
    // It doesnt need any order update subscription and hence it is the reponsibility of caller to close the open positions if any.
    internal class SingleLegUnmanagedOrderProcessor : IOrderProcessor
    {
        public Task CancelOrder(CancelOrder cancelOrder)
        {
            throw new NotImplementedException();
        }

        public Task CreateOrder(CreateOrder createOrder)
        {
            throw new NotImplementedException();
        }

        public Task ModifyOrder(ModifyOrder modifyOrder)
        {
            throw new NotImplementedException();
        }
    }
}
