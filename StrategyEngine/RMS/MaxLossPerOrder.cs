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
        private readonly bool _enabled = true;
        protected override string Name => $"{GetType().Name}_RmsRule";
        private readonly decimal _configuredMaxLossPerOrder = 300;

        public override async Task<bool> IsValidationSucceeded(StrategySignal strategySignal)
        {
            if (!_enabled)
                return true;

            var validationSucceeded = false;
            if (!validationSucceeded)
            {
                await base.WriteToDb(strategySignal, Name, $"ConfiguredMaxLossPerOrder ({_configuredMaxLossPerOrder}) is brached. More parameters later.");
            }
            return validationSucceeded;
        }

        protected override Task OnUpdateInternal(StrategyOnTradeSnapshot input)
        {
            return Task.CompletedTask;
        }

    }
}
