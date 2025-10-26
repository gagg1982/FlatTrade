using FlatTrade.Common.Types;
using FlatTrade.Common.Types.Base;
using FlatTrade.HoldingsManager;
using FlatTrade.SubscriptionManager.Quote;
using FlatTrade.SubscriptionManager.TouchLine;
using FlatTrade.TradeManager;
using Newtonsoft.Json.Linq;
using StrategyEngine.Helpers;
using System.Collections.Concurrent;

namespace StrategyEngine.Model
{
    public class SubscriptionDetails
    {
        public ConcurrentDictionary<Exchange, TouchLineSubscriptionRequestAck> TouchLineSubscription { get; set; } = [];
        public ConcurrentDictionary<Exchange, QuoteSubscriptionRequestAck> QuoteSubscription { get; set; } = [];
    }
    public class Details
    {       
        // key noren order number        
        public ConcurrentDictionary<long, OrderInfo> OpenOrders { get; } = [];
        public ConcurrentDictionary<long, OrderInfo> ClosedOrders { get; } = [];

        public ConcurrentDictionary<ProductType, PositionBookResponse> OpenPositions { get; } = [];
        public ConcurrentDictionary<ProductType, PositionBookResponse> ClosedPositions { get; } = [];
        public ConcurrentDictionary<Exchange, ConcurrentBag<TradeBookResponse>> TradeInfo { get; } = [];
        public ConcurrentDictionary<Exchange, HoldingsResponse> HoldingInfo { get; } = [];
        public ConcurrentDictionary<Exchange, ScripInfo> SecurityInfo { get; } = [];
        public ConcurrentDictionary<Exchange, ConcurrentDictionary<ChartInterval, SortedSet<PriceCandle>>> PriceCandleInfo { get; } = [];        
    }

    public static class GlobalDataSet
    {
        //key trading symbol
        public static ConcurrentDictionary<string, Details> Data { get; set; }  = [];
        public static ConcurrentDictionary<long, SubscriptionDetails> Subscriptions { get; set; } = [];

        private static readonly CancellationTokenSource _cts = new();
        private static readonly Task _backgroundTask;

        static GlobalDataSet()
        {
            // Start the background task when the class is first used
            _backgroundTask = Task.Run(() => RunAsync(_cts.Token));

            // Automatically stop when app is shutting down
            AppDomain.CurrentDomain.ProcessExit += async (_, _) => await StopAsync();
            Console.CancelKeyPress += async (_, e) =>
            {
                e.Cancel = true; // prevent abrupt termination
                await StopAsync();
            };
        }

        public static async Task StopAsync()
        {
            if (_cts.IsCancellationRequested)
                return;

            _cts.Cancel();
            try
            {
                await _backgroundTask;
            }
            catch (TaskCanceledException)
            {
                // Expected during shutdown
            }
            _cts.Dispose();
        }

        private static async Task RunAsync(CancellationToken token)
        {
            List<Task> task = [];
            try
            {
                task.Add(RunMinuteJobAsync(token));
            }
            catch (TaskCanceledException)
            {
                // Normal shutdown
            }
            //catch (Exception ex)
            //{
            //    Console.WriteLine($"GlobalDataSet: Background task error: {ex}");
            //}
            finally
            {
                await Task.WhenAll(task);
               // Console.WriteLine("GlobalDataSet: Background task stopped gracefully.");
            }
        }

        private static async Task ShiftCandles()
        {
            var currentDateTime = DateTime.Now.ToLocalTime();

            await Task.Run(async () =>
            {
                foreach (var (tradingSymbol, details) in Data)
                {
                    await Task.Run(async () =>
                    {
                        foreach (var (exch, priceInfo) in details.PriceCandleInfo)
                        {
                            await Task.Run(async () =>
                            {
                                foreach (var (interval, priceCandleSet) in priceInfo)
                                {
                                    await Task.Run(() =>
                                    {
                                        lock (priceCandleSet)
                                        {
                                            var previousTimeAlignToInterval = Utility.AlignToInterval(currentDateTime.AddMinutes(-1), (int)interval);
                                            var previousCandle = new PriceCandle { StartTimeStamp = previousTimeAlignToInterval };

                                            var currentTimeAlignToInterval = Utility.AlignToInterval(currentDateTime, (int)interval);
                                            var currentCandle = new PriceCandle { StartTimeStamp = currentTimeAlignToInterval };

                                            var currentCandleView = priceCandleSet.GetViewBetween(currentCandle, currentCandle).FirstOrDefault();
                                            var previousCandleView = priceCandleSet.GetViewBetween(previousCandle, previousCandle).FirstOrDefault();
                                            if (currentCandleView is null && previousCandleView is not null)
                                            {
                                                var pseudoCurrentCandle = new PriceCandle
                                                {
                                                    StartTimeStamp = currentTimeAlignToInterval,
                                                    Open = previousCandleView.Open,
                                                    Close = previousCandleView.Close,
                                                    High = previousCandleView.High,
                                                    Low = previousCandleView.Low,
                                                    Volume = previousCandleView.Volume,
                                                    PseudoFlag = true
                                                };
                                                priceCandleSet.Add(pseudoCurrentCandle);
                                            }
                                        }
                                    });
                                }
                            });
                        }
                    });
                }
            });
        }

        //private static async Task WriteCandles(DateTime fromTime, DateTime toTime)
        //{
        //    var baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CandleDumps");            
        //    Directory.CreateDirectory(baseDir);

        //    foreach (var intervalEntry in CandleStore.Candles)
        //    {
        //        int interval = intervalEntry.Key;
        //        var buckets = intervalEntry.Value;

        //        var filePath = Path.Combine(baseDir, $"candles_{interval}m.csv");
        //        using var stream = new StreamWriter(filePath, append: true);

        //        // find all hours < toTime and >= fromTime
        //        var hoursToDump = buckets.Keys
        //            .Where(h => h >= fromTime && h < toTime)
        //            .OrderBy(h => h)
        //            .ToList();

        //        foreach (var hour in hoursToDump)
        //        {
        //            if (hourlyBuckets.TryRemove(hour, out var candles))
        //            {
        //                foreach (var c in candles)
        //                {
        //                    string line = $"{c.Start:yyyy-MM-dd HH:mm:ss},{c.Open},{c.High},{c.Low},{c.Close},{c.Volume}";
        //                    await stream.WriteLineAsync(line);
        //                }
        //            }
        //        }

        //        await stream.FlushAsync();
        //        Console.WriteLine($"✅ Dumped {hoursToDump.Count} hour-buckets for {interval}m candles between {fromTime:t}–{toTime:t}");
        //    }
        //}

        private static async Task RunMinuteJobAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var now = DateTime.Now.ToLocalTime();
                var nextMinute = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0).AddMinutes(1);
                var delay = nextMinute - now;
                await Task.Delay(delay, token);

                // Run your logic at the exact minute
                if (nextMinute.Hour >= 9 && nextMinute.Hour <= 16)
                {
                    await ShiftCandles();
                    //await WriteCandles();
                }
            }            
        }

    }


}
