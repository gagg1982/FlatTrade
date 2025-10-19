
using StrategyEngine.Model;

namespace StrategyEngine.Strategy
{
    internal interface IStrategy
    {
        Task<StrategySignal?> Process(StrategyEvent strategyEvent);
        IEnumerable<StrategyEngineEventType> GetStrategyEngineEventTypes();
    }
}
