using FlatTrade;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StrategyEngine.Model;
using StrategyEngine.OrderProcessors;

namespace StrategyEngine.RMS
{
    internal class MaxLossPerOrder(IConfiguration config, Api api, IOrderProcessor orderProcessor, ILoggerFactory loggerFactory)
    : AbstractRms<MaxLossPerOrder>(config, api, orderProcessor, loggerFactory)
    {
        protected override string Name => $"{GetType().Name}_RmsRule";

        public override Task<bool> IsValidationSucceeded(StrategySignal strategySignal) => Task.FromResult(true);

        protected override Task OnUpdateInternal(StrategyOnTradeSnapshot input)
        {
            return Task.CompletedTask;
        }

    }
}
