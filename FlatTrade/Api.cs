namespace FlatTrade
{
    using Castle.DynamicProxy;
    using Common.Transport;
    using FlatTrade.AlertManager;
    using FlatTrade.AuthenticationManager;
    using FlatTrade.Common.Throttle;
    using FlatTrade.FundManager;
    using FlatTrade.HoldingsManager;
    using FlatTrade.LimitsManager;
    using FlatTrade.MarketInfoManager;
    using FlatTrade.OrderManager;
    using FlatTrade.ScripManager;
    using FlatTrade.SubscriptionManager;
    using FlatTrade.TradeManager;
    using FlatTrade.UserManager;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// 
    /// </summary>
    public sealed class Api : IDisposable, IAsyncDisposable
    {
        private bool _disposed = false;
        private readonly static RestHttpClient _client = new(new HttpClient(), string.Empty);

        private readonly Order _order;
        private readonly Trade _trade;
        private readonly User _user;
        private readonly Scrips _scrips;
        private readonly Alert _alerts;
        private readonly Authentication _authentication;
        private readonly Subscription _subscription;
        private readonly Holdings _holdings;
        private readonly Limits _limits;
        private readonly Funds _funds;
        private readonly MarketInfo _marketInfo;

        private ILogger<Api> _logger;
        public static RestHttpClient HttpClient => _client;
        public Order Order => _order;
        public Trade Trade => _trade;
        public User User => _user;
        public Scrips Scrips => _scrips;
        public Alert Alerts => _alerts;
        public Authentication Authentication => _authentication;
        public Subscription Subscription => _subscription;
        public Holdings Holdings => _holdings;
        public Limits Limits => _limits;
        public Funds Funds => _funds;
        public MarketInfo MarketInfo => _marketInfo;

        public Api(string apiKey, string redirectUrl, string secret, string accessTokenFilePath, ILoggerFactory? loggerFactory, IInterceptor? throttlerInterceptor)
        {
            loggerFactory ??= new LoggerFactory();
            _logger = loggerFactory.CreateLogger<Api>();

            var proxyGen = new ProxyGenerator();
            var interceptor = throttlerInterceptor ?? new NullThrottleInterceptor();


            _authentication = proxyGen.CreateClassProxy<Authentication>([apiKey, redirectUrl, secret, accessTokenFilePath, _client, loggerFactory], interceptor);

            _user = proxyGen.CreateClassProxy<User>([_authentication, _client, loggerFactory], interceptor);
            _order = proxyGen.CreateClassProxy<Order>([this, _client, loggerFactory], interceptor);
            _trade = proxyGen.CreateClassProxy<Trade>([_authentication, _client, loggerFactory], interceptor);
            _scrips = proxyGen.CreateClassProxy<Scrips>([_authentication, _client, loggerFactory], interceptor);
            _holdings = proxyGen.CreateClassProxy<Holdings>([_authentication, _client, loggerFactory], interceptor);
            _limits = proxyGen.CreateClassProxy<Limits>([_authentication, _client, loggerFactory], interceptor);
            _funds = proxyGen.CreateClassProxy<Funds>([_authentication, _client, loggerFactory], interceptor);
            _marketInfo = proxyGen.CreateClassProxy<MarketInfo>([_authentication, _client, loggerFactory], interceptor);
            _alerts = proxyGen.CreateClassProxy<Alert>([_authentication, _client, loggerFactory], interceptor);
            
            _subscription = Task.Run(async () =>
            {
                var (userResult, eMsg) = await _user.GetUserDetailsAsync().ConfigureAwait(false);
                if (userResult is null)
                    throw new InvalidOperationException($"Cannot fetch user details. {eMsg}");

                var (accessTokenResult, eMsg1) = await _authentication.GetAccessTokenAsync().ConfigureAwait(false);
                if (accessTokenResult is null)
                    throw new InvalidOperationException($"Failed to retrieve access token: {eMsg1}");

                var subscription = await Subscription.CreateAsync(userResult.AccountId, userResult.AccountId, accessTokenResult.AccessToken, loggerFactory)
                                                    .ConfigureAwait(false);
                return subscription;
            }).GetAwaiter().GetResult();
        }


        public void Dispose()
        {
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

            // Dispose async resources             
            await Subscription.DisposeAsync();
            _client.GetNativeHttpClient().Dispose();

            _logger.LogInformation("{0}: Disposed gracefully", GetType().Name);
            // Dispose other sync-only resources here (e.g., timers, files)
        }

    }
}
