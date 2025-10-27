using FlatTrade;
using FlatTrade.Common.Types.Base;
using FlatTrade.SubscriptionManager;
using FlatTrade.SubscriptionManager.TouchLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StrategyEngine.Model;
using StrategyEngine.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StrategyEngine.BrokerData
{
    internal class TouchLine : IDisposable
    {
        private bool _disposed = false;
        private readonly Api _api;
        private readonly ILogger<TouchLine> _logger;
        private readonly IConfiguration _config;
        private readonly ContextAccessor _contextAccessor;

        private readonly OnUpdate? _onTouchLine;

        private readonly Helpers.Queue<TouchLineSubscriptionUpdates> _queue;

        private List<SelectedSymbol> _subscribedSymbols = [];
        public TouchLine(IConfiguration config, ContextAccessor contextAccessor, Api api, OnUpdate? onUpdate, ILoggerFactory loggerFactory)
        {
            _api = api;
            _config = config;
            _logger = loggerFactory.CreateLogger<TouchLine>();
            _contextAccessor = contextAccessor;
            _onTouchLine = onUpdate;
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


            _logger.LogInformation("Subscribed to touchline updates successfully.");
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
                if (_onTouchLine is not null)
                    await _onTouchLine(new StrategyOnTouchLineSnapshot
                    {
                        Exchange = update.Exchange,
                        Token = update.Token,
                        TradingSymbol = update.TradingSymbol,
                    });
            }
        }

        private Task OnTouchLineUpdates(object? _, SubscriptionType subscriptionType, string rawMessage, object? subscriptionObject)
        {
            var msg = string.Format("Message processed: '{0}'", rawMessage);

            switch (subscriptionType)
            {
                case SubscriptionType.ConnectAck:
                    _logger.LogInformation("[OnTouchLineUpdates-ConnectAck] {msg}", msg);
                    SubscribeTouchLineAsync(_subscribedSymbols).GetAwaiter().GetResult();
                    break;
                case SubscriptionType.SubscribeTouchLineAck:
                    _logger.LogInformation("[OnTouchLineUpdates-SubscribeTouchLineAck] {msg}", msg);
                    UpdateTouchLines((TouchLineSubscriptionRequestAck)subscriptionObject!);
                    break;
                case SubscriptionType.UnsubscribeTouchLineAck:
                    _logger.LogInformation("[OnTouchLineUpdates-UnSubscribeTouchLineAck] {msg}", msg);
                    UnSubscribeTouchLineAsync(_subscribedSymbols).GetAwaiter().GetResult();
                    break;
                case SubscriptionType.SubscribeTouchLineUpdates:
                    _logger.LogInformation("[OnTouchLineUpdates-SubscribeTouchLineUpdates] {msg}", msg);
                    if (subscriptionObject is TouchLineSubscriptionUpdates Object)
                        _queue.WriteAsync(Object).GetAwaiter().GetResult();
                    break;
                default:
                    _logger.LogWarning("[OnTouchLineUpdates]: unknown message type '{type}' Msg '{msg}'", subscriptionType, msg);
                    break;
            }
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this); // Prevent finalizer from running again
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources here
                    _queue.WriteComplete().GetAwaiter().GetResult();
                    _queue.Dispose();
                }
                // Dispose unmanaged resources here if any
                _disposed = true;
            }
        }

        // Finalizer (only if you have unmanaged resources)
        ~TouchLine()
        {
            Dispose(false);
        }
    }
}
