using Castle.DynamicProxy;

namespace FlatTrade.Common.Throttle
{
    public interface IThrottler
    {
        public Task<IInterceptor> GetThrottlerInterceptor<T>();
    }
}
