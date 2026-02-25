using Castle.DynamicProxy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Reflection;

namespace Common.Throttle
{
    internal class PerSecAndMinSlimSemaphores
    {
        public bool Throttled { get; set; } = false;
        public required SemaphoreSlim CurrentPerSec { get; set; } 
        public required SemaphoreSlim CurrentPerMin { get; set; } 
    }

    public class OutstandingThrottleInterceptor: IInterceptor
    {
        private readonly ILogger<OutstandingThrottleInterceptor> _logger;
        private readonly OutstandingThrottleSetting _setting;
        private static readonly ConcurrentDictionary<MethodInfo, PerSecAndMinSlimSemaphores> _limiters = new();

        public OutstandingThrottleInterceptor(IConfiguration config, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<OutstandingThrottleInterceptor>();

            _setting = config.GetSection("Api:Throttling:ConcurrencyOnOutstanding")
                             .Get<OutstandingThrottleSetting>() ?? new OutstandingThrottleSetting();
            var dict = config.GetSection("Api:Throttling:ConcurrencyOnOutstanding:PerApiCalls")
                                                    .Get<ConcurrentDictionary<string, PerApiThrottleSetting>>();
            if (dict is not null && !dict.IsEmpty)
                _setting.PerApiCalls = dict;
        }
        public void Intercept(IInvocation invocation)
        {
            var method = invocation.MethodInvocationTarget ?? invocation.Method;
            //var attr = method.GetCustomAttribute<ThrottleAttribute>();
            //if (attr == null)
            //{
            //    invocation.Proceed();
            //    return;
            //}

            // semaphore acts as "outstanding counter"
            var perMethodSetting = new PerApiThrottleSetting
            {     CallsPerSec = _setting.GlobalCallsPerSec,
                  CallsPerMin = _setting.GlobalCallsPerMin };

            if (_setting.PerApiCalls.TryGetValue(method.Name, out PerApiThrottleSetting? rate) && rate is not null)
            {
                perMethodSetting.CallsPerSec = rate.CallsPerSec;
                perMethodSetting.CallsPerMin = rate.CallsPerMin;
            }

            var limiter = _limiters.GetOrAdd(method, _ => new PerSecAndMinSlimSemaphores 
                                                                {
                                                                    CurrentPerSec = new SemaphoreSlim(perMethodSetting.CallsPerSec, perMethodSetting.CallsPerSec),
                                                                    CurrentPerMin = new SemaphoreSlim(perMethodSetting.CallsPerMin, perMethodSetting.CallsPerMin)
                                                                });
            if(limiter.CurrentPerMin.CurrentCount <= perMethodSetting.CallsPerMin * 0.01 && limiter.Throttled == false)
            {
                _logger.LogWarning("[THROTTLE STARTED] {methodName} exceeded {perMethodSetting.CallsPerSec} calls/min. Current count {limiter.PerMin.CurrentCount} calls"
                                                , method.Name, perMethodSetting.CallsPerMin, limiter.CurrentPerMin.CurrentCount);
                limiter.Throttled = true;
            }

            if (limiter.CurrentPerSec.CurrentCount <= perMethodSetting.CallsPerSec * 0.01 && limiter.Throttled == false)
            {
                    _logger.LogWarning("[THROTTLE STARTED] {methodName} exceeded {perMethodSetting.CallsPerSec} calls/min. Current count {limiter.PerSec.CurrentCount} calls"
                               , method.Name, perMethodSetting.CallsPerSec, limiter.CurrentPerSec.CurrentCount);
                    limiter.Throttled = true;
            }

            limiter.CurrentPerMin.Wait(); // blocks if too many outstanding
            limiter.CurrentPerSec.Wait(); // blocks if too many outstanding
            
            try
            {
                invocation.Proceed(); // execute the method
            }
            finally
            {
                if (limiter.Throttled == true &&
                        (limiter.CurrentPerMin.CurrentCount >= perMethodSetting.CallsPerMin * 0.3 ||
                         limiter.CurrentPerSec.CurrentCount >= perMethodSetting.CallsPerSec * 0.3 ))
                {
                    _logger.LogWarning("[THROTTLE ENDED]");
                    limiter.Throttled = false;
                }

                limiter.CurrentPerMin.Release(); // request finished
                limiter.CurrentPerSec.Release(); // request finished                
            }
        }
    }
}
