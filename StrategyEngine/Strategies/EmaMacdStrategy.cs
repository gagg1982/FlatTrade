using FlatTrade;
using FlatTrade.Types.Base;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StrategyEngine.BrokerData;
using StrategyEngine.Model;
using StrategyEngine.Strategies.Indicators;
using StrategyEngine.Strategies.Test;


namespace StrategyEngine.Strategies
{
    internal class EmaMacdStrategy(IConfiguration config, Api api, ILoggerFactory loggerFactory)
        : AbstractStrategy<TestStrategy>(config, api, loggerFactory)
    {
        protected override string Name => $"{GetType().Name}";
        private const int FastEma = 12;
        private const int SlowEma = 26;
        private const ChartInterval WorkingChartInterval = ChartInterval.Five;

        protected override Task<StrategySignal?> ProcessInternal(StrategyOnCandleSnapshot input)
        {

            if(input.ChartInterval != WorkingChartInterval ||
               input.Candles.Count < SlowEma)
                return Task.FromResult<StrategySignal?>(default);
            
            var candles = input.Candles;

            var closes = candles.Select(c => (double)c.Close).ToArray();
            var emaFast = Technicals.Ema(closes, FastEma);
            var emaSlow = Technicals.Ema(closes, SlowEma);
            var (macd, signal, hist) = Technicals.Macd(closes);

            var latestIdx = emaSlow.Length - 1;
            var prevIdx = emaSlow.Length - 2;

            bool bullishCross = emaFast[prevIdx] <= emaSlow[prevIdx] &&
                                emaFast[latestIdx] > emaSlow[latestIdx];

            bool bearishCross = emaFast[prevIdx] >= emaSlow[prevIdx] &&
                                emaFast[latestIdx] < emaSlow[latestIdx];

            bool macdUp = macd.Last() > signal.Last();
            bool macdDown = macd.Last() < signal.Last();

            if (bullishCross && macdUp)
            {
                string decisionMakingRemarks = $"Buy Signal At {candles.Last().StartTimeStamp}," +
                                               $" EMA and MACD bullish crossover," +
                                               $" EMA Fast: {emaFast.Last():F2}," +
                                               $" EMA Slow: {emaSlow.Last():F2}," +
                                               $" MACD: {macd.Last():F2}," +
                                               $" Signal: {signal.Last():F2}";
                _logger.LogDebug(decisionMakingRemarks);
                return Task.FromResult<StrategySignal?>(CreateSignal(input.TradingSymbol,
                                                                     input.Token,
                                                                     input.Exchange,
                                                                     TransactionType.Buy,
                                                                     decisionMakingRemarks));
            }

            if (bearishCross && macdDown)
            {
                string decisionMakingRemarks = $"Sell Signal At {candles.Last().StartTimeStamp}," +
                                               $" EMA and MACD bearish crossover," +
                                               $" EMA Fast: {emaFast.Last():F2}," +
                                               $" EMA Slow: {emaSlow.Last():F2}," +
                                               $" MACD: {macd.Last():F2}," +
                                               $" Signal: {signal.Last():F2}";
                _logger.LogInformation(decisionMakingRemarks);
                return Task.FromResult<StrategySignal?>(CreateSignal(input.TradingSymbol,
                                                                     input.Token,
                                                                     input.Exchange, 
                                                                     TransactionType.Sell,
                                                                     decisionMakingRemarks));
            }

            return Task.FromResult<StrategySignal?>(default);
        }

        private StrategySignal CreateSignal(string tradingSymbol, long token, Exchange exchange,TransactionType transactionType, string decisionMakingRemarks)
        {

            return new StrategySignal(Guid.NewGuid(), Name, 
                new OutputDecision.Create(
                    new CreateOrder
                    { 
                        Exchange = exchange,
                        TradingSymbol = tradingSymbol,
                        Token = token,
                        TransactionType = transactionType,
                        PriceType = PriceType.Limit,

                        LimitPrice = 0, // This should be set based on your strategy's logic
                        DifferentialSLPrice = 0, // This should be set based on your strategy's logic
                        DifferentialProfitPrice = 0, // This should be set based on your strategy's logic
                        Quantity = 1 // This should be set based on your strategy's logic
                    }),
                new DecisionMakingInputs { DecisionMakingRemarks = decisionMakingRemarks });
        }
    }
}
