using StrategyEngine.Strategy;

namespace StrategyEngine.RMS
{
    public interface IRMS
    {
        public bool IsValidationSucceeded(StrategySignal signal);
    }
}
