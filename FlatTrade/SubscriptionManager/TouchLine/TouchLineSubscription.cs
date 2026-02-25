using Common.Helpers;
using FlatTrade.Types.Base;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using HelperUtility = FlatTrade.SubscriptionManager.Helper.HelperUtility;

namespace FlatTrade.SubscriptionManager.TouchLine
{
    public class TouchlineSubscription(Subscription subscription, ILoggerFactory loggerFactory) : ISubscriptionType, IUpdateHandler, IDisposable, IAsyncDisposable
    {
        private bool _disposed = false;
        private readonly List<KeyValuePair<Exchange, long>> _subscribedTokens = [];
        private readonly ILogger<TouchlineSubscription> _logger = loggerFactory.CreateLogger<TouchlineSubscription>();
        private readonly Subscription _subscription = subscription;
        public OnSubscriptionEvents? OnSubscriptionEvents = null;

        public async Task<bool> SubscribeAsync(IEnumerable<KeyValuePair<Exchange, long>> exchangeSymbolTokenPair)
        {
            var validExchangePairs = HelperUtility.GetValidExchangePairs(exchangeSymbolTokenPair);
            var pair = HelperUtility.CreateExchangeSymbolTokenPair(validExchangePairs);
            if (string.IsNullOrEmpty(pair))
            {
                _logger.LogWarning("No valid exchange symbol token pairs provided for touchline subscription.");
                return false;
            }
            _subscribedTokens.AddRange(validExchangePairs);
            var request = new TouchLineSubscriptionRequest { SubscriptionScriptList = pair, RequestType = SubscriptionType.SubscribeTouchLine };
            return await _subscription.SendRequestAsync(request);
        }

        public async Task<bool> UnsubscribeAsync(IEnumerable<KeyValuePair<Exchange, long>> exchangeSymbolTokenPair)
        {
            var validExchangePairs = HelperUtility.GetValidExchangePairs(exchangeSymbolTokenPair);
            var pair = HelperUtility.CreateExchangeSymbolTokenPair(validExchangePairs);
            if (string.IsNullOrEmpty(pair))
            {
                _logger.LogWarning("No valid exchange symbol token pairs provided for trade un-subscription.");
                return true;
            }
            _subscribedTokens.RemoveAll(e => validExchangePairs.Any(v => v.Key == e.Key && v.Value == e.Value));
            var request = new TouchLineUnsubscriptionRequest { SubscriptionScriptList = pair, RequestType = SubscriptionType.UnsubscribeTouchLine };
            if (await _subscription.SendRequestAsync(request))
                return true;            

            _logger.LogError("Failed to unsubscribe from touchline updates.");
            return false;
        }

        public IEnumerable<SubscriptionType> GetSubscriptionTypes()
        {
            return [SubscriptionType.ConnectAck,
                    SubscriptionType.SubscribeTouchLineUpdates,
                    SubscriptionType.SubscribeTouchLineAck,
                    SubscriptionType.UnsubscribeTouchLineAck];
        }

        public async Task OnMessageReceived(object? obj, SubscriptionEventArgs subscriptionEvent)
        {
            if (string.IsNullOrEmpty(subscriptionEvent?.RawMessage))
            {
                _logger.LogWarning("[TouchLine Subscription]: Received empty message from WebSocket.");
                return;
            }

            try
            {
                switch (subscriptionEvent.SubscriptionType)
                {
                    case SubscriptionType.ConnectAck:
                        _logger.LogInformation("[TouchLine Subscription]: ConnectAck received.");
                        if (OnSubscriptionEvents != null)
                        {
                            _logger.LogInformation("[TouchLine Subscription]: Subscribing for the touchline updates .");
                            await SubscribeAsync(_subscribedTokens);
                            await OnSubscriptionEvents.Invoke(this, subscriptionEvent.SubscriptionType, subscriptionEvent.RawMessage, JsonConvert.DeserializeObject<ConnectResponse>(subscriptionEvent.RawMessage));
                        }
                        break;
                    case SubscriptionType.SubscribeTouchLineUpdates:
                        RestHttpClientExtension.CheckJsonAgainstModel<TouchLineSubscriptionUpdates>(subscriptionEvent.RawMessage, false);
                        if (OnSubscriptionEvents != null)
                            await OnSubscriptionEvents.Invoke(this, subscriptionEvent.SubscriptionType, subscriptionEvent.RawMessage, JsonConvert.DeserializeObject<TouchLineSubscriptionUpdates>(subscriptionEvent.RawMessage));
                        break;
                    case SubscriptionType.SubscribeTouchLineAck:
                        RestHttpClientExtension.CheckJsonAgainstModel<TouchLineSubscriptionRequestAck>(subscriptionEvent.RawMessage, false);
                        if (OnSubscriptionEvents != null)
                            await OnSubscriptionEvents.Invoke(this, subscriptionEvent.SubscriptionType, subscriptionEvent.RawMessage, JsonConvert.DeserializeObject<TouchLineSubscriptionRequestAck>(subscriptionEvent.RawMessage));
                        break;
                    case SubscriptionType.UnsubscribeTouchLineAck:
                        RestHttpClientExtension.CheckJsonAgainstModel<TouchLineUnsubscriptionRequestAck>(subscriptionEvent.RawMessage, false);
                        if (OnSubscriptionEvents != null)
                            await OnSubscriptionEvents.Invoke(this, subscriptionEvent.SubscriptionType, subscriptionEvent.RawMessage, JsonConvert.DeserializeObject<TouchLineUnsubscriptionRequestAck>(subscriptionEvent.RawMessage));
                        break;
                    default:
                        _logger.LogError("[TouchLine Subscription]: Incorrect message received in OnMessageReceived handler: '{message}'", subscriptionEvent.RawMessage);
                        break;
                }
            }
            catch (JsonSerializationException e)
            {
                _logger.LogError("[TouchLine Subscription]: OnMessageReceived Serialization Exception : {e.Message}. Message {data}", e.Message, subscriptionEvent.RawMessage);
            }
            catch (ArgumentOutOfRangeException e)
            {
                _logger.LogError("[TouchLine Subscription]: OnMessageReceived ArgumentOutOfRange Exception : {e.Message}. Message {data}", e.Message, subscriptionEvent.RawMessage);
            }
            catch (Exception e)
            {
                _logger.LogError("[TouchLine Subscription]: OnMessageReceived Exception : {e.Message}. Message {data}", e.Message, subscriptionEvent.RawMessage);
            }
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

        protected async ValueTask DisposeAsyncCore()
        {
            if (_disposed)
                return;

            _disposed = true;

            // Dispose async resources
            if (_subscribedTokens is not null && _subscribedTokens.Any())
                await UnsubscribeAsync(_subscribedTokens);

            _subscribedTokens!.Clear();
            OnSubscriptionEvents = null;
            
            _logger.LogInformation("{0}: Disposed gracefully", GetType().Name);
            // Dispose other sync-only resources here (e.g., timers, files)
        }

    }
}
