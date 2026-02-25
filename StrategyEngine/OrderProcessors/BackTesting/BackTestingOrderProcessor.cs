using FlatTrade;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StrategyEngine.OrderProcessors.SingleLegOrder;

namespace StrategyEngine.OrderProcessors.BackTesting
{   
    internal class BackTestingOrderProcessor(IConfiguration config, Api api, ILoggerFactory loggerFactory) 
        : SingleLegBaseManagedOrderProcessor(config, api, loggerFactory)
    {
        protected override string Name => $"{GetType().Name}";
    }
}
