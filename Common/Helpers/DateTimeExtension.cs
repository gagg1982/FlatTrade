namespace Common.Helpers
{
    public static class DateTimeExtension
    {
        public static DateTime GetBusinessDaysAgo(this DateTime fromDate, int daysBack)
        {
            if (daysBack < 0)
                throw new ArgumentOutOfRangeException(nameof(daysBack), "Days back cannot be negative.");

            var date = fromDate;
            int count = 0;

            while (count < daysBack)
            {
                date = date.AddDays(-1);

                // Skip weekends
                if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                {
                    count++;
                }
            }

            return date.Date;
        }
    }
}
