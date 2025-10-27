using StrategyEngine.Model;

namespace StrategyEngine.RMS
{
    public interface IRMS
    {
        public Task<bool> IsValidationSucceeded(StrategySignal signal);
    }
}
