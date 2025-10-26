using StrategyEngine.Strategy;

namespace StrategyEngine.RMS
{
    internal class MaxLossLimitPerDay : IRMS
    {
        public Task<bool> IsValidationSucceeded(StrategySignal strategySignal)
        {
            throw new NotImplementedException();
        }
    }
}
