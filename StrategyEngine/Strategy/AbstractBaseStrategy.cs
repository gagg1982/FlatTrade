using FlatTrade;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StrategyEngine.Model;
using StrategyEngine.RMS;

namespace StrategyEngine.Strategy
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
                _logger.LogWarning("Strategy_{0}: received null object to process. ", Name);
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
            catch (Microsoft.CSharp.RuntimeBinder.RuntimeBinderException ex)
            {
                _logger.LogInformation("Strategy_{0}: Error: {1}", Name, ex);
            }
        }

        protected virtual Task<StrategySignal?> ProcessInternal(object input)
        {
            return Task.FromResult<StrategySignal?>(default);
        }
    }
}
