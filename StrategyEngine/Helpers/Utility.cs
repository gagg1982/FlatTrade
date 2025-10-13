using FlatTrade.MarketInfoManager;
using StrategyEngine.Model;
using System.Threading.Channels;

namespace StrategyEngine.Helpers
{
    internal class Utility
    {
        internal static Channel<T> CreateBoundedChannel<T>(int capacity)
        {
            var options = new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true, // Can have multiple readers if needed
                SingleWriter = true // Can have multiple writers if needed      
            };
            return Channel.CreateBounded<T>(options);
        }

        public static IEnumerable<PriceCandle> AggregateCandles(IEnumerable<TimePriceDataResponse> oneMinuteCandles, int targetIntervalMinutes)
        {
            // Validation for target interval
            if (targetIntervalMinutes <= 0 || targetIntervalMinutes > 1440)
            {
                throw new ArgumentOutOfRangeException(nameof(targetIntervalMinutes),
                    "Target interval must be a positive integer, typically <= 1440 minutes (day).");
            }

            // Validation for input data
            if (oneMinuteCandles == null || !oneMinuteCandles.Any())
                return [];

            // Ensure candles are sorted by DateTime (LocalDateTime), crucial for correct Open/Close determination
            // If your input is already sorted, you can skip this, but it's a good safeguard.
            var sortedCandles = oneMinuteCandles.OrderBy(c => c.StartDateTime).ToList();

            // Group candles into buckets based on the target interval.
            // The key for grouping is the start time of each aggregation interval.
            return sortedCandles
                .GroupBy(candle =>
                {
                    long totalMinutesSinceStartOfDay = (long)candle.StartDateTime.TimeOfDay.TotalMinutes;
                    long intervalStartMinutesSinceStartOfDay = (totalMinutesSinceStartOfDay / targetIntervalMinutes) * targetIntervalMinutes;

                    // Create a new DateTime for the start of the interval, preserving date part.
                    return new DateTime(
                        candle.StartDateTime.Year,
                        candle.StartDateTime.Month,
                        candle.StartDateTime.Day,
                        (int)(intervalStartMinutesSinceStartOfDay / 60),  // Hour
                        (int)(intervalStartMinutesSinceStartOfDay % 60),  // Minute
                        0, // Seconds
                        candle.StartDateTime.Kind // Preserve DateTimeKind
                    );
                })
                .OrderBy(group => group.Key) // Order the groups by their start time
                .Select(group => new PriceCandle // Project each group into a new ListOfPriceCandleData object
                {
                    TimeStamp = group.Key, // The start time of the interval
                    Open = group.First().OpenPrice, // Open price of the first candle in the group
                    High = group.Max(c => c.HighPrice), // Highest HighPrice in the group
                    Low = group.Min(c => c.LowPrice),   // Lowest LowPrice in the group
                    Close = group.Last().ClosePrice,  // Close price of the last candle in the group
                    Volume = group.Sum(c => (long)c.Volume)      // Sum of volumes in the group
                })
                .ToList(); // Materialize the results into a List<ListOfPriceCandleData>
        }
    }
}
