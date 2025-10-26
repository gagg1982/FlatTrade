using StrategyEngine.Strategy;

namespace StrategyEngine.RMS
{
    public interface IRMS
    {
        public Task<bool> IsValidationSucceeded(StrategySignal signal);
    }
}
