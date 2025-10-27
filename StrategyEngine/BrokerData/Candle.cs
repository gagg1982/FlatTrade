using FlatTrade;
using FlatTrade.Common.Helpers;
using FlatTrade.Common.Types;
using FlatTrade.Common.Types.Base;
using FlatTrade.MarketInfoManager;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StrategyEngine.Helpers;
using StrategyEngine.Model;
using StrategyEngine.Strategy;
using System.Collections.Concurrent;

namespace StrategyEngine.BrokerData
{
    internal class Candle
    {
        private readonly IConfiguration _config;
        private readonly Api _api;
        private readonly ILogger _logger;
        private readonly ContextAccessor _contextAccessor;

        private event OnUpdate? _onCandles;


        private static IEnumerable<ChartInterval> _priceIntervals = [ChartInterval.One,
                                                                    ChartInterval.Three, ChartInterval.Five,
                                                                    ChartInterval.Ten, ChartInterval.Fifteen,
                                                                    ChartInterval.Thirty];

        public Candle(IConfiguration config, ContextAccessor contextAccessor, Api api, OnUpdate? onUpdate, ILoggerFactory loggerFactory)
        {
            _api = api;
            _logger = loggerFactory.CreateLogger<Candle>();
            _config = config;
            _contextAccessor = contextAccessor;
            _onCandles += onUpdate;
        }

        public async Task UpdateCandlesAsync(string tradingSymbol, Exchange exchange, long token, decimal price, long quantity, DateTime tradeDateTime)
        {
            List<Task> tasks = [];

            var details = GlobalDataSet.Data.GetOrAdd(tradingSymbol, _ => new Details());
            var intervalCandle = details.PriceCandleInfo.GetOrAdd(exchange, _ => new(_priceIntervals.ToDictionary( key => key, key => new SortedSet<PriceCandle>())));

            foreach (var (interval, candle) in intervalCandle)
            {
                if (price == decimal.MinValue &&
                    intervalCandle.TryGetValue(interval, out SortedSet<PriceCandle>? existingCandles) &&
                    existingCandles is not null &&
                    existingCandles.Any())
                {
                    price = existingCandles.Last().Close;
                }

                var priceCandle = new PriceCandle
                {
                    Close = price,
                    Open = price,
                    High = price,
                    Low = price,
                    Volume = quantity,
                    StartTimeStamp = Utility.AlignToInterval(tradeDateTime, (int)interval).ToLocalTime()
                };

                SortedSet<PriceCandle> holder = [];
                var modified = true;
                var existing = intervalCandle.GetOrAdd(interval, _ => new SortedSet<PriceCandle>());

                lock (existing)
                {
                    var candleToBeUpdated = existing.LastOrDefault(c => c.StartTimeStamp == priceCandle.StartTimeStamp);
                    if (candleToBeUpdated is null)
                        existing.Add(priceCandle);
                    else
                        modified = modified || candleToBeUpdated.UpdateCandle(priceCandle);

                    if (modified)
                        holder = [.. existing.GetViewBetween(candleToBeUpdated, candleToBeUpdated)];
                }


                if (_onCandles is not null && modified)
                {
                    await _onCandles(new StrategyOnCandleSnapshot
                    {
                        ChartInterval = interval,
                        Exchange = exchange,
                        Token = token,
                        TradingSymbol = tradingSymbol,
                        Candles = holder
                    });
                }
            }
        }

        public async Task UpdateCandlesAsync(IEnumerable<SelectedSymbol> selection, int lastXDaysCandle = 5)
        {
            if (_priceIntervals.Contains(ChartInterval.Daily))
                _logger.LogWarning("Daily interval candles cannot be fetched using TimePriceData API. Continuing for rest of the intervals...");

            List<Task> tasks = [];
            var initStartDate = DateTime.Now.Date.GetBusinessDaysAgo(lastXDaysCandle);
            var endDate = DateTime.Now.Date.AddDays(1).ToLocalTime();
            tasks.Add(Task.Run(async () =>
            {
                foreach (var item in selection)
                {
                    var (resp, msg) = await _api.MarketInfo.GetTimePriceDataAsync(item.Exchange, item.TradingSymbol, initStartDate, endDate, ChartInterval.One);
                    msg = $"{item.Exchange} {item.TradingSymbol} OHLCV data ({(int)ChartInterval.One} min) for start date {initStartDate} and end date {endDate}. {msg}";
                    if (resp is null || !resp.Any())
                    {
                        _logger.LogError("NOK: {msg}", msg);
                        continue;
                    }

                    var intervalCandles = GenerateIntervalCandles(_priceIntervals, resp);

                    var details = GlobalDataSet.Data.GetOrAdd(item.TradingSymbol, _ => new Details());
                    var priceCandleDict = details.PriceCandleInfo.GetOrAdd(item.Exchange, _ => new());

                    foreach (var (interval, candles) in intervalCandles)
                    {
                        SortedSet<PriceCandle> holder = [];
                        var modified = true;

                        var existing = priceCandleDict.GetOrAdd(interval, _ => new SortedSet<PriceCandle>());

                        lock (existing)
                        {
                            foreach (var candle in candles)
                            {
                                var existingCandle = existing.LastOrDefault(c => c.StartTimeStamp == candle.StartTimeStamp);
                                if (existingCandle is null)
                                    existing.Add(candle);
                                else
                                    modified = modified || existingCandle.UpdateCandle(candle);
                            }

                            if (modified)
                            {
                                var start = Utility.AlignToInterval(candles.First().StartTimeStamp, (int)interval);
                                var end = Utility.AlignToInterval(candles.Last().StartTimeStamp, (int)interval);

                                holder = [.. existing.GetViewBetween(
                                            new PriceCandle { StartTimeStamp = start },
                                            new PriceCandle { StartTimeStamp = end })];
                            }
                        }

                        if (modified && _onCandles is not null)
                        {
                            await _onCandles(new StrategyOnCandleSnapshot
                            {
                                ChartInterval = interval,
                                Exchange = item.Exchange,
                                Token = item.Token,
                                TradingSymbol = item.TradingSymbol,
                                Candles = holder
                            });
                        }
                    }
                }
            }));
            await Task.WhenAll(tasks);
        }

        private static ConcurrentDictionary<ChartInterval, SortedSet<PriceCandle>> GenerateIntervalCandles(IEnumerable<ChartInterval> chartIntervals, IEnumerable<TimePriceDataResponse> oneMinutePriceData)
        {
            ConcurrentDictionary<ChartInterval, SortedSet<PriceCandle>> resp = [];
            foreach (var timeInterval in chartIntervals)
                resp.TryAdd(timeInterval, Utility.AggregateCandles(oneMinutePriceData, (int)timeInterval));

            return resp;
        }
    }
}
