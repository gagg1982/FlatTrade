using FlatTrade.SubscriptionManager.Order;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FlatTrade.SubscriptionManager.Alert
{
    public class AlertSubscription(Subscription subscription, ILoggerFactory loggerFactory, OnSubscriptionEvents? onSubscriptionEvents) : ISubscriptionType, IUpdateHandler, IDisposable, IAsyncDisposable
    {
        private bool _disposed = false;
        private OnSubscriptionEvents? _onSubscriptionEvents = onSubscriptionEvents;
        private readonly ILogger<AlertSubscription> _logger = loggerFactory.CreateLogger<AlertSubscription>();
        private readonly Subscription _subscription = subscription;
        public IEnumerable<SubscriptionType> GetSubscriptionTypes()
        {
            return [SubscriptionType.ConnectAck, SubscriptionType.SubscribeAlertMessages];
        }

        public async Task OnMessageReceived(object? obj, SubscriptionEventArgs subscriptionEvent)
        {
            if (string.IsNullOrEmpty(subscriptionEvent.RawMessage))
            {
                _logger.LogWarning("[Alert Subscription]: Received empty message from WebSocket.");
                return;
            }

            try
            {
                switch (subscriptionEvent.SubscriptionType)
                {
                    case SubscriptionType.ConnectAck:
                        _logger.LogInformation("[Alert Subscription]: ConnectAck received.");
                        if (_onSubscriptionEvents != null)
                        {
                            _logger.LogInformation("[Alert Subscription]: Subscribing for the Alert again.");
                            await _onSubscriptionEvents.Invoke(this, subscriptionEvent.SubscriptionType, subscriptionEvent.RawMessage, JsonConvert.DeserializeObject<ConnectResponse>(subscriptionEvent.RawMessage));
                        }
                        break;
                    case SubscriptionType.SubscribeAlertMessages:
                        if (_onSubscriptionEvents != null)
                            await _onSubscriptionEvents.Invoke(this, subscriptionEvent.SubscriptionType, subscriptionEvent.RawMessage, JsonConvert.DeserializeObject<OrderSubscriptionRequestAck>(subscriptionEvent.RawMessage));
                        break;
                    default:
                        _logger.LogError("[Alert Subscription]: Incorrect message received in OnMessageReceived handler: '{message}'", subscriptionEvent.RawMessage);
                        break;
                }
            }
            catch (JsonSerializationException e)
            {
                _logger.LogError("[Alert Subscription]: OnMessageReceived Serialization Exception : {e.Message}", e.Message);
            }
            catch (ArgumentOutOfRangeException e)
            {
                _logger.LogError("[Alert Subscription]: OnMessageReceived ArgumentOutOfRange Exception : {e.Message}", e.Message);
            }
            catch (Exception e)
            {
                _logger.LogError("[Alert Subscription]: OnMessageReceived Exception : {e.Message}", e.Message);
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

        protected ValueTask DisposeAsyncCore()
        {
            if (_disposed)
                return default;

            _disposed = true;

            // Dispose async resources
            _onSubscriptionEvents = null;

            _logger.LogInformation("{0}: Disposed gracefully", GetType().Name);
            // Dispose other sync-only resources here (e.g., timers, files)
            return default;
        }
    }
}
