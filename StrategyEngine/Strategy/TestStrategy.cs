using FlatTrade;
using FlatTrade.Common.Types;
using FlatTrade.Common.Types.Base;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using StrategyEngine.Model;
using StrategyEngine.RMS;
using System.Collections.Concurrent;

namespace StrategyEngine.Strategy
{
    internal class TestStrategy(IConfiguration config, Api api, IRMS rmsManager, IOrderProcessor orderProcessor, ILoggerFactory loggerFactory) 
        : AbstractBaseStrategy<TestStrategy>(config, api, rmsManager, orderProcessor, loggerFactory)
    {        
        protected override string Name => $"{GetType().Name}";

        private ConcurrentDictionary<ChartInterval, (PriceCandle, StreamWriter)> _lastCandle = [];
        private static readonly object _fileLock = new();

        protected override Task<StrategySignal?> ProcessInternal(StrategyOnScripSnapshot input)
        {
            return Task.FromResult<StrategySignal?>(default);
        }

        protected override Task<StrategySignal?> ProcessInternal(StrategyOnHoldingSnapshot input)
        {
            return Task.FromResult<StrategySignal?>(default);
        }
        protected override Task<StrategySignal?> ProcessInternal(StrategyOnOrderSnapshot input)
        {
            return Task.FromResult<StrategySignal?>(default);
        }
        protected override Task<StrategySignal?> ProcessInternal(StrategyOnPositionSnapshot input)
        {
            return Task.FromResult<StrategySignal?>(default);
        }

        protected override Task<StrategySignal?> ProcessInternal(StrategyOnTradeSnapshot input)
        {
            return Task.FromResult<StrategySignal?>(default);
        }

        protected override Task<StrategySignal?> ProcessInternal(StrategyOnQuoteSnapshot input)
        {
            return Task.FromResult<StrategySignal?>(default);
        }

        protected override Task<StrategySignal?> ProcessInternal(StrategyOnTouchLineSnapshot input)
        {
            return Task.FromResult<StrategySignal?>(default);
        }
        protected override Task<StrategySignal?> ProcessInternal(StrategyOnCandleSnapshot input)
        {
            foreach(var candle in input.Candles)
            {
                var lastCandle = _lastCandle.GetOrAdd(input.ChartInterval, key =>
                {
                    var fileName = $"..//..//..//{(int)key}_candles.csv";
                    var writer = new StreamWriter(fileName, append: true)
                    {
                        AutoFlush = false
                    };
                    return (candle, writer);
                });

                if (lastCandle.Item1 != candle) //StartTimeStamp comparison only.
                {
                    lock (_fileLock)
                    {
                        lastCandle.Item2.WriteLine($"{lastCandle.Item1.StartTimeStamp},{lastCandle.Item1.Open},{lastCandle.Item1.High},{lastCandle.Item1.Low},{lastCandle.Item1.Close},{lastCandle.Item1.Volume}");
                        lastCandle.Item2.Flush(); // or flush every few writes
                    }
                }
                
                _lastCandle.AddOrUpdate(input.ChartInterval, (candle, lastCandle.Item2), (_,_) => (candle, lastCandle.Item2));
            }
            return Task.FromResult<StrategySignal?>(default);
        }
    }
}
