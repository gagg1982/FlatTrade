using StrategyEngine.Model;

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
