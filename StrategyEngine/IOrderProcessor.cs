using StrategyEngine.Model;

namespace StrategyEngine
{
    public interface IOrderProcessor
    {
        public Task CancelOrder(CancelOrder cancelOrder);
        public Task ModifyOrder(ModifyOrder modifyOrder);
        public Task CreateOrder(CreateOrder createOrder);        
    }
}
