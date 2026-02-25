using System.Diagnostics;

namespace Common.Helpers
{
    public static class StopwatchExtension
    {
        /// <summary>
        /// Stops the Stopwatch and logs the elapsed time along with a custom message.
        /// </summary>
        /// <param name="stopwatch">The Stopwatch instance.</param>
        /// <param name="message">A custom message to display with the elapsed time.</param>
        public static string StopAndLog(this Stopwatch stopwatch)
        {
            // Call the original Stop() method
            stopwatch.Stop();
            TimeSpan ts = stopwatch.Elapsed;
            string elapsedTime = string.Format("{0:00}:{1:00}:{2:00}.{3:000}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds);

            return $"{elapsedTime}";
        }
    }

}
