using StrategyEngine.Model;

namespace StrategyEngine
{
    internal class BackTestingOrderProcessor : IOrderProcessor
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
