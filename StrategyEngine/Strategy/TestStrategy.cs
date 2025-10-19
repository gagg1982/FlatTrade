using FlatTrade;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StrategyEngine.Model;
using StrategyEngine.RMS;

namespace StrategyEngine.Strategy
{
    internal class TestStrategy : IStrategy
    {
        private readonly ILogger<TestStrategy> _logger;
        private readonly ILoggerFactory _loggerFactory;

        private readonly IEnumerable<StrategyEngineEventType> _strategyEngineEventTypesSubscription =[];
        private readonly IRMS _rmsManager;
        private readonly Api _api;

        internal TestStrategy(IConfiguration config, Api api, IRMS rmsManager, IEnumerable<StrategyEngineEventType> strategyEngineEventTypes,  ILoggerFactory loggerFactory)
        {
            _api = api;
            _loggerFactory = loggerFactory ?? new LoggerFactory();
            _logger = _loggerFactory.CreateLogger<TestStrategy>();
            _rmsManager= rmsManager;
            _strategyEngineEventTypesSubscription = strategyEngineEventTypes ?? [];
        }

        public IEnumerable<StrategyEngineEventType> GetStrategyEngineEventTypes()
        {
            return [StrategyEngineEventType.Holdings,
                    StrategyEngineEventType.Trades,
                    StrategyEngineEventType.Candles,
                    StrategyEngineEventType.Quotes,
                    StrategyEngineEventType.Orders,
                    StrategyEngineEventType.Securities,
                    StrategyEngineEventType.Positions,
                    StrategyEngineEventType.TouchLine];
        }

        public Task<StrategySignal?> Process(StrategyEvent strategyEvent)
        {

            //if (_rmsManager.IsValidationSucceeded(signal!))
            {

            }

            return Task.FromResult<StrategySignal?>(default);            
        }
    }
}
