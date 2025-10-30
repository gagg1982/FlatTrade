using FlatTrade;
using FlatTrade.Common.Helpers;
using FlatTrade.Common.Types;
using FlatTrade.Common.Types.Base;
using FlatTrade.MarketInfoManager;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StrategyEngine.Helpers;
using StrategyEngine.Model;
using StrategyEngine.Strategies;
using System.Collections.Concurrent;


namespace StrategyEngine.BrokerData
{
    internal class Candle : IAsyncDisposable
    {
        private readonly IConfiguration _config;
        private readonly Api _api;
        private readonly ILogger _logger;
        private readonly ContextAccessor _contextAccessor;

        private readonly CancellationTokenSource _cts = new();
        private readonly Task _shiftIntervalTask;

        private event OnUpdate? _onCandles;


        private static IEnumerable<ChartInterval> _priceIntervals = [ChartInterval.One,
                                                                    ChartInterval.Three, ChartInterval.Five,
                                                                    ChartInterval.Ten, ChartInterval.Fifteen,
                                                                    ChartInterval.Thirty
                                                                    ];

        public Candle(IConfiguration config, ContextAccessor contextAccessor, Api api, OnUpdate? onUpdate, ILoggerFactory loggerFactory)
        {
            _api = api;
            _logger = loggerFactory.CreateLogger<Candle>();
            _config = config;
            _contextAccessor = contextAccessor;
            _onCandles += onUpdate;
            
            _shiftIntervalTask = Task.Run(() => RunAsync(_cts.Token));
            _logger.LogInformation("Candle:RunAsync task Scheduled.");
        }

        public async ValueTask DisposeAsync()
        {
            await StopAsync();
            _cts.Dispose();
        }

        private async Task RunAsync(CancellationToken token)
        {
            List<Task> task = [];
            try
            {
                foreach(var interval in _priceIntervals)
                    task.Add(RunMinuteJobAsync(token, interval));
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError("Candle:RunAsync Unexpected Error: {0} ", ex);
            }
            finally
            {
                await Task.WhenAll(task);
                _logger.LogInformation("Candle:RunAsync: Task stopped gracefully.");
            }
        }

        private async Task ShiftCandles(ChartInterval interval)
        {
            var currentDateTime = DateTime.Now.ToLocalTime();            
            var currentTimeAlignToInterval = Utility.AlignToInterval(currentDateTime, (int)interval);
            var currentCandle = new PriceCandle { StartTimeStamp = currentTimeAlignToInterval };

            List<Task> tasks = [];
            foreach (var (tradingSymbol, details) in GlobalDataSet.Data)
            {
                foreach (var (exch, priceInfo) in details.PriceCandleInfo)
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        long token = 0;
                        if (details.SecurityInfo.TryGetValue(exch, out ScripInfo? scrip) && scrip is not null)
                            token = scrip.Token;

                        var priceCandleSet = priceInfo[interval];
                        PriceCandle? previousCandleFromView = null;
                        lock (priceCandleSet)
                        {
                            previousCandleFromView = priceCandleSet.FirstOrDefault();
                            if (previousCandleFromView is not null)
                            {
                                if (!priceCandleSet.TryGetValue(currentCandle, out PriceCandle? currentCandleFromView) &&
                                    currentCandleFromView is null)
                                {
                                    var pseudoCurrentCandle = new PriceCandle
                                    {
                                        StartTimeStamp = currentTimeAlignToInterval,
                                        Open = previousCandleFromView.Close,
                                        Close = previousCandleFromView.Close,
                                        High = previousCandleFromView.Close,
                                        Low = previousCandleFromView.Close,
                                        Volume = 0,
                                        AccumulatedVolume = previousCandleFromView.AccumulatedVolume,
                                        PseudoFlag = true
                                    };
                                    priceCandleSet.Add(pseudoCurrentCandle);
                                }
                            }
                        }

                        if (previousCandleFromView is not null &&
                            previousCandleFromView.PseudoFlag &&
                            _onCandles is not null)
                        {
                            await _onCandles(new StrategyOnCandleSnapshot
                            {
                                ChartInterval = interval,
                                Exchange = exch,
                                Token = token,
                                TradingSymbol = tradingSymbol,
                                Candles = [previousCandleFromView]
                            });
                        }
                    }));
                }                    
            }
            await Task.WhenAll(tasks);
        }

        private async Task RunMinuteJobAsync(CancellationToken token, ChartInterval chartInterval)
        {
            while (!token.IsCancellationRequested)
            {
                var now = DateTime.Now.ToLocalTime();
                var nextMinuteInterval = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0).AddMinutes((int)chartInterval);
                var delay = nextMinuteInterval - now;
                await Task.Delay(delay, token);

                // Run your logic at the exact minute
                if (now.Hour >= 9 && now.Hour <= 18)
                {
                    await ShiftCandles(chartInterval);
                    //await WriteCandles();
                }
            }
        }

        public async Task StopAsync()
        {
            if (_cts.IsCancellationRequested)
                return;

            _cts.Cancel();
            try
            {
                await _shiftIntervalTask;
            }
            catch (TaskCanceledException)
            {
                // Expected during shutdown
            }
            _cts.Dispose();
        }

        //public async Task UpdateCandlesAsync(string tradingSymbol, Exchange exchange, long token, decimal price, long quantity, DateTime tradeDateTime)
        //{
        //    if (_priceIntervals.Contains(ChartInterval.Daily))
        //        _logger.LogWarning("Daily interval candles cannot be fetched using TimePriceData API. Continuing for rest of the intervals...");

        //    List<Task> tasks = [];
        //    var selection = new SelectedSymbol { Exchange = exchange, Token = token, TradingSymbol = tradingSymbol };
        //    var start = tradeDateTime.AddSeconds(-tradeDateTime.Second).AddMinutes(-2);
        //    var end = tradeDateTime.Date.AddDays(1);

        //    tasks.Add(UpdateHistoricCandlesHelperAsync(_api, [selection], start, end, _onCandles, _logger));
        //    await Task.WhenAll(tasks);
        //}

        public async Task UpdateCandlesWithQuotesAsync(string tradingSymbol, Exchange exchange, long token, decimal price, long dayVolume, DateTime tradeDateTime)
        {
            if (_priceIntervals.Contains(ChartInterval.Daily))
                _logger.LogWarning("Daily interval candles cannot be fetched using TimePriceData API. Continuing for rest of the intervals...");
            List<Task> tasks = [];
            var priceResponseObject = new TimePriceDataResponse {   ClosePrice = price, OpenPrice = price, HighPrice = price, LowPrice = price, Volume = dayVolume, StartDateTime = tradeDateTime };

            tasks.Add(UpdateCandlesWithQuotesHelperAsync(_api, priceResponseObject, exchange, tradingSymbol, token, _onCandles, _logger));
            await Task.WhenAll(tasks);
        }

        private static async Task UpdateCandlesWithQuotesHelperAsync(Api api,
                                                           TimePriceDataResponse resp,
                                                           Exchange exchange,
                                                           string tradingSymbol,
                                                           long token,
                                                           OnUpdate? _onCandles,
                                                           ILogger logger
                                                          )
        {
            var customIntervalCandleSet = GenerateIntervalCandles(_priceIntervals, [resp]);

            var details = GlobalDataSet.Data.GetOrAdd(tradingSymbol, _ => new Details());
            var priceCandleDict = details.PriceCandleInfo.GetOrAdd(exchange, _ => new());

            foreach (var (interval, customCandleSet) in customIntervalCandleSet)
            {
                if (customCandleSet is null)
                {
                    logger.LogWarning("Candle:UpdateCandlesWithQuotesHelperAsync: Candles are not computed for interval {0} min.", interval);
                    continue;
                }

                var set = priceCandleDict.GetOrAdd(interval, _ => new SortedSet<PriceCandle>());

                SortedSet<PriceCandle> holder = [];
                if(customCandleSet.Count > 1)
                {
                    logger.LogError("Candle:UpdateCandlesWithQuotesHelperAsync: Multiple candles generated for interval {0} in updates. Ideally only once candle should be generated.", interval);
                }
                //  Per interval only one candle will be generated in case of updates.
                var candleToApply = customCandleSet.First(); 
                lock (set)
                {
                    set.TryGetValue(new PriceCandle { StartTimeStamp = candleToApply.StartTimeStamp.AddMinutes(-(int)interval) }, out PriceCandle? previousCandle);
                                       
                    set.TryGetValue(candleToApply, out PriceCandle? existingCandle);
                    if(existingCandle is null)
                    {
                        if (previousCandle is not null) //overwriting for accumulated volume
                        {
                            candleToApply.AccumulatedVolume = previousCandle.AccumulatedVolume == 0 ? candleToApply.Volume: previousCandle.AccumulatedVolume;
                            candleToApply.Volume = candleToApply.Volume - previousCandle.AccumulatedVolume;
                        }
                        else
                        {
                            candleToApply.AccumulatedVolume = candleToApply.Volume;
                            candleToApply.Volume = 0;
                        }
                        set.Add(candleToApply);
                        holder.Add(candleToApply.Clone());
                    }
                    else
                    {                            
                        if (candleToApply.Open == decimal.MinValue && previousCandle is not null)
                        {
                            candleToApply.Open = previousCandle.Close;
                            candleToApply.High = previousCandle.Close;
                            candleToApply.Low = previousCandle.Close;
                            candleToApply.Close = previousCandle.Close;
                            candleToApply.Volume = candleToApply.Volume - existingCandle.AccumulatedVolume;
                            // not touching the Accumulated volume here (in case of update).
                        }

                        if(existingCandle.ApplyWithQuotes(candleToApply))
                            holder.Add(existingCandle.Clone());
                    }
                   
                }

                if (_onCandles is not null && holder is not null && holder.Any())
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

        public async Task GetHistoricCandlesFromServerAsync(IEnumerable<SelectedSymbol> selection, int lastXDaysCandle = 5)
        {
            if (_priceIntervals.Contains(ChartInterval.Daily))
                _logger.LogWarning("Daily interval candles cannot be fetched using TimePriceData API. Continuing for rest of the intervals...");

            List<Task> tasks = [];
            var startDate = DateTime.Now.Date.GetBusinessDaysAgo(lastXDaysCandle);
            var endDate = DateTime.Now.Date.AddDays(1).ToLocalTime();
            tasks.Add(UpdateHistoricCandlesHelperAsync(_api, selection, startDate, endDate, _onCandles, _logger));
            await Task.WhenAll(tasks);
        }

        private static async Task UpdateHistoricCandlesHelperAsync(Api api,
                                                           IEnumerable<SelectedSymbol> selection,
                                                           DateTime startDate, DateTime endDate,
                                                           OnUpdate? _onCandles,
                                                           ILogger logger
                                                          )
        {
            foreach (var item in selection)
            {
                IEnumerable<TimePriceDataResponse>? resp = null;
                string msg = string.Empty;
                int cnt = 0;
                while (true)
                {
                    ++cnt;
                    (resp, msg) = await api.MarketInfo.GetTimePriceDataAsync(item.Exchange, item.TradingSymbol, startDate, endDate, ChartInterval.One);
                    msg = $"{item.Exchange} {item.TradingSymbol} OHLCV data ({(int)ChartInterval.One} min) for start date {startDate} and end date {endDate}. {msg}";
                    
                    if (cnt > 3 || (resp is not null && resp.Any()))
                        break;
                    Thread.Sleep(100);
                }

                if (msg.Contains("no data"))
                    continue;

                if (resp is null || !resp.Any())
                {
                    logger.LogError("NOK: {msg}", msg);
                    continue;
                }
                
                var intervalCandles = GenerateIntervalCandles(_priceIntervals, resp);
                
                var details = GlobalDataSet.Data.GetOrAdd(item.TradingSymbol, _ => new Details());
                var priceCandleDict = details.PriceCandleInfo.GetOrAdd(item.Exchange, _ => new());

                foreach (var (interval, candles) in intervalCandles)
                {
                    var existing = priceCandleDict.GetOrAdd(interval, _ => new SortedSet<PriceCandle>());

                    SortedSet<PriceCandle> holder = [];
                    lock (existing)
                    {
                        foreach (var candle in candles)
                        {
                            var existingCandle = existing.LastOrDefault(c => c.StartTimeStamp == candle.StartTimeStamp);
                            if (existingCandle is null)
                            {
                                existing.Add(candle);
                                holder.Add(candle);
                            }
                            else
                            {
                                //ideally this leg will onyl come if user call this api multiple times.
                                if (existingCandle.OverWrite(candle))
                                    holder.Add(candle);
                            }
                        }
                    }

                    if (_onCandles is not null && holder.Any())
                    {
                        await _onCandles(new StrategyOnCandleSnapshot
                        {
                            ChartInterval = interval,
                            Exchange = item.Exchange,
                            Token = item.Token,
                            TradingSymbol = item.TradingSymbol,
                            Candles = [.. holder.Reverse()]
                        });
                    }                 
                }
            }
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
