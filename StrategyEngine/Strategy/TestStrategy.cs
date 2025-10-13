using FlatTrade;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StrategyEngine.Model;

namespace StrategyEngine.Strategy
{
    internal class TestStrategy : IStrategy
    {
        private readonly ILogger<TestStrategy> _logger;
        private readonly ILoggerFactory _loggerFactory;

        private readonly Api _api;

        internal TestStrategy(IConfiguration config, Api api, ILoggerFactory loggerFactory)
        {
            _api = api;
            _loggerFactory = loggerFactory ?? new LoggerFactory();
            _logger = _loggerFactory.CreateLogger<TestStrategy>();

        }
        public Task<StrategySignal?> Process(StrategyEvent strategyEvent)
        {
            return Task.FromResult<StrategySignal?>(default);            
        }
    }
}
