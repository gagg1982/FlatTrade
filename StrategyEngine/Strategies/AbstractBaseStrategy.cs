using FlatTrade;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StrategyEngine.Model;
using StrategyEngine.OrderProcessors;
using StrategyEngine.RMS;

namespace StrategyEngine.Strategies
{
    internal abstract class AbstractBaseStrategy<T> : IStrategy
    {
        protected ILoggerFactory _loggerFactory;
        protected ILogger<T> _logger;
        protected abstract string Name { get; }

        private readonly IOrderProcessor _orderProcessor;

        protected readonly IConfiguration Config;
        protected readonly Api Api;
        protected readonly IRMS RmsManager;

        internal AbstractBaseStrategy(IConfiguration config, Api api, IRMS rmsManager, IOrderProcessor orderProcessor, ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory ?? new LoggerFactory();
            _logger = _loggerFactory.CreateLogger<T>();
            Config = config;
            Api = api;
            RmsManager = rmsManager;
            _orderProcessor = orderProcessor;
        }

        public async Task Process(object? obj)
        {
            if (obj is null)
            {
                _logger.LogWarning("{0}: received null object to process. ", Name);
                return;
            }
            try
            {
                var signal = await ProcessInternal((dynamic)obj);
                if (signal is null)
                    return;

                if (!await RmsManager.IsValidationSucceeded(signal))
                    return;

                return;
                switch (signal.OutputDecision.OrderEventType)
                {
                    case OrderEventType.Create:
                        if (signal.OutputDecision.CreateOrder is not null)
                            await _orderProcessor.CreateOrder(signal.OutputDecision.CreateOrder!);
                        break;
                    case OrderEventType.Modify:
                        if (signal.OutputDecision.ModifyOrder is not null)
                            await _orderProcessor.ModifyOrder(signal.OutputDecision.ModifyOrder);
                        break;
                    case OrderEventType.Cancel:
                        if (signal.OutputDecision.CancelOrder is not null)
                            await _orderProcessor.CancelOrder(signal.OutputDecision.CancelOrder);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation("{0}: Error: {1}", Name, ex);
            }
        }

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
