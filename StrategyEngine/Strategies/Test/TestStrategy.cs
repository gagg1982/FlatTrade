using FlatTrade;
using FlatTrade.Common.Types.Base;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StrategyEngine.Model;
using StrategyEngine.OrderProcessors;
using StrategyEngine.RMS;
using System.Collections.Concurrent;

namespace StrategyEngine.Strategies.Test
{
    internal class TestStrategy(IConfiguration config, Api api, IRMS rmsManager, IOrderProcessor orderProcessor, ILoggerFactory loggerFactory) 
        : AbstractBaseStrategy<TestStrategy>(config, api, rmsManager, orderProcessor, loggerFactory)
    {
        protected override string Name => $"{GetType().Name}";

        private readonly ConcurrentDictionary<(ChartInterval, string), StreamWriter> _intervalWriter = [];
        
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
            var intervalWriter = _intervalWriter.GetOrAdd((input.ChartInterval,input.TradingSymbol), key =>
            {
                var fileName = $"..//..//..//{(int)key.Item1}_{key.Item2}_candles.csv";
                var writer = new StreamWriter(new FileStream(fileName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                {
                    AutoFlush = true
                };
                return writer ;
            });

            lock (intervalWriter)
            {
                foreach (var candle in input.Candles.Reverse())
                    intervalWriter.WriteLine($"S:{input.TradingSymbol}, T:{candle.StartTimeStamp}, O:{candle.Open}, H:{candle.High}, L:{candle.Low}, C:{candle.Close}, V:{candle.Volume}, Pseudo:{candle.PseudoFlag}, Accum:{candle.AccumulatedVolume}");                
            }

            return Task.FromResult<StrategySignal?>(default);
        }
        
        protected virtual void Dispose(bool disposing)
        {
            DisposeAsyncCore(disposing).AsTask().GetAwaiter().GetResult(); // Safe sync fallback
            base.Dispose();
            GC.SuppressFinalize(this);
        }

        
        protected virtual async ValueTask DisposeAsync(bool disposing)
        {
            await DisposeAsyncCore(disposing);
            await base.DisposeAsync();
            GC.SuppressFinalize(this);
        }

        private async ValueTask DisposeAsyncCore(bool disposing)
        {
            if (disposing)
                return;

            disposing = true;

            // Dispose async resources
            foreach(var (key,writer) in _intervalWriter)
            {
                if (writer is not null)
                    await writer.DisposeAsync();
            }

            _logger.LogInformation("{0}: Disposed gracefully", GetType().Name);
            // Dispose other sync-only resources here (e.g., timers, files)
        }

    }
}
