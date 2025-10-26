using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FlatTrade.SubscriptionManager.Order
{

    public class OrderSubscription(Subscription subscription, string accountId, string userId, ILoggerFactory loggerFactory) : ISubscriptionType, IUpdateHandler
    {
        private readonly ILogger<OrderSubscription> _logger = loggerFactory.CreateLogger<OrderSubscription>();
        private readonly Subscription _subscription = subscription;
        private OnSubscriptionEvents? _onSubscriptionEvents = null;


        private readonly string _accountId = accountId;
        private readonly string _userId = userId;
        public async Task<bool> UnsubscribeAsync(OnSubscriptionEvents handler)
        {
            if (_onSubscriptionEvents is null || handler is null)
            {
                _logger.LogWarning("No handler is set for order subscription. Cannot unsubscribe or input handler is null.");
                return false;
            }

            var request = new OrderUnsubscriptionRequest { RequestType = SubscriptionType.UnsubscribeOrder };

            if (await _subscription.SendRequestAsync(request))
            {
                _logger.LogInformation("Unsubscribed from order subscription updates successfully.");
                _onSubscriptionEvents = null;
                return true;
            }

            _logger.LogError("Failed to unsubscribe from order subscription updates.");
            return false;
        }

        public async Task<bool> SubscribeAsync(OnSubscriptionEvents handler)
        {
            if (handler is null || _onSubscriptionEvents is not null)
            {
                _logger.LogWarning("Already subscribed to order subscription updates. Cannot subscribe again or input handler is null.");
                return true;
            }

            _onSubscriptionEvents = handler;

            var request = new SubscribeOrderUpdatesRequest { AccountId = _accountId, RequestType = SubscriptionType.SubscribeOrder };

            if (await _subscription.SendRequestAsync(request))
            {
                _logger.LogInformation("Subscribed to order updates successfully.");
                return true;
            }
            _logger.LogError("Failed to subscribe to order updates.");
            return false;
        }

        public IEnumerable<SubscriptionType> GetSubscriptionTypes()
        {
            return [
                SubscriptionType.SubscribeOrderUpdate,
                SubscriptionType.SubscribeOrderAck,
                SubscriptionType.UnsubscribeOrderAck];
        }

        public async Task OnMessageReceived(object? obj, SubscriptionEventArgs subscriptionEvent)
        {
            if (string.IsNullOrEmpty(subscriptionEvent.RawMessage))
            {
                _logger.LogWarning("[Order Subscription]: Received empty message from WebSocket.");
                return;
            }

            try
            {
                switch (subscriptionEvent.SubscriptionType)
                {
                    case SubscriptionType.ConnectAck:
                        _logger.LogInformation("[Order Subscription]: ConnectAck received.");
                        if (_onSubscriptionEvents is not null)
                        {
                            _logger.LogInformation("[Order Subscription]: Subscribing for the order updates.");
                            await SubscribeAsync(_onSubscriptionEvents);
                            await _onSubscriptionEvents.Invoke(this, subscriptionEvent.SubscriptionType, subscriptionEvent.RawMessage, JsonConvert.DeserializeObject<ConnectResponse>(subscriptionEvent.RawMessage));
                        }
                        break;
                    case SubscriptionType.SubscribeOrderUpdate:
                        if (_onSubscriptionEvents is not null)
                            await _onSubscriptionEvents.Invoke(this, subscriptionEvent.SubscriptionType, subscriptionEvent.RawMessage, JsonConvert.DeserializeObject<OrderSubscriptionUpdates>(subscriptionEvent.RawMessage));
                        break;
                    case SubscriptionType.SubscribeOrderAck:
                        if (_onSubscriptionEvents is not null)
                            await _onSubscriptionEvents.Invoke(this, subscriptionEvent.SubscriptionType, subscriptionEvent.RawMessage, JsonConvert.DeserializeObject<OrderSubscriptionRequestAck>(subscriptionEvent.RawMessage));
                        break;
                    case SubscriptionType.UnsubscribeOrderAck:
                        if (_onSubscriptionEvents is not null)
                            await _onSubscriptionEvents.Invoke(this, subscriptionEvent.SubscriptionType, subscriptionEvent.RawMessage, JsonConvert.DeserializeObject<OrderUnsubscriptionRequestAck>(subscriptionEvent.RawMessage));
                        break;
                    default:
                        _logger.LogError("[Order Subscription]: Incorrect message received in OnMessageReceived handler: '{message}'", subscriptionEvent.RawMessage);
                        break;
                }
            }
            catch (JsonSerializationException e)
            {
                _logger.LogError("[Order Subscription]: OnMessageReceived Serialization Exception : {e.Message}. Message {data}", e.Message, subscriptionEvent.RawMessage);
            }
            catch (ArgumentOutOfRangeException e)
            {
                _logger.LogError("[Order Subscription]: OnMessageReceived ArgumentOutOfRange Exception : {e.Message}. Message {data}", e.Message, subscriptionEvent.RawMessage);
            }
            catch (Exception e)
            {
                _logger.LogError("[Order Subscription]: OnMessageReceived Exception : {e.Message}. Message {data}", e.Message, subscriptionEvent.RawMessage);
            }
        }
    }
}
