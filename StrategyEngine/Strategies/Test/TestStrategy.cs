using FlatTrade;
using FlatTrade.Types.Base;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StrategyEngine.Model;
using System.Collections.Concurrent;

namespace StrategyEngine.Strategies.Test
{
    internal class TestStrategy(IConfiguration config, Api api, ILoggerFactory loggerFactory) 
        : AbstractStrategy<TestStrategy>(config, api, loggerFactory), IAsyncDisposable, IDisposable
    {
        private ConcurrentDictionary<string, DateTime> _lastAccessTime = [];
        private TimeSpan _startTradingTime = new TimeSpan(9,15,0);
        private TimeSpan _endTradingTime = new TimeSpan(15,14,0);
        private decimal _riskRewardRatio = 1.5m;
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

        protected override Task<StrategySignal?> ProcessInternal(StrategyOnTouchLineSnapshot input)
        {
            var now = DateTime.Now;

            if (now.TimeOfDay >= _startTradingTime 
                && now.TimeOfDay <= _endTradingTime 
                && now >= _lastAccessTime.GetOrAdd(input.updates.TradingSymbol, DateTime.MinValue).AddMinutes(5)
                && !AnyPendingOrder(input.updates.TradingSymbol, input.updates.Token)
                && !AnyPendingPosition(input.updates.TradingSymbol, ProductType.IntraDay))
            {
                _lastAccessTime.AddOrUpdate(input.updates.TradingSymbol, now, (_,_) => now);
                int value = Random.Shared.Next(0, 2);

                return Task.FromResult<StrategySignal?>(
                    new StrategySignal(Guid.NewGuid(),
                        Name,
                        new OutputDecision.Create
                        (
                            new CreateOrder
                            {
                                DifferentialProfitPrice = 2,
                                DifferentialSLPrice = 1,
                                LimitPrice = value == 0 ? input.updates.LastTradePrice - input.updates.TickSize:
                                                          input.updates.LastTradePrice + input.updates.TickSize,
                                Exchange = input.updates.Exchange,
                                TradingSymbol = input.updates.TradingSymbol,
                                Token = input.updates.Token,
                                TransactionType = value == 0 ? TransactionType.Buy : TransactionType.Sell,
                                Quantity = 10,
                                //DifferentialTrailingTicks = 1.15m,
                                //MarketProtectionInPercent = 0.01m,
                                
                                ProductType = ProductType.BracketOrder,
                                
                                //TriggerPrice = value == 0 ? input.updates.LastTradePrice - input.updates.TickSize :
                                //                            input.updates.LastTradePrice + input.updates.TickSize
                            }
                        ),
                        new DecisionMakingInputs { DecisionMakingRemarks = "testStrategy, will add more input params later"}
                    ));
            }

            return Task.FromResult<StrategySignal?>(default);
        }
       
        protected override  async ValueTask DisposeAsyncCore()
        {
            if (_disposed)
                return;

            // Dispose async resources
            foreach(var (key,writer) in _intervalWriter)
            {
                if (writer is not null)
                    await writer.DisposeAsync();
            }
            await base.DisposeAsyncCore().ConfigureAwait(false);

            _logger.LogInformation("{0}: Disposed gracefully", GetType().Name);
            // Dispose other sync-only resources here (e.g., timers, files)
        }

    }
}
