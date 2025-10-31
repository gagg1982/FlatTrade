namespace StrategyEngine.Strategies
{
    internal delegate Task OnUpdate(object obj);

    internal interface IStrategy: IDisposable, IAsyncDisposable
    {
        Task Process(object? obj);
    }
}
