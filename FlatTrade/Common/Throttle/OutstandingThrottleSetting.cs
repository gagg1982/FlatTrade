using System.Collections.Concurrent;

namespace FlatTrade.Common.Throttle
{
    public class PerApiThrottleSetting
    {        
        public int CallsPerSec { get; set; } = 12;
        public int CallsPerMin { get; set; } = 120;
    }

    public class OutstandingThrottleSetting
    {

        public int GlobalCallsPerSec { get; set; } = 12;
        public int GlobalCallsPerMin { get; set; } = 120;
        public ConcurrentDictionary<string, PerApiThrottleSetting> PerApiCalls = [];
    }
}
