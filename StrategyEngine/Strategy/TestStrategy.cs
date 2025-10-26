using FlatTrade;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StrategyEngine.RMS;

namespace StrategyEngine.Strategy
{
    internal class TestStrategy(IConfiguration config, Api api, IRMS rmsManager, IOrderProcessor orderProcessor, ILoggerFactory loggerFactory) 
        : AbstractBaseStrategy<TestStrategy>(config, api, rmsManager, orderProcessor, loggerFactory)
    {        
        protected override string Name => "Test";

        protected override Task<StrategySignal?> ProcessInternal(object input)
        {
            return base.ProcessInternal(input);
        }

    }
}
