using FlatTrade;
using FlatTrade.Common.Types.Base;
using FlatTrade.SubscriptionManager;
using FlatTrade.SubscriptionManager.Quote;
using FlatTrade.SubscriptionManager.TouchLine;
using Microsoft.Extensions.Logging;
using StrategyEngine.Model;
using StrategyEngine.Strategy;
using static StrategyEngine.StrategyProcessor;

namespace StrategyEngine
{
    internal class QuoteDetails : IDisposable
    {
        private bool _disposed = false;
        private readonly Api _api;
        private readonly ILogger<QuoteDetails> _logger;
        private readonly OnStrategyEvents? _onStrategyEvents;

        private readonly Helpers.Queue<QuoteSubscriptionUpdates> _queue;

        private List<SelectedSymbol> _subscribedSymbols = [];
        public QuoteDetails(Api api, OnStrategyEvents? onStrategyEvents, ILoggerFactory loggerFactory)
        {
            _api = api;
            _logger = loggerFactory.CreateLogger<QuoteDetails>();
            _onStrategyEvents = onStrategyEvents;
            _queue = new(50000, "QuoteUpdateQueue", OnQuoteUpdates, loggerFactory);
        }

        public async Task<bool> SubscribeQuoteAsync(IEnumerable<SelectedSymbol> selectedSymbols)
        {            
            if(selectedSymbols is null || !selectedSymbols.Any())
            {
                _logger.LogWarning("No symbols provided for quote subscription.");
                return false;
            }
            _subscribedSymbols = [.._subscribedSymbols.Union(selectedSymbols)];
            var selectionProjection = selectedSymbols.Select(a => new KeyValuePair<Exchange, long>(a.Exchange, a.Token));

            var ok = await _api.Subscription.QuoteSubscription.SubscribeAsync(selectionProjection);
            if (!ok)
            {
                _logger.LogError("Failed to subscribe to quote updates.");
                return ok;
            }

            if (_api.Subscription.QuoteSubscription._onSubscriptionEvents is null)
                _api.Subscription.QuoteSubscription._onSubscriptionEvents = OnQuoteUpdates;
            
            _logger.LogInformation("Subscribed to quote updates successfully.");
            return ok;
        }

        public async Task<bool> UnSubscribeQuoteAsync(IEnumerable<SelectedSymbol> selectedSymbols)
        {            
            if (selectedSymbols is null || !selectedSymbols.Any())
            {
                _logger.LogWarning("No symbols provided for quote un-subscription.");
                return false;
            }

            var hashSet = new HashSet<SelectedSymbol>(selectedSymbols);
            _subscribedSymbols.RemoveAll(selectedSymbol => hashSet.Contains(selectedSymbol));

            var selectionProjection = selectedSymbols.Select(a => new KeyValuePair<Exchange, long>(a.Exchange, a.Token));
            var ok = await _api.Subscription.QuoteSubscription.UnsubscribeAsync(selectionProjection);
            if (!ok)
            {
                _logger.LogError("Failed to subscribe to quote updates.");
                return ok;
            }

            _logger.LogInformation("Un-Subscribed to quote updates successfully.");
            return ok;
        }          

        private static QuoteSubscriptionRequestAck UpdateQuotes(QuoteSubscriptionUpdates quoteUpdate)
        {
            var newQuoteUpdate = new QuoteSubscriptionRequestAck
            {
                Token = quoteUpdate.Token,
                Exchange = quoteUpdate.Exchange,
            };
            newQuoteUpdate.Update(quoteUpdate);

            var details = GlobalDataSet.Subscriptions.GetOrAdd(quoteUpdate.Token, new SubscriptionDetails());
            return details.QuoteSubscription.AddOrUpdate(quoteUpdate.Exchange, newQuoteUpdate, (key, existingValue) => existingValue.Update(quoteUpdate));
        }

        private static void UpdateQuotes(QuoteSubscriptionRequestAck quoteUpdates)
        {
            var details = GlobalDataSet.Subscriptions.GetOrAdd(quoteUpdates.Token, new SubscriptionDetails());
            details.QuoteSubscription.AddOrUpdate(quoteUpdates.Exchange, quoteUpdates, (_, _) => quoteUpdates);
        }

        private async Task OnQuoteUpdates(QuoteSubscriptionUpdates Object)
        {
            if (Object is not null)
            {
                var update = UpdateQuotes(Object);
                if (_onStrategyEvents is not null)
                    await _onStrategyEvents(new StrategyEvent
                    {
                        EventType = StrategyEngineEventType.Quotes,                        
                        Exchange = update.Exchange,
                        Token = update.Token,
                        TradingSymbol = update.TradingSymbol
                    });
            }
        }

        private Task OnQuoteUpdates(object? _, SubscriptionType subscriptionType, string rawMessage, object? subscriptionObject)
        {
            var msg = string.Format($": Message processed: '{rawMessage}'");

            switch (subscriptionType)
            {
                case SubscriptionType.ConnectAck:
                    _logger.LogInformation("[OnQuoteUpdates-ConnectAck] {msg}", msg);
                    SubscribeQuoteAsync(_subscribedSymbols).GetAwaiter().GetResult();
                    break;
                case SubscriptionType.SubscribeTouchLineAck:
                    _logger.LogInformation("[OnQuoteUpdates-SubscribeQuoteAck] {msg}", msg);
                    UpdateQuotes((QuoteSubscriptionRequestAck)subscriptionObject!);
                    break;
                case SubscriptionType.UnsubscribeTouchLineAck:
                    _logger.LogInformation("[OnQuoteUpdates-UnSubscribeQuoteLineAck] {msg}", msg);
                    UnSubscribeQuoteAsync(_subscribedSymbols).GetAwaiter().GetResult();
                    break;
                case SubscriptionType.SubscribeTouchLineUpdates:
                    _logger.LogInformation("[OnQuoteUpdates-SubscribeQuoteUpdates] {msg}", msg);
                    if (subscriptionObject is QuoteSubscriptionUpdates Object)
                        _queue.WriteAsync(Object).GetAwaiter().GetResult();
                    break;
                default:
                    _logger.LogWarning("[OnQuoteUpdates]: unknown message type '{type}' Msg '{msg}'", subscriptionType, msg);
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
        ~QuoteDetails()
        {
            Dispose(false);
        }
    }
}
