namespace Upstox
{
    using Castle.DynamicProxy;
    using Common.Transport;
    using Common.Throttle;
    using Upstox.MarketInfoManager;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// 
    /// </summary>
    public sealed class Api : IDisposable, IAsyncDisposable
    {
        private bool _disposed = false;
        private readonly static RestHttpClient _client = new(new HttpClient(), string.Empty);

        private readonly MarketInfo _marketInfo;

        private ILogger<Api> _logger;
        public static RestHttpClient HttpClient => _client;

        public MarketInfo MarketInfo => _marketInfo;

        public Api(ILoggerFactory? loggerFactory, IInterceptor? throttlerInterceptor)
        {
            loggerFactory ??= new LoggerFactory();
            _logger = loggerFactory.CreateLogger<Api>();

            var proxyGen = new ProxyGenerator();
            var interceptor = throttlerInterceptor ?? new NullThrottleInterceptor();
            _marketInfo = proxyGen.CreateClassProxy<MarketInfo>([_client, loggerFactory], interceptor);            
        }


        public void Dispose()
        {
            _client.GetNativeHttpClient().Dispose();
            DisposeAsyncCore().AsTask().GetAwaiter().GetResult(); // Safe sync fallback
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore();
            GC.SuppressFinalize(this);
        }

        private async ValueTask DisposeAsyncCore()
        {
            if (_disposed)
                return;

            _disposed = true;           

            _logger.LogInformation("{0}: Disposed gracefully", GetType().Name);
            // Dispose other sync-only resources here (e.g., timers, files)
            await Task.CompletedTask;
        }

    }
}
