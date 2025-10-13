
namespace StrategyEngine.Strategy
{
    internal interface IStrategy
    {
        Task<StrategySignal?> Process(StrategyEvent strategyEvent);
    }
}
