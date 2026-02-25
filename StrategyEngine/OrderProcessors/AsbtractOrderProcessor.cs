using Common.Helpers;
using FlatTrade;
using FlatTrade.Types.Base;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StrategyEngine.Model;
using System.Collections.Concurrent;


namespace StrategyEngine.OrderProcessors
{
    internal abstract class AsbtractOrderProcessor<T>: IOrderProcessor
    {
        protected ILoggerFactory _loggerFactory;
        protected ILogger<T> _logger;
        protected abstract string Name { get; }

        protected readonly IConfiguration Config;
        protected readonly Api Api;
        
        internal AsbtractOrderProcessor(IConfiguration config, Api api, ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory ?? new LoggerFactory();
            _logger = _loggerFactory.CreateLogger<T>();
            Config = config;
            Api = api;
        }        

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

        public abstract Task CancelOrder(string strategyName, CancelOrder cancelOrder);
        public abstract Task ModifyOrder(string strategyName, ModifyOrder modifyOrder);
        public abstract Task CreateOrder(string strategyName, CreateOrder createOrder);

    }
}
