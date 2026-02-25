using FlatTrade;
using FlatTrade.SubscriptionManager;
using FlatTrade.SubscriptionManager.TouchLine;
using FlatTrade.Types.Base;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StrategyEngine.Model;
using StrategyEngine.Strategies;

namespace StrategyEngine.BrokerData
{
    internal sealed class TouchLine : IAsyncDisposable
    {
        private bool _disposed = false;
        private readonly Api _api;
        private readonly ILogger<TouchLine> _logger;
        private readonly IConfiguration _config;
        private readonly ContextAccessor _contextAccessor;

        public static event OnUpdate? OnTouchLine;

        private readonly Helpers.Queue<TouchLineSubscriptionUpdates> _queue;

        private List<SelectedSymbol> _subscribedSymbols = [];
        public TouchLine(IConfiguration config, ContextAccessor contextAccessor, Api api, ILoggerFactory loggerFactory)
        {
            _api = api;
            _config = config;
            _logger = loggerFactory.CreateLogger<TouchLine>();
            _contextAccessor = contextAccessor;

            _queue = new(50000, "TouchLineUpdateQueue", OnTouchLineUpdates, loggerFactory);
        }

        public async Task<bool> SubscribeTouchLineAsync(IEnumerable<SelectedSymbol> selectedSymbols)
        {
            if (selectedSymbols is null || !selectedSymbols.Any())
            {
                _logger.LogWarning("No symbols provided for touchline subscription.");
                return false;
            }
            _subscribedSymbols = [.. _subscribedSymbols.Union(selectedSymbols)];
            var selectionProjection = selectedSymbols.Select(a => new KeyValuePair<Exchange, long>(a.Exchange, a.Token));

            if (_api.Subscription.TouchLineSubscription.OnSubscriptionEvents is null)
                _api.Subscription.TouchLineSubscription.OnSubscriptionEvents = OnTouchLineUpdates;

            var ok = await _api.Subscription.TouchLineSubscription.SubscribeAsync(selectionProjection);
            if (!ok)
            {
                _logger.LogError("Failed to subscribe to touchline updates.");
                return ok;
            }


            _logger.LogDebug("Subscribed to touchline updates successfully.");
            return ok;
        }

        public async Task<bool> UnSubscribeTouchLineAsync(IEnumerable<SelectedSymbol> selectedSymbols)
        {
            if (selectedSymbols is null || !selectedSymbols.Any())
            {
                _logger.LogWarning("No symbols provided for touchline un-subscription.");
                return false;
            }

            var hashSet = new HashSet<SelectedSymbol>(selectedSymbols);
            _subscribedSymbols.RemoveAll(selectedSymbol => hashSet.Contains(selectedSymbol));

            var selectionProjection = selectedSymbols.Select(a => new KeyValuePair<Exchange, long>(a.Exchange, a.Token));
            var ok = await _api.Subscription.TouchLineSubscription.UnsubscribeAsync(selectionProjection);
            if (!ok)
            {
                _logger.LogError("Failed to subscribe to touchline updates.");
                return ok;
            }

            _logger.LogInformation("Un-Subscribed to touchline updates successfully.");
            return ok;
        }

        private static void UpdateTouchLines(TouchLineSubscriptionRequestAck touchLineAck)
        {
            var details = GlobalDataSet.Subscriptions.GetOrAdd(touchLineAck.Token, new SubscriptionDetails());
            details.TouchLineSubscription.AddOrUpdate(touchLineAck.Exchange, touchLineAck, (_, existingValue) => { return existingValue.Update(touchLineAck); });
        }

        private static TouchLineSubscriptionRequestAck UpdateTouchLines(TouchLineSubscriptionUpdates touchLineUpdate)
        {
            var newTouchLineUpdate = new TouchLineSubscriptionRequestAck
            {
                Token = touchLineUpdate.Token,
                Exchange = touchLineUpdate.Exchange,
            };
            newTouchLineUpdate.Update(touchLineUpdate);

            var details = GlobalDataSet.Subscriptions.GetOrAdd(touchLineUpdate.Token, new SubscriptionDetails());
            return details.TouchLineSubscription.AddOrUpdate(touchLineUpdate.Exchange, newTouchLineUpdate, (key, existingValue) => { return existingValue.Update(touchLineUpdate); });
        }

        private async Task OnTouchLineUpdates(TouchLineSubscriptionUpdates Object)
        {
            if (Object is not null)
            {
                var update = UpdateTouchLines(Object);
                if (OnTouchLine is not null)
                    await OnTouchLine.Invoke(new StrategyOnTouchLineSnapshot(update));
            }
        }

        private async Task OnTouchLineUpdates(object? _, SubscriptionType subscriptionType, string rawMessage, object? subscriptionObject)
        {
            var msg = string.Format("Message processed: '{0}'", rawMessage);

            switch (subscriptionType)
            {
                case SubscriptionType.ConnectAck:
                    _logger.LogInformation("[OnTouchLineUpdates-ConnectAck] {msg}", msg);
                    await SubscribeTouchLineAsync(_subscribedSymbols);
                    break;
                case SubscriptionType.SubscribeTouchLineAck:
                    _logger.LogInformation("[OnTouchLineUpdates-SubscribeTouchLineAck] {msg}", msg);
                    UpdateTouchLines((TouchLineSubscriptionRequestAck)subscriptionObject!);
                    break;
                case SubscriptionType.UnsubscribeTouchLineAck:
                    _logger.LogInformation("[OnTouchLineUpdates-UnSubscribeTouchLineAck] {msg}", msg);
                    await UnSubscribeTouchLineAsync(_subscribedSymbols);
                    break;
                case SubscriptionType.SubscribeTouchLineUpdates:
                    //_logger.LogInformation("[OnTouchLineUpdates-SubscribeTouchLineUpdates] {msg}", msg);
                    if (subscriptionObject is TouchLineSubscriptionUpdates Object)
                        await _queue.WriteAsync(Object);
                    break;
                default:
                    _logger.LogWarning("[OnTouchLineUpdates]: unknown message type '{type}' Msg '{msg}'", subscriptionType, msg);
                    break;
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

        private async ValueTask DisposeAsyncCore()
        {
            if (_disposed)
                return;

            _disposed = true;

            // Dispose async resources
            await _queue.DisposeAsync();
            _logger.LogInformation("{0}: Disposed gracefully", GetType().Name);
            // Dispose other sync-only resources here (e.g., timers, files)
        }
    }
}
