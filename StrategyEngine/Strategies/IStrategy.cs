namespace StrategyEngine.Strategies
{
    internal delegate Task OnUpdate(object obj);

    internal interface IStrategy
    {
        Task Process(object? obj);
    }
}
