using FlatTrade;
using FlatTrade.Common.Types.Base;
using FlatTrade.SubscriptionManager;
using FlatTrade.SubscriptionManager.Quote;
using Microsoft.Extensions.Logging;
using StrategyEngine.Model;
using StrategyEngine.Strategy;

namespace StrategyEngine
{
    internal class QuoteDetails : IDisposable
    {
        private bool _disposed = false;
        private readonly Api _api;
        private readonly ILogger<QuoteDetails> _logger;
        private readonly OnUpdate? OnQuote;
        private readonly DirectFromServer _directFromServer;

        private readonly Helpers.Queue<QuoteSubscriptionUpdates> _queue;

        private List<SelectedSymbol> _subscribedSymbols = [];
        public QuoteDetails(Api api, DirectFromServer directFromServer, OnUpdate? onQuote, ILoggerFactory loggerFactory)
        {
            _api = api;
            _logger = loggerFactory.CreateLogger<QuoteDetails>();
            _directFromServer = directFromServer;
            OnQuote = onQuote;
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

            if (_api.Subscription.QuoteSubscription.OnSubscriptionEvents is null)
                _api.Subscription.QuoteSubscription.OnSubscriptionEvents = OnQuoteUpdates;
            
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

        private async Task UpdateOhlcv(string tradingSymbol, Exchange exchange, long token, long quantity, decimal price, DateTime tradeDateTime)
        {
            if (string.IsNullOrEmpty(tradingSymbol) || token == 0 ||  ( quantity == 0 &&  price == decimal.MinValue) || tradeDateTime == DateTime.MinValue)
                return;

            // bad hack of -1 seconds as Flattrade captures the price exactly at 9:20:00.000 into 9:19:00 candle instead of 9:20:00 candle.
            // This is impacting OHLCV. Other platforms consider it the part of 9:20 candle instead of 9:19.
            await _directFromServer.UpdateCandles(tradingSymbol, exchange, token, price, quantity, tradeDateTime.ToLocalTime().AddSeconds(-1)); 
        }

        private async Task<QuoteSubscriptionRequestAck> UpdateQuotes(QuoteSubscriptionUpdates quoteUpdate)
        {
            var newQuoteUpdate = new QuoteSubscriptionRequestAck
            {
                Token = quoteUpdate.Token,
                Exchange = quoteUpdate.Exchange,
            };
            newQuoteUpdate.Update(quoteUpdate);           

            var details = GlobalDataSet.Subscriptions.GetOrAdd(quoteUpdate.Token, new SubscriptionDetails());            
            var quoteRequestAck = details.QuoteSubscription.AddOrUpdate(quoteUpdate.Exchange, newQuoteUpdate, (key, existingValue) => { return existingValue.Update(quoteUpdate);});
            
            await UpdateOhlcv(quoteRequestAck.TradingSymbol,
                                    quoteUpdate.Exchange,
                                    quoteUpdate.Token,
                                    quoteUpdate.LastTradeQuantity,
                                    quoteUpdate.LastTradePrice,
                                    quoteUpdate.LastTradeDateTime);

            return quoteRequestAck;
        }

        private async Task UpdateQuotes(QuoteSubscriptionRequestAck quoteUpdates)
        {
            var details = GlobalDataSet.Subscriptions.GetOrAdd(quoteUpdates.Token, new SubscriptionDetails());
            details.QuoteSubscription.AddOrUpdate(quoteUpdates.Exchange, quoteUpdates, (_, _) => { return quoteUpdates; });

            await UpdateOhlcv(quoteUpdates.TradingSymbol,
                            quoteUpdates.Exchange,
                            quoteUpdates.Token,                                                       
                            quoteUpdates.LastTradeQuantity,
                            quoteUpdates.LastTradePrice,
                            quoteUpdates.LastTradeDateTime);
        }

        private async Task OnQuoteUpdates(QuoteSubscriptionUpdates Object)
        {
            if (Object is not null)
            {
                var update = await UpdateQuotes(Object);
                if (OnQuote is not null)
                    await OnQuote(new StrategyEvent
                    {
                        Exchange = update.Exchange,
                        Token = update.Token,
                        TradingSymbol = update.TradingSymbol
                    });
            }
        }

        private Task OnQuoteUpdates(object? _, SubscriptionType subscriptionType, string rawMessage, object? subscriptionObject)
        {
            var msg = string.Format("Message processed: '{0}'", rawMessage);

            switch (subscriptionType)
            {
                case SubscriptionType.ConnectAck:
                    _logger.LogInformation("[OnQuoteUpdates-ConnectAck] {msg}", msg);
                    SubscribeQuoteAsync(_subscribedSymbols).GetAwaiter().GetResult();
                    break;
                case SubscriptionType.SubscribeQuoteAck:
                    _logger.LogInformation("[OnQuoteUpdates-SubscribeQuoteAck] {msg}", msg);
                    UpdateQuotes((QuoteSubscriptionRequestAck)subscriptionObject!).GetAwaiter().GetResult();
                    break;
                case SubscriptionType.UnsubscribeQuoteAck:
                    _logger.LogInformation("[OnQuoteUpdates-UnSubscribeQuoteLineAck] {msg}", msg);
                    UnSubscribeQuoteAsync(_subscribedSymbols).GetAwaiter().GetResult();
                    break;
                case SubscriptionType.SubscribeQuoteUpdates:
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
