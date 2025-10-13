namespace FlatTrade.SubscriptionManager
{
    using Common.Transport;
    using FlatTrade.Common.Throttle;
    using FlatTrade.SubscriptionManager.Alert;
    using FlatTrade.SubscriptionManager.Order;
    using FlatTrade.SubscriptionManager.Quote;
    using FlatTrade.SubscriptionManager.TouchLine;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    public class SubscriptionEventArgs(SubscriptionType subscriptionType, string rawMessage) : EventArgs
    {
        public string RawMessage { get; } = rawMessage;
        public SubscriptionType SubscriptionType { get; } = subscriptionType;
    }

    public delegate Task OnSubscriptionEvents(object? sender, SubscriptionType subscriptionType, string rawMessage, object? subscriptionObject);

    /// <summary>
    /// 
    /// </summary>
    public class Subscription : ISubscriptionType
    {
        private readonly ILogger<Subscription> _logger;
        private readonly CustomWebSocket _customClientWebSocket;

        public readonly TouchlineSubscription TouchLineSubscription;
        public readonly OrderSubscription OrderSubscription;
        public readonly QuoteSubscription QuoteSubscription;
        public readonly AlertSubscription AlertSubscription;

        readonly ConcurrentDictionary<SubscriptionType, AsyncEventHandler<SubscriptionEventArgs>> _updateHandlers = [];
        public Subscription(string accountId, string userId, string accessToken, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Subscription>();
            if (string.IsNullOrEmpty(accountId) || string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(accessToken))
            {
                throw new ArgumentException("User account, user ID, and access token must be provided.");
            }
            var _accountId = accountId;
            var _userId = userId;

            _logger.LogInformation("[Subscription Logic]: Initializing WebSocket connection for user: {_accountId} with ID: {_userId}", _accountId, _userId);
            GetSubscriptionTypes().ToList().ForEach(v => { _updateHandlers.TryAdd(v, OnConnectReceived); });

            _customClientWebSocket = new CustomWebSocket(new Uri(EndPoints.WebSocketUrl), loggerFactory);
            _customClientWebSocket.OnMessageReceived += OnMessageReceived;

            TouchLineSubscription = new TouchlineSubscription(this, loggerFactory);
            TouchLineSubscription.GetSubscriptionTypes().ToList().ForEach(v => { _updateHandlers.TryAdd(v, TouchLineSubscription.OnMessageReceived); });

            OrderSubscription = new OrderSubscription(this, _accountId, _userId, loggerFactory);
            OrderSubscription.GetSubscriptionTypes().ToList().ForEach(v => { _updateHandlers.TryAdd(v, OrderSubscription.OnMessageReceived); });

            QuoteSubscription = new QuoteSubscription(this, loggerFactory);
            QuoteSubscription.GetSubscriptionTypes().ToList().ForEach(v => { _updateHandlers.TryAdd(v, QuoteSubscription.OnMessageReceived); });

            AlertSubscription = new AlertSubscription(this, loggerFactory, null);
            AlertSubscription.GetSubscriptionTypes().ToList().ForEach(v => { _updateHandlers.TryAdd(v, AlertSubscription.OnMessageReceived); });

            if (_customClientWebSocket.StartAsync().GetAwaiter().GetResult())
            {
                var request = new ConnectRequest { AccountId = _accountId, RequestType = SubscriptionType.Connect, UserId = _userId, UserSessionToken = accessToken };
                if (!SendRequestAsync(request).GetAwaiter().GetResult())
                    throw new InvalidOperationException("Failed to send connect message to WebSocket server.");
            }
            else
            {
                var msg = "Unable to connect to Websocket";
                _logger.LogCritical("{msg}", msg);
                throw new InvalidOperationException(msg);
            }
        }

        private Task OnConnectReceived(object? obj, SubscriptionEventArgs subscriptionEvent)
        {
            if (string.IsNullOrEmpty(subscriptionEvent?.RawMessage))
            {
                _logger.LogWarning("[Subscription OnConnect]: Received empty message from WebSocket.");
                return Task.CompletedTask;
            }

            var msg = JsonConvert.DeserializeObject<ConnectResponse>(subscriptionEvent.RawMessage);
            if (msg?.RequestType == SubscriptionType.ConnectAck)
            {
                if (!msg.Status?.Equals("Ok", StringComparison.OrdinalIgnoreCase) ?? true)
                {
                    _logger.LogError("[Subscription Logic]: ConnectAck failed with status: {message}", subscriptionEvent.RawMessage);
                    return Task.CompletedTask;
                }

                _logger.LogDebug("[Subscription Logic]: ConnectAck message received: '{message}'", subscriptionEvent.RawMessage);
                return Task.CompletedTask;
            }
            _logger.LogError("[Subscription Logic]: Incorrect message received in connect handler: '{message}'", subscriptionEvent.RawMessage);
            return Task.CompletedTask;
        }

        //[Throttle]
        public async virtual Task<bool> SendRequestAsync<T>(T request)
        {
            var serializedMessage = JsonConvert.SerializeObject(request);
            return await _customClientWebSocket.SendMessageAsync(serializedMessage!);
        }

        private async Task OnMessageReceived(object? sender, string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                _logger.LogWarning("[Subscription OnMessage]: Received empty message from WebSocket.");
                return;
            }

            var mesg = JsonConvert.DeserializeObject<BaseSubscriptionRequest>(message);
            if (mesg == null)
            {
                _logger.LogError("[Subscription Logic]: Unrecognized message type received: '{message}'", message);
                return;
            }

            if (mesg.RequestType == SubscriptionType.ConnectAck)
            {
                var subscriptionEventArgs = new SubscriptionEventArgs(mesg.RequestType, message);
                await TouchLineSubscription.OnMessageReceived(mesg, subscriptionEventArgs);
                await AlertSubscription.OnMessageReceived(mesg, subscriptionEventArgs);
                await QuoteSubscription.OnMessageReceived(mesg, subscriptionEventArgs);
                await OrderSubscription.OnMessageReceived(mesg, subscriptionEventArgs);
                return;
            }

            if (_updateHandlers.TryGetValue(mesg.RequestType, out AsyncEventHandler<SubscriptionEventArgs>? handler) && handler != null)
            {
                if (handler is not null)
                {
                    var raw = JsonConvert.DeserializeObject<string>(message);
                    await handler.Invoke(this, new SubscriptionEventArgs(mesg.RequestType, raw!));
                }

                return;
            }

            _logger.LogError("[Subscription Logic]: No handler registered for message type '{mesg.RequestType}' received: '{message}'", mesg.RequestType, message);
        }

        public IEnumerable<SubscriptionType> GetSubscriptionTypes()
        {
            return [SubscriptionType.ConnectAck];
        }
    }
}
