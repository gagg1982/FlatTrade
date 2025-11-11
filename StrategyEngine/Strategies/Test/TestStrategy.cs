using FlatTrade;
using FlatTrade.Common.Types.Base;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StrategyEngine.Model;
using System.Collections.Concurrent;
using TicTacTec.TA.Library;

namespace StrategyEngine.Strategies.Test
{
    internal class TestStrategy(IConfiguration config, Api api, ILoggerFactory loggerFactory) 
        : AbstractStrategy<TestStrategy>(config, api, loggerFactory)
    {
        private bool _disposed = false;
        protected override string Name => $"{GetType().Name}";

        private readonly ConcurrentDictionary<(ChartInterval, string), StreamWriter> _intervalWriter = [];
                
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
        
        public virtual void Dispose()
        {
            DisposeAsyncCore().AsTask().GetAwaiter().GetResult(); // Safe sync fallback
            GC.SuppressFinalize(this);
        }
        
        public virtual async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore();
            GC.SuppressFinalize(this);
        }

        private async ValueTask DisposeAsyncCore()
        {
            if (_disposed)
                return;

            _disposed = true;

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
