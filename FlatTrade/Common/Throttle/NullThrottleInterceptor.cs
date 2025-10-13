using Castle.DynamicProxy;

namespace FlatTrade.Common.Throttle
{
    /// <summary>
    /// A no-operation interceptor that simply proceeds with the original method call without any throttling.
    /// </summary>
    public class NullThrottleInterceptor : IInterceptor
    {
        public void Intercept(IInvocation invocation) => invocation.Proceed();
    }
}
