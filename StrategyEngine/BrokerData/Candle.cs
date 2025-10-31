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
        private bool _disposed = false;
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
                        PriceCandle? holder = null;
                        lock (priceCandleSet)
                        {
                            var previousCandleFromView = priceCandleSet.FirstOrDefault();
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
                                        AccumulatedVolume = previousCandleFromView.AccumulatedVolume + previousCandleFromView.Volume,
                                        Volume = 0,
                                        PseudoFlag = true
                                    };
                                    priceCandleSet.Add(pseudoCurrentCandle);
                                    _logger.LogWarning("===========Shift Added: I:{0}, T:{1}, O:{2}, AC:{3}",
                                    interval,
                                    pseudoCurrentCandle.StartTimeStamp,
                                    pseudoCurrentCandle.Open,
                                    pseudoCurrentCandle.AccumulatedVolume);
                                    holder = pseudoCurrentCandle.Clone();
                                }
                            }
                        }

                        if (holder is not null &&
                            _onCandles is not null)
                        {
                            await _onCandles(new StrategyOnCandleSnapshot
                            {
                                ChartInterval = interval,
                                Exchange = exch,
                                Token = token,
                                TradingSymbol = tradingSymbol,
                                Candles = [holder]
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
                if (now.Hour >= 9 && now.Hour <= 22)
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
                _onCandles = null;
            }
            catch (TaskCanceledException)
            {
                // Expected during shutdown
            }
            _cts.Dispose();
        }       

        public async Task UpdateCandlesWithQuotesAsync(string tradingSymbol, Exchange exchange, 
                                                        long token, decimal latestPrice, 
                                                        decimal previousPrice, long currentDayVolume, DateTime tradeDateTime
                                                      )
        {
            if (_priceIntervals.Contains(ChartInterval.Daily))
                _logger.LogWarning("Daily interval candles cannot be fetched using TimePriceData API. Continuing for rest of the intervals...");

            var details = GlobalDataSet.Data.GetOrAdd(tradingSymbol, _ => new Details());
            var priceCandleDict = details.PriceCandleInfo.GetOrAdd(exchange, _ => new());
            
            foreach (var (interval, sortedSet) in priceCandleDict)
            {
                if (sortedSet is null)
                {
                    _logger.LogWarning("Candle:UpdateCandlesWithQuotesHelperAsync: Candles are not computed for {0} min interval. Skipping update... ", interval);
                    continue;
                }

                SortedSet<PriceCandle> holder = [];                               
                lock (sortedSet)
                {
                    var currentTimeInterval = Utility.AlignToInterval(tradeDateTime, (int)interval);
                    var previousTimeInterval = currentTimeInterval.AddMinutes(-(int)interval);
                    
                    sortedSet.TryGetValue(new PriceCandle { StartTimeStamp = previousTimeInterval }, out PriceCandle? previousCandle);
                    sortedSet.TryGetValue(new PriceCandle { StartTimeStamp = currentTimeInterval }, out PriceCandle? existingCandle);
                    
                    if(existingCandle is null)
                    {
                        var accumVol = currentDayVolume;
                        if (previousCandle is not null && previousCandle.AccumulatedVolume !=0)
                        {
                            accumVol = previousCandle.AccumulatedVolume + previousCandle.Volume;
                        }

                        var newPriceCandleToAdd = new PriceCandle
                        {
                            StartTimeStamp = currentTimeInterval,
                            Open = latestPrice == decimal.MinValue ? previousPrice : latestPrice,
                            High = latestPrice == decimal.MinValue ? previousPrice : latestPrice,
                            Low = latestPrice == decimal.MinValue ? previousPrice : latestPrice,
                            Close = latestPrice == decimal.MinValue ? previousPrice : latestPrice,
                            PseudoFlag = (latestPrice == decimal.MinValue),
                            AccumulatedVolume = accumVol,
                            Volume = currentDayVolume - accumVol,
                        };

                        sortedSet.Add(newPriceCandleToAdd);
                        _logger.LogWarning("====== Candle Added: T:{0}, V{1}, AccumVol:{2}, P:{3}, Pseudo:{4}, ActualTime:{5}, LatestP:{6}, PreviousP:{7}", newPriceCandleToAdd.StartTimeStamp, newPriceCandleToAdd.Volume, newPriceCandleToAdd.AccumulatedVolume, newPriceCandleToAdd.Open, newPriceCandleToAdd.PseudoFlag, tradeDateTime, latestPrice, previousPrice);
                        
                        if(!newPriceCandleToAdd.PseudoFlag)
                            holder.Add(newPriceCandleToAdd.Clone());
                    }
                    else
                    {
                        _logger.LogWarning("===== Candle Updated: T:{0}, P:{1}, V:{2}, Pseudo:{3}", currentTimeInterval, latestPrice, currentDayVolume, latestPrice == decimal.MinValue);
                        if (existingCandle.ApplyQuotes(currentTimeInterval, latestPrice, currentDayVolume, latestPrice == decimal.MinValue))
                            holder.Add(existingCandle.Clone());
                    }                   
                }

                if (_onCandles is not null && holder.Any())
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
                    await Task.Delay(100);
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

        public void Dispose()
        {
            DisposeAsyncCore().AsTask().GetAwaiter().GetResult(); // Safe sync fallback
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore();
            GC.SuppressFinalize(this);
        }

        private async ValueTask DisposeAsyncCore()
        {
            if (_disposed)
                return;

            _disposed = true;

            await StopAsync();
            _logger.LogInformation("{0}: Disposed gracefully", GetType().Name);

        }
    }
}
