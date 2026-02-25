using Microsoft.VisualBasic;
using System.Security.AccessControl;
using Upstox.Types.Base;

namespace Upstox
{
    public static class EndPoints
    {
        private static string TimePriceDataUrl { get; } = "https://api.upstox.com/v3/historical-candle/EXCHANGE_ISIN_PAIR/INTERVAL/1/ENDDATETIME/STARTDATETIME";

        private static void ValidateStartAndEndDateTime(DateTime startDateTime, DateTime endDateTime )
        {
            if(startDateTime > endDateTime)
                throw new InvalidDataException("Start date time must be earlier than end date time.");
        }

        public static string GetOneMinuteTimePriceDataUrl(DateTime startDateTime, DateTime endDateTime, string isin, Exchange exchange)
        => GetTimePriceDataUrl("minutes", startDateTime, endDateTime, isin, exchange);

        public static string GetEodChartDataUrl(DateTime startDateTime, DateTime endDateTime, string isin, Exchange exchange)
           => GetTimePriceDataUrl("days", startDateTime, endDateTime, isin, exchange);

        private static string GetTimePriceDataUrl(string interval, DateTime startDateTime, DateTime endDateTime, string isin, Exchange exchange)
        {
            ValidateStartAndEndDateTime(startDateTime, endDateTime);

            return TimePriceDataUrl.Replace("INTERVAL", interval)
                                   .Replace("STARTDATETIME", startDateTime.ToString("yyyy-MM-dd"))
                                   .Replace("ENDDATETIME", endDateTime.ToString("yyyy-MM-dd"))
                                   .Replace("EXCHANGE_ISIN_PAIR", string.Format($"{exchange}_EQ|{isin}"));
        }
    }
}
