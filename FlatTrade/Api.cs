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
    public class Api
    {
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

            var (userResult, eMsg) = _user.GetUserDetailsAsync().GetAwaiter().GetResult(); // Ensure user details are fetched on initialization
            if (userResult is null)
                throw new InvalidOperationException($"Cannot fetch user details. {eMsg}");

            var (accessTokenResult, eMsg1) = _authentication.GetAccessTokenAsync().GetAwaiter().GetResult();
            if (accessTokenResult is null)
                throw new InvalidOperationException($"Failed to retrieve access token: {eMsg1}");

            _subscription = new Subscription(userResult.AccountId, userResult.AccountId, accessTokenResult.AccessToken, loggerFactory);
        }
    }
}
