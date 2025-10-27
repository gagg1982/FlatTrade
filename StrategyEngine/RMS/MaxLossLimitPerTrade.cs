using StrategyEngine.Model;

namespace StrategyEngine.RMS
{
    internal class MaxLossLimitPerTrade : IRMS
    {
        public Task<bool> IsValidationSucceeded(StrategySignal strategySignal)
        {
            throw new NotImplementedException();
        }
    }
}
