namespace FlatTrade.SubscriptionManager
{
    using Common.Transport;
    using FlatTrade.SubscriptionManager.Alert;
    using FlatTrade.SubscriptionManager.Order;
    using FlatTrade.SubscriptionManager.Quote;
    using FlatTrade.SubscriptionManager.TouchLine;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    public class SubscriptionEventArgs(SubscriptionType subscriptionType, string rawMessage) : EventArgs
    {
        public string RawMessage { get; }= rawMessage.Replace("\n", "").Replace(" ","")!;
        public SubscriptionType SubscriptionType { get; } = subscriptionType;
    }

    public delegate Task OnSubscriptionEvents(object? sender, SubscriptionType subscriptionType, string rawMessage, object? subscriptionObject);

    /// <summary>
    /// 
    /// </summary>
    public sealed class Subscription : IAsyncDisposable, IDisposable
    {
        private readonly ILogger<Subscription> _logger;
        private readonly CustomWebSocket _customClientWebSocket;

        public readonly TouchlineSubscription TouchLineSubscription;
        public readonly OrderSubscription OrderSubscription;
        public readonly QuoteSubscription QuoteSubscription;
        public readonly AlertSubscription AlertSubscription;

        private readonly ConcurrentDictionary<SubscriptionType, AsyncEventHandler<SubscriptionEventArgs>> _updateHandlers = [];

        private bool _disposed;

        private Subscription(string accountId, string userId, string accessToken, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Subscription>();

            if (string.IsNullOrEmpty(accountId) || string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(accessToken))
                throw new ArgumentException("User account, user ID, and access token must be provided.");

            _logger.LogInformation("[Subscription]: Initializing WebSocket for user: {AccountId}", accountId);

            GetSubscriptionTypes().ToList().ForEach(v => _updateHandlers.TryAdd(v, OnConnectReceived));

            _customClientWebSocket = new CustomWebSocket(new Uri(EndPoints.WebSocketUrl), OnMessageReceived, loggerFactory);

            TouchLineSubscription = new TouchlineSubscription(this, loggerFactory);
            TouchLineSubscription.GetSubscriptionTypes().ToList().ForEach(v => _updateHandlers.TryAdd(v, TouchLineSubscription.OnMessageReceived));

            OrderSubscription = new OrderSubscription(this, accountId, userId, loggerFactory);
            OrderSubscription.GetSubscriptionTypes().ToList().ForEach(v => _updateHandlers.TryAdd(v, OrderSubscription.OnMessageReceived));

            QuoteSubscription = new QuoteSubscription(this, loggerFactory);
            QuoteSubscription.GetSubscriptionTypes().ToList().ForEach(v => _updateHandlers.TryAdd(v, QuoteSubscription.OnMessageReceived));

            AlertSubscription = new AlertSubscription(this, loggerFactory, null);
            AlertSubscription.GetSubscriptionTypes().ToList().ForEach(v => _updateHandlers.TryAdd(v, AlertSubscription.OnMessageReceived));
        }

        /// <summary>
        /// Factory for async initialization.
        /// </summary>
        public static async Task<Subscription> CreateAsync(string accountId, string userId, string accessToken, ILoggerFactory loggerFactory)
        {
            var instance = new Subscription(accountId, userId, accessToken, loggerFactory);

            instance._customClientWebSocket.Start(); // non-blocking

            var request = new ConnectRequest
            {
                AccountId = accountId,
                RequestType = SubscriptionType.Connect,
                UserId = userId,
                UserSessionToken = accessToken
            };

            if (!await instance.SendRequestAsync(request))
            {
                throw new InvalidOperationException("Failed to send connect message to WebSocket server.");
            }

            return instance;
        }

        private Task OnConnectReceived(object? obj, SubscriptionEventArgs subscriptionEvent)
        {
            if (string.IsNullOrEmpty(subscriptionEvent?.RawMessage))
            {
                _logger.LogWarning("[Subscription]: Empty message received.");
                return Task.CompletedTask;
            }

            var msg = JsonConvert.DeserializeObject<ConnectResponse>(subscriptionEvent.RawMessage);
            if (msg?.RequestType == SubscriptionType.ConnectAck)
            {
                if (!msg.Status?.Equals("Ok", StringComparison.OrdinalIgnoreCase) ?? true)
                {
                    _logger.LogError("[Subscription]: ConnectAck failed: {Message}", subscriptionEvent.RawMessage);
                }
                else
                {
                    _logger.LogDebug("[Subscription]: ConnectAck OK: {Message}", subscriptionEvent.RawMessage);
                }
            }
            return Task.CompletedTask;
        }

        public async Task<bool> SendRequestAsync<T>(T request)
        {
            var serializedMessage = JsonConvert.SerializeObject(request);
            return await _customClientWebSocket.SendMessageAsync(serializedMessage);
        }

        private async Task OnMessageReceived(object? sender, string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                _logger.LogWarning("[Subscription]: Empty message received.");
                return;
            }

            var msg = JsonConvert.DeserializeObject<BaseSubscriptionRequest>(message);
            if (msg == null)
            {
                _logger.LogError("[Subscription]: Unrecognized message: {Message}", message);
                return;
            }

            if (_updateHandlers.TryGetValue(msg.RequestType, out var handler) && handler is not null)
            {
                await handler.Invoke(this, new SubscriptionEventArgs(msg.RequestType, message));
            }
            else
            {
                _logger.LogError("[Subscription]: No handler for message type {Type}.", msg.RequestType);
            }
        }

        public IEnumerable<SubscriptionType> GetSubscriptionTypes() => [SubscriptionType.ConnectAck];

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
            await TouchLineSubscription.DisposeAsync();
            await OrderSubscription.DisposeAsync();
            await QuoteSubscription.DisposeAsync();
            await AlertSubscription.DisposeAsync();

            await _customClientWebSocket.DisposeAsync();
            _logger.LogInformation("{0}: Disposed gracefully", GetType().Name);
            // Dispose other sync-only resources here (e.g., timers, files)
        }

    }

}
