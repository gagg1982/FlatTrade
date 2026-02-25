using System.Collections.Concurrent;

namespace Common.Throttle
{
    public class RateLimiterThrottleSetting
    {
        public int GlobalCallsPerSec { get; set; } = 12;
        public int GlobalCallsPerMin { get; set; } = 120;
        public int SleepForXMilliSecondsIfBreached { get; set; } = 50;

        public ConcurrentDictionary<string, PerApiThrottleSetting> PerApiCalls = [];

    }
}
