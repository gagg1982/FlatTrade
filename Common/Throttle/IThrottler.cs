using Castle.DynamicProxy;

namespace Common.Throttle
{
    public interface IThrottler
    {
        public Task<IInterceptor> GetThrottlerInterceptor<T>();
    }
}
