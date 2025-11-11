using StrategyEngine.Model;

namespace StrategyEngine.RMS
{
    public interface IRms
    {
        public Task OnUpdate(object? obj);
        public Task<bool> IsValidationSucceeded(StrategySignal signal);        
    }
}
