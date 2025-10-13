using FlatTrade;
using FlatTrade.Common.Types.Base;
using FlatTrade.SubscriptionManager;
using FlatTrade.SubscriptionManager.TouchLine;
using Microsoft.Extensions.Logging;
using StrategyEngine.Model;
using StrategyEngine.Strategy;
using static StrategyEngine.StrategyProcessor;

namespace StrategyEngine
{
    internal class TouchLineDetails : IDisposable
    {
        private bool _disposed = false;
        private readonly Api _api;
        private readonly ILogger<TouchLineDetails> _logger;
        private readonly OnStrategyEvents? _onStrategyEvents;

        private readonly Helpers.Queue<TouchLineSubscriptionUpdates> _queue;

        private List<SelectedSymbol> _subscribedSymbols = [];
        public TouchLineDetails(Api api, OnStrategyEvents? onStrategyEvents, ILoggerFactory loggerFactory)
        {
            _api = api;
            _logger = loggerFactory.CreateLogger<TouchLineDetails>();
            _onStrategyEvents = onStrategyEvents;
            _queue = new(50000, "TouchLineUpdateQueue", OnTouchLineUpdates, loggerFactory);
        }

        public async Task<bool> SubscribeTouchLineAsync(IEnumerable<SelectedSymbol> selectedSymbols)
        {            
            if(selectedSymbols is null || !selectedSymbols.Any())
            {
                _logger.LogWarning("No symbols provided for touchline subscription.");
                return false;
            }
            _subscribedSymbols = [.._subscribedSymbols.Union(selectedSymbols)];
            var selectionProjection = selectedSymbols.Select(a => new KeyValuePair<Exchange, long>(a.Exchange, a.Token));

            if (_api.Subscription.TouchLineSubscription._onSubscriptionEvents is null)
                _api.Subscription.TouchLineSubscription._onSubscriptionEvents = OnTouchLineUpdates;

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

        private void UpdateTouchLines(IEnumerable<TouchLineSubscriptionUpdates> touchLines)
        {
            //foreach (var touchLine in touchLines)
            //{
            //    var key = new KeyValuePair<Exchange, long>(touchLine.Exchange, touchLine.Token);
            //    if (TouchLines.TryGetValue(key, out ConcurrentDictionary<ChartInterval, ConcurrentSortedList<TouchLineSubscriptionUpdates>>? value) && value is not null)
            //    {
            //        foreach (var interval in Intervals)
            //        {
            //            if (!value.TryGetValue(interval, out ConcurrentSortedList<TouchLineSubscriptionUpdates>? intervalList) || intervalList is null)
            //            {
            //                intervalList = new ConcurrentSortedList<TouchLineSubscriptionUpdates>(new TouchLineComparer());
            //                value[interval] = intervalList;
            //            }
            //            var existingTouchLine = intervalList.Find(t => t.Timestamp == touchLine.Timestamp);
            //            if (existingTouchLine is not null)
            //            {
            //                intervalList.Remove(existingTouchLine);
            //            }
            //            if (touchLine.Interval == interval)
            //            {
            //                intervalList.Add(touchLine);
            //            }
            //        }
            //        value.Add(touchLine);
            //    }
            //    else
            //    {
            //        TouchLines[key] = [touchLine];
            //    }
            //}
        }

        private async Task OnTouchLineUpdates(TouchLineSubscriptionUpdates Object)
        {
            if (Object is not null)
            {
                //var orderInfo = OrderInfo.ConvertFrom(Object);
                //if (orderInfo is not null)
                //    await UpdateOrderBook([orderInfo]);
                if (_onStrategyEvents is not null)
                    await _onStrategyEvents(new StrategyEvent
                    {
                        EventType = StrategyEngineEventType.TouchLine,
                        TradingSymbol = Object.TradingSymbol,
                        Token = Object.Token
                    });
            }
        }

        private async Task OnTouchLineUpdates(object? _, SubscriptionType subscriptionType, string rawMessage, object? subscriptionObject)
        {
            var msg = string.Format($": Message processed: '{rawMessage}'");

            switch (subscriptionType)
            {
                case SubscriptionType.ConnectAck:
                    _logger.LogInformation("[OnTouchLineUpdates-ConnectAck] {msg}", msg);
                    //UpdateTouchLines(touchLineDetails);
                    await SubscribeTouchLineAsync(_subscribedSymbols);
                    break;
                case SubscriptionType.SubscribeTouchLineAck:
                    _logger.LogInformation("[OnTouchLineUpdates-SubscribeTouchLineAck] {msg}", msg);
                    //UpdateTouchLines([(TouchLineSubscriptionUpdates)subscriptionObject!]);
                    break;
                case SubscriptionType.UnsubscribeTouchLineAck:
                    _logger.LogInformation("[OnTouchLineUpdates-UnSubscribeTouchLineAck] {msg}", msg);
                    await UnSubscribeTouchLineAsync(_subscribedSymbols);
                    break;
                case SubscriptionType.SubscribeTouchLineUpdates:
                    _logger.LogInformation("[OnTouchLineUpdates-SubscribeTouchLineUpdates] {msg}", msg);
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
        ~TouchLineDetails()
        {
            Dispose(false);
        }
    }
}
