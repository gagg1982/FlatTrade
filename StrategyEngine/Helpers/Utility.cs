using FlatTrade.Common.Types;
using FlatTrade.MarketInfoManager;

namespace StrategyEngine.Helpers
{
    internal class Utility
    {        
        public static DateTime AlignToInterval(DateTime input, int intervalMinutes)
        {
            // Remove seconds and milliseconds first
            input = input.AddSeconds(-input.Second).AddMilliseconds(-input.Millisecond);

            // Calculate total minutes since midnight
            int totalMinutes = input.Hour * 60 + input.Minute;

            // Find the aligned boundary
            int alignedMinutes = (totalMinutes / intervalMinutes) * intervalMinutes;

            // Construct aligned DateTime
            DateTime aligned = new DateTime(input.Year, input.Month, input.Day, 0, 0, 0, input.Kind)
                                   .AddMinutes(alignedMinutes);

            return aligned;
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
                .GroupBy(candle => AlignToInterval(candle.StartDateTime, targetIntervalMinutes))
                .OrderBy(group => group.Key) // Order the groups by their start time
                .Select(group => new PriceCandle // Project each group into a new ListOfPriceCandleData object
                {
                    StartTimeStamp = group.Key, // The start time of the interval
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
