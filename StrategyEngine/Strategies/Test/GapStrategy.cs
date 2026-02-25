using FlatTrade;
using Common.Helpers;
using FlatTrade.Types;
using FlatTrade.Types.Base;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StrategyEngine.Model;
using System.Collections.Concurrent;
using Common.Types;

namespace StrategyEngine.Strategies.Test
{
    internal enum Gap
    {
        Up,
        Down,
        None,
    }
    internal class StrategyInfo
    {
        internal PriceCandle? PreviousDayCandle { get; set; } = null;
        internal decimal? CurrentDayOpeningPrice { get; set; } = null;
    };

    internal class GapStrategy(IConfiguration config, Api api, ILoggerFactory loggerFactory)
        : AbstractStrategy<GapStrategy>(config, api, loggerFactory), IAsyncDisposable, IDisposable
    {
        private ConcurrentDictionary<string, StrategyInfo> _strategyInfo = [];
        private static TimeSpan _startTradingTime = new TimeSpan(9, 14, 55);
        private static TimeSpan _endTradingTime = new TimeSpan(15, 14, 0);
        private static decimal _riskRewardRatio = 1.5m;
        private static int _fixedQuantityPerOrder = 100;
        private static decimal _limitPriceAdjustedTick = 1; //i.e. limit order price adjusted to 1 tick. Buyprice -1 tick, sellprice +1 tick
        private static decimal _gapStartConditionInPerentage = 1.0m;
        private static decimal _gapEndConditionInPerentage = 8.0m;

        protected override string Name => $"{GetType().Name}";

        private readonly ConcurrentDictionary<(ChartInterval, string), StreamWriter> _intervalWriter = [];

        protected override Task<StrategySignal?> ProcessInternal(StrategyOnTouchLineSnapshot input)
        {            
            if(_strategyInfo.TryGetValue(input.updates.TradingSymbol, out StrategyInfo? outVal) &&
                outVal is not null &&
                outVal.CurrentDayOpeningPrice is not null &&
                outVal.PreviousDayCandle is not null &&
                !AnyPendingOrder(input.updates.TradingSymbol, input.updates.Token) &&
                !AnyPendingPosition(input.updates.TradingSymbol, ProductType.IntraDay))
            {
                return Task.FromResult<StrategySignal?>(default);
            }

            var now = DateTime.Now;
            if (now.TimeOfDay >= _startTradingTime && now.TimeOfDay <= _endTradingTime)
            {
                var strategyInfo = _strategyInfo.AddOrUpdate(input.updates.TradingSymbol, 
                                            new StrategyInfo
                                            {
                                                CurrentDayOpeningPrice = input.updates.Open,                     
                                            },
                                            (_, existing) =>
                                            {
                                                lock(existing)
                                                {
                                                    existing.CurrentDayOpeningPrice ??= input.updates.Open;                                                                                       
                                                }
                                                return existing;
                                            });

                if(strategyInfo.PreviousDayCandle is null ||
                   strategyInfo.CurrentDayOpeningPrice is null)
                {
                    return Task.FromResult<StrategySignal?>(default);
                }

                var (gap, perct) = FindGap(strategyInfo.PreviousDayCandle, strategyInfo.CurrentDayOpeningPrice ?? 0.0m);

                if (gap == Gap.None)
                {
                    _logger.LogInformation("[{0}]: No gap found for [{1}]. CurrentDayOpen [{2}], PreviousDayClose [{3}], GapPercentage [{4}%], ConfiguredThreshold [+/- {5}% to +/- {6}%]",
                                            Name, input.updates.TradingSymbol, strategyInfo.CurrentDayOpeningPrice,
                                            strategyInfo.PreviousDayCandle.Close, perct, 
                                            _gapStartConditionInPerentage, _gapEndConditionInPerentage);
                    return Task.FromResult<StrategySignal?>(default);
                }
                
                var transactionType = (gap == Gap.Down) ? TransactionType.Buy : TransactionType.Sell ; // 0 = Gap.Down (take long) else 1 = Gap.Up (take short)
                var currentdayOpeningPrice = strategyInfo.CurrentDayOpeningPrice ?? 0.0m;
                var previousDayClosingPrice = strategyInfo.PreviousDayCandle.Close;

                var differentialProfitPrice = transactionType == TransactionType.Buy ?
                                               previousDayClosingPrice - currentdayOpeningPrice
                                             : currentdayOpeningPrice - previousDayClosingPrice;

                var differentialSLPrice = transactionType == TransactionType.Buy ?
                            Helpers.Utility.RoundToTickWithPrecision(currentdayOpeningPrice - ((previousDayClosingPrice - currentdayOpeningPrice) / _riskRewardRatio), input.updates.TickSize, input.updates.PricePrecision)
                          : Helpers.Utility.RoundToTickWithPrecision(currentdayOpeningPrice + ((currentdayOpeningPrice - previousDayClosingPrice) / _riskRewardRatio), input.updates.TickSize, input.updates.PricePrecision);


                return Task.FromResult<StrategySignal?>(
                    new StrategySignal(Guid.NewGuid(),
                        Name,
                        new OutputDecision.Create
                        (
                            new CreateOrder
                            {
                                DifferentialProfitPrice = differentialProfitPrice,
                                DifferentialSLPrice = differentialSLPrice,
                                LimitPrice = transactionType == TransactionType.Buy ? 
                                          currentdayOpeningPrice - (_limitPriceAdjustedTick * input.updates.TickSize) :
                                          currentdayOpeningPrice + (_limitPriceAdjustedTick * input.updates.TickSize),
                                Exchange = input.updates.Exchange,
                                TradingSymbol = input.updates.TradingSymbol,
                                Token = input.updates.Token,
                                TransactionType = transactionType,
                                Quantity = _fixedQuantityPerOrder,
                                //DifferentialTrailingTicks = 1.15m,
                                //MarketProtectionInPercent = 0.01m,
                                PriceType = PriceType.Limit,
                                ProductType = ProductType.BracketOrder,                                
                                //TriggerPrice = value == 0 ? input.updates.LastTradePrice - input.updates.TickSize :
                                //                            input.updates.LastTradePrice + input.updates.TickSize
                            }
                        ),
                        new DecisionMakingInputs { DecisionMakingRemarks = "GapUpDown Strategy, will add more params later" }
                    ));
            }

            return Task.FromResult<StrategySignal?>(default);
        }

        private static (Gap, decimal) FindGap(PriceCandle priceCandle, decimal openingPrice)
        {
            decimal gapPercent = Math.Round(((openingPrice - priceCandle.Close) / priceCandle.Close) * 100, 4);
            if(Math.Abs(gapPercent) > _gapStartConditionInPerentage && Math.Abs(gapPercent) <= _gapEndConditionInPerentage)
            {
                if(gapPercent < 0.0m)
                    return (Gap.Up, gapPercent);
                return (Gap.Down, gapPercent);
            }
            return (Gap.None, gapPercent);
        }

        protected override Task<StrategySignal?> ProcessInternal(StrategyOnCandleSnapshot input)
        {            
            if(input.ChartInterval != ChartInterval.Daily)
                return Task.FromResult<StrategySignal?>(default);

            if (_strategyInfo.TryGetValue(input.TradingSymbol, out StrategyInfo? outVal) && outVal is not null && outVal.PreviousDayCandle is not null)
                return Task.FromResult<StrategySignal?>(default);

            var previousDayDate = DateTime.Now.Date.GetBusinessDaysAgo(1);
            if(input.Candles.TryGetValue(new PriceCandle { StartTimeStamp = previousDayDate }, out PriceCandle? previousDayCandle) || previousDayCandle is null)
            {
                _strategyInfo.AddOrUpdate(input.TradingSymbol,
                                          new StrategyInfo 
                                          {
                                               PreviousDayCandle = previousDayCandle
                                          },
                                          (_, existing) =>
                                          {
                                              lock (existing)
                                              {
                                                  existing.PreviousDayCandle ??= previousDayCandle;
                                              }
                                              return existing;
                                          });
                                    
            }
            return Task.FromResult<StrategySignal?>(default);
        }      

        protected override async ValueTask DisposeAsyncCore()
        {
            if (!_disposed)
                return;

            // Dispose async resources
            foreach (var (key, writer) in _intervalWriter)
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
