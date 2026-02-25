using FlatTrade;
using FlatTrade.Types.Base;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StrategyEngine.Model;
using StrategyEngine.OrderProcessors;
using System.Collections.Concurrent;

namespace StrategyEngine.RMS
{
    internal class MaxLossPerDay(IConfiguration config, Api api, IOrderProcessor orderProcessor, ILoggerFactory loggerFactory)
     : AbstractRms<MaxLossPerDay>(config, api, orderProcessor, loggerFactory)
    {
        private ConcurrentDictionary<TransactionType, IEnumerable<StrategyOnTradeSnapshot>> _profitAndLossInfo = [];
        protected override string Name => $"{GetType().Name}_RmsRule";

        private readonly bool _enabled = true;
        private readonly decimal _configuredMaxLossPerDay = 2000;

        public override async Task<bool> IsValidationSucceeded(StrategySignal strategySignal)
        {
            if (!_enabled)
                return true;

            var validationSucceeded = false;
            if(!validationSucceeded)
            {
               await base.WriteToDb(strategySignal, Name, $"ConfiguredMaxLossPerDay ({_configuredMaxLossPerDay}) is brached. More parameters later.");
            }
            return validationSucceeded;
        }

        protected override Task OnUpdateInternal(StrategyOnTradeSnapshot input)
        {
            return Task.CompletedTask;
        }
    }
}
