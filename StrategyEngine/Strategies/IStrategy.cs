using StrategyEngine.Model;

namespace StrategyEngine.Strategies
{
    internal delegate Task OnUpdate(object obj);

    internal interface IStrategy
    {
        Task<StrategySignal?> Process(object? obj);
    }
}
