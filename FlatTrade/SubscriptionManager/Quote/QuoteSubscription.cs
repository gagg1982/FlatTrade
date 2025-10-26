using FlatTrade.Common.Types.Base;
using FlatTrade.SubscriptionManager.Helper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FlatTrade.SubscriptionManager.Quote
{
    public class QuoteSubscription(Subscription subscription, ILoggerFactory loggerFactory) : ISubscriptionType, IUpdateHandler
    {
        private readonly ILogger<QuoteSubscription> _logger = loggerFactory.CreateLogger<QuoteSubscription>();
        private readonly Subscription _subscription = subscription;
        public OnSubscriptionEvents? OnSubscriptionEvents = null;
        private readonly List<KeyValuePair<Exchange, long>> _subscribedTokens = [];
        public async Task<bool> SubscribeAsync(IEnumerable<KeyValuePair<Exchange, long>> exchangeSymbolTokenPair)
        {

            var validExchangePairs = HelperUtility.GetValidExchangePairs(exchangeSymbolTokenPair);
            var pair = HelperUtility.CreateExchangeSymbolTokenPair(validExchangePairs);
            if (pair.Length == 0)
            {
                _logger.LogWarning("No valid exchange symbol token pairs provided for quote subscription.");
                return false;
            }
            _subscribedTokens.AddRange(validExchangePairs);
            var request = new QuoteSubscriptionRequest { SubscriptionScriptList = pair, RequestType = SubscriptionType.SubscribeQuote };
            return await _subscription.SendRequestAsync(request);
        }


        public async Task<bool> UnsubscribeAsync(IEnumerable<KeyValuePair<Exchange, long>> exchangeSymbolTokenPair)
        {
            var validExchangepairs = HelperUtility.GetValidExchangePairs(exchangeSymbolTokenPair);
            var pair = HelperUtility.CreateExchangeSymbolTokenPair(validExchangepairs);
            if (pair.Length == 0)
            {
                _logger.LogWarning("No valid exchange symbol token pairs provided for quote un-subscription.");
                return true;
            }
            _subscribedTokens.RemoveAll(e => validExchangepairs.Any(v => v.Key == e.Key && v.Value == e.Value));
            var request = new QuoteUnsubscriptionRequest { SubscriptionScriptList = pair, RequestType = SubscriptionType.UnsubscribeQuote };
            return await _subscription.SendRequestAsync(request);
        }

        public IEnumerable<SubscriptionType> GetSubscriptionTypes()
        {

            return [
                    SubscriptionType.SubscribeQuoteUpdates,
                    SubscriptionType.SubscribeQuoteAck,
                    SubscriptionType.UnsubscribeQuoteAck];

        }
        public async Task OnMessageReceived(object? obj, SubscriptionEventArgs subscriptionEvent)
        {
            if (string.IsNullOrEmpty(subscriptionEvent.RawMessage))
            {
                _logger.LogWarning("[Quote Subscription]: Received empty message from WebSocket.");
                return;
            }

            try
            {
                switch (subscriptionEvent.SubscriptionType)
                {
                    case SubscriptionType.ConnectAck:
                        _logger.LogInformation("[Quote Subscription]: ConnectAck received.");
                        if (OnSubscriptionEvents != null)
                        {
                            _logger.LogInformation("[Quote Subscription]: Subscribing for the quotes again.");
                            await SubscribeAsync(_subscribedTokens);
                            await OnSubscriptionEvents.Invoke(this, subscriptionEvent.SubscriptionType, subscriptionEvent.RawMessage, JsonConvert.DeserializeObject<ConnectResponse>(subscriptionEvent.RawMessage));
                        }
                        break;
                    case SubscriptionType.SubscribeQuoteAck:
                        if (OnSubscriptionEvents != null)
                            await OnSubscriptionEvents.Invoke(this, subscriptionEvent.SubscriptionType, subscriptionEvent.RawMessage, JsonConvert.DeserializeObject<QuoteSubscriptionRequestAck>(subscriptionEvent.RawMessage));
                        break;
                    case SubscriptionType.SubscribeQuoteUpdates:
                        if (OnSubscriptionEvents != null)
                            await OnSubscriptionEvents.Invoke(this, subscriptionEvent.SubscriptionType, subscriptionEvent.RawMessage, JsonConvert.DeserializeObject<QuoteSubscriptionUpdates>(subscriptionEvent.RawMessage));
                        break;
                    case SubscriptionType.UnsubscribeQuoteAck:
                        if (OnSubscriptionEvents != null)
                            await OnSubscriptionEvents.Invoke(this, subscriptionEvent.SubscriptionType, subscriptionEvent.RawMessage, JsonConvert.DeserializeObject<QuoteUnsubscriptionRequestAck>(subscriptionEvent.RawMessage));
                        break;
                    default:
                        _logger.LogError("[Quote Subscription]: Incorrect message received in OnMessageReceived handler: '{message}'", subscriptionEvent.RawMessage);
                        break;
                }
            }
            catch (JsonSerializationException e)
            {
                _logger.LogError("[Quote Subscription]: OnMessageReceived Serialization Exception : {e.Message}. Message {data}", e.Message, subscriptionEvent.RawMessage);
            }
            catch (ArgumentOutOfRangeException e)
            {
                _logger.LogError("[Quote Subscription]: OnMessageReceived ArgumentOutOfRange Exception : {e.Message}. Message {data}", e.Message, subscriptionEvent.RawMessage);
            }
            catch (Exception e)
            {
                _logger.LogError("[Quote Subscription]: OnMessageReceived Exception : {e.Message}. Message {data}", e.Message, subscriptionEvent.RawMessage);
            }
        }
    }
}
