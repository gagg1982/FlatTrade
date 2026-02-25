using Castle.DynamicProxy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Reflection;

namespace Common.Throttle
{
    internal class PerSecAndMinDateTime
    {
        public bool Throttled { get; set; } = false;
        public int MaxPerSec { get; set; } 
        public int MaxPerMin { get; set; }
        public ConcurrentQueue<DateTime> CurrentPerSec { get; set; } = [];
        public ConcurrentQueue<DateTime> CurrentPerMin { get; set; } = [];
    }

    public class RateLimiterThrottleInterceptor : IInterceptor
    {
        private readonly ILogger<RateLimiterThrottleInterceptor> _logger;
        private readonly RateLimiterThrottleSetting _setting;
        private readonly ConcurrentDictionary<MethodInfo, PerSecAndMinDateTime> _callHistory = new();

        public RateLimiterThrottleInterceptor(IConfiguration config, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<RateLimiterThrottleInterceptor>();

            _setting = config.GetSection("Api:Throttling:RateLimiter")
                 .Get<RateLimiterThrottleSetting>() ?? new RateLimiterThrottleSetting();
            var dict = config.GetSection("Api:Throttling:RateLimiter:PerApiCalls")
                                                    .Get<ConcurrentDictionary<string, PerApiThrottleSetting>>();
            if (dict is not null && !dict.IsEmpty)
                _setting.PerApiCalls = dict;

        }

        public void Intercept(IInvocation invocation)
        {
            var method = invocation.MethodInvocationTarget ?? invocation.Method;

            var perMethodSetting = new PerApiThrottleSetting
            {
                CallsPerSec = _setting.GlobalCallsPerSec,
                CallsPerMin = _setting.GlobalCallsPerMin
            };

            if (_setting.PerApiCalls.TryGetValue(method.Name, out PerApiThrottleSetting? rate) && rate is not null)
            {
                perMethodSetting.CallsPerSec = rate.CallsPerSec;
                perMethodSetting.CallsPerMin = rate.CallsPerMin;
            }

            var history = _callHistory.GetOrAdd(method, _ => new PerSecAndMinDateTime{ MaxPerMin = perMethodSetting.CallsPerMin, MaxPerSec = perMethodSetting.CallsPerSec });

            lock (history)
            {
                while (true)
                {
                    var now = DateTime.Now;
                    // Remove old timestamps outside the window
                    while (!history.CurrentPerSec.IsEmpty &&
                           history.CurrentPerSec.TryPeek(out DateTime oldest) && (now - oldest).TotalSeconds > 1)
                    {
                        history.CurrentPerSec.TryDequeue(out DateTime _);
                    }

                    while (!history.CurrentPerMin.IsEmpty &&
                           history.CurrentPerMin.TryPeek(out DateTime oldest) && (now - oldest).TotalMinutes > 1)
                    {
                        history.CurrentPerMin.TryDequeue(out DateTime _);
                    }


                    if (history.CurrentPerMin.Count >= history.MaxPerMin)
                    {
                        if (history.Throttled == false)
                        {
                            _logger.LogWarning("[THROTTLE STARTED] {methodName} exceeded {history.MaxPerMin} calls/min. Current count {history.CurrentPerMin.Count} calls. Sleeping for {_setting.SleepForXMilliSecondsIfBreached}ms."
                                                    , method.Name, history.MaxPerMin, history.CurrentPerMin.Count, 10 * _setting.SleepForXMilliSecondsIfBreached);
                            history.Throttled = true;
                        }
                        Thread.Sleep(10 * _setting.SleepForXMilliSecondsIfBreached);                        
                        continue;
                    }

                    if (history.CurrentPerSec.Count >= history.MaxPerSec)
                    {
                        if (history.Throttled == false)
                        {
                            _logger.LogWarning("[THROTTLE STARTED] {methodName} exceeded {history.MaxPerSec} calls/sec. Current count {history.CurrentPerSec.Count} calls. Sleeping for {_setting.SleepForXMilliSecondsIfBreached}ms."
                                                    , method.Name, history.MaxPerSec, history.CurrentPerSec.Count, 10 * _setting.SleepForXMilliSecondsIfBreached);
                            history.Throttled = true;
                        }
                        Thread.Sleep(10 * _setting.SleepForXMilliSecondsIfBreached);
                        continue;
                    }

                    if (history.Throttled == true && 
                        (history.CurrentPerMin.Count <= history.MaxPerMin * 0.9 ||
                         history.CurrentPerSec.Count <= history.MaxPerSec * 0.9))
                    {
                        _logger.LogWarning("[THROTTLE ENDED]");
                        history.Throttled = false;
                    }

                    history.CurrentPerMin.Enqueue(now);
                    history.CurrentPerSec.Enqueue(now);

                    if (history.CurrentPerSec.Count >= 0.8 * history.MaxPerSec && history.CurrentPerSec.Count < 0.9 * history.MaxPerSec)
                        Thread.Sleep(3 * _setting.SleepForXMilliSecondsIfBreached);
                    else if (history.CurrentPerSec.Count >= 0.9 * history.MaxPerSec)
                        Thread.Sleep(5 * _setting.SleepForXMilliSecondsIfBreached);

                    if (history.CurrentPerMin.Count >= 0.8 * history.MaxPerMin && history.CurrentPerMin.Count < 0.9 * history.MaxPerMin)
                        Thread.Sleep(3 * _setting.SleepForXMilliSecondsIfBreached);
                    else if (history.CurrentPerMin.Count >= 0.9 * history.MaxPerMin)
                        Thread.Sleep(5 * _setting.SleepForXMilliSecondsIfBreached);

                    invocation.Proceed();
                    break;
                    
                }
            }
        }
    }
}