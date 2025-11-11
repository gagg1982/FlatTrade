using FlatTrade;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StrategyEngine.Model;
using StrategyEngine.OrderProcessors;

namespace StrategyEngine.RMS
{
    internal abstract class AbstractRms<T> : IRms
    {
        protected ILoggerFactory _loggerFactory;
        protected ILogger<T> _logger;
        protected abstract string Name { get; }

        protected readonly IOrderProcessor OrderProcessor;
        protected readonly IConfiguration Config;
        protected readonly Api Api;

        internal AbstractRms(IConfiguration config, Api api, IOrderProcessor orderProcessor, ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory ?? new LoggerFactory();
            _logger = _loggerFactory.CreateLogger<T>();
            Config = config;
            OrderProcessor = orderProcessor;
            Api = api;
        }

        public virtual Task<bool> IsValidationSucceeded(StrategySignal signal) => Task.FromResult(true);

        public async Task OnUpdate(object? obj) => await OnUpdateInternal((dynamic)obj!);

        protected virtual Task OnUpdateInternal(object input)
        {
            return input switch
            {
                StrategyOnScripSnapshot scrip => OnUpdateInternal(scrip),
                StrategyOnHoldingSnapshot holding => OnUpdateInternal(holding),
                StrategyOnOrderSnapshot order => OnUpdateInternal(order),
                StrategyOnPositionSnapshot position => OnUpdateInternal(position),
                StrategyOnTradeSnapshot trade => OnUpdateInternal(trade),
                StrategyOnQuoteSnapshot quote => OnUpdateInternal(quote),
                StrategyOnTouchLineSnapshot touch => OnUpdateInternal(touch),
                _ => Task.CompletedTask
            };
        }

        // Default implementations — derived classes can override only what they need
        protected virtual Task OnUpdateInternal(StrategyOnCandleSnapshot input) => Task.CompletedTask;
        protected virtual Task OnUpdateInternal(StrategyOnScripSnapshot input) => Task.CompletedTask;
        protected virtual Task OnUpdateInternal(StrategyOnHoldingSnapshot input) => Task.CompletedTask;
        protected virtual Task OnUpdateInternal(StrategyOnOrderSnapshot input) => Task.CompletedTask;
        protected virtual Task OnUpdateInternal(StrategyOnPositionSnapshot input) => Task.CompletedTask;
        protected virtual Task OnUpdateInternal(StrategyOnTradeSnapshot input) => Task.CompletedTask;
        protected virtual Task OnUpdateInternal(StrategyOnQuoteSnapshot input) => Task.CompletedTask;
        protected virtual Task OnUpdateInternal(StrategyOnTouchLineSnapshot input) => Task.CompletedTask;

    }
}
