using StrategyEngine.Model;

namespace StrategyEngine.OrderProcessors
{
    public interface IOrderProcessor
    {
        public Task OnUpdate(object? obj);
        public Task CancelOrder(string strategyName, CancelOrder cancelOrder);
        public Task ModifyOrder(string strategyName, ModifyOrder modifyOrder);
        public Task CreateOrder(string strategyName, CreateOrder createOrder);        
    }
}
