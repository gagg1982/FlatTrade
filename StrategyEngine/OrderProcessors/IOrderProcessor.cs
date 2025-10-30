using StrategyEngine.Model;

namespace StrategyEngine.OrderProcessors
{
    public interface IOrderProcessor
    {
        public Task CancelOrder(CancelOrder cancelOrder);
        public Task ModifyOrder(ModifyOrder modifyOrder);
        public Task CreateOrder(CreateOrder createOrder);        
    }
}
