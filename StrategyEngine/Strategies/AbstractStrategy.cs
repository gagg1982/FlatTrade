using FlatTrade;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StrategyEngine.Model;

namespace StrategyEngine.Strategies
{
    internal abstract class AbstractStrategy<T> : IStrategy
    {
        protected ILoggerFactory _loggerFactory;
        protected ILogger<T> _logger;
        protected abstract string Name { get; }

        protected readonly IConfiguration Config;
        protected readonly Api Api;

        internal AbstractStrategy(IConfiguration config, Api api, ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory ?? new LoggerFactory();
            _logger = _loggerFactory.CreateLogger<T>();
            Config = config;
            Api = api;
        }

        public async Task<StrategySignal?> Process(object? obj) => await ProcessInternal((dynamic)obj!); 
        
        protected virtual Task<StrategySignal?> ProcessInternal(object input)
        {
            return input switch
            {
                StrategyOnScripSnapshot scrip => ProcessInternal(scrip),
                StrategyOnHoldingSnapshot holding => ProcessInternal(holding),
                StrategyOnOrderSnapshot order => ProcessInternal(order),
                StrategyOnPositionSnapshot position => ProcessInternal(position),
                StrategyOnTradeSnapshot trade => ProcessInternal(trade),
                StrategyOnQuoteSnapshot quote => ProcessInternal(quote),
                StrategyOnTouchLineSnapshot touch => ProcessInternal(touch),
                _ => Task.FromResult<StrategySignal?>(default)
            };
        }

        // Default implementations — derived classes can override only what they need
        protected virtual Task<StrategySignal?> ProcessInternal(StrategyOnCandleSnapshot input) => Task.FromResult<StrategySignal?>(default);
        protected virtual Task<StrategySignal?> ProcessInternal(StrategyOnScripSnapshot input) => Task.FromResult<StrategySignal?>(default);
        protected virtual Task<StrategySignal?> ProcessInternal(StrategyOnHoldingSnapshot input) => Task.FromResult<StrategySignal?>(default);
        protected virtual Task<StrategySignal?> ProcessInternal(StrategyOnOrderSnapshot input) => Task.FromResult<StrategySignal?>(default);
        protected virtual Task<StrategySignal?> ProcessInternal(StrategyOnPositionSnapshot input) => Task.FromResult<StrategySignal?>(default);
        protected virtual Task<StrategySignal?> ProcessInternal(StrategyOnTradeSnapshot input) => Task.FromResult<StrategySignal?>(default);
        protected virtual Task<StrategySignal?> ProcessInternal(StrategyOnQuoteSnapshot input) => Task.FromResult<StrategySignal?>(default);
        protected virtual Task<StrategySignal?> ProcessInternal(StrategyOnTouchLineSnapshot input) => Task.FromResult<StrategySignal?>(default);
    }
}
