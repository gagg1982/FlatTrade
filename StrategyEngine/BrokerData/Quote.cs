using FlatTrade;
using FlatTrade.Common.Types.Base;
using FlatTrade.SubscriptionManager;
using FlatTrade.SubscriptionManager.Quote;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StrategyEngine.Model;
using StrategyEngine.Strategies;
using System.Collections.Concurrent;

namespace StrategyEngine.BrokerData
{
    internal class Quote : IAsyncDisposable
    {
        private bool _disposed = false;
        private readonly Api _api;
        private readonly ILogger<Quote> _logger;
        private readonly IConfiguration _config;

        private readonly OnUpdate? _onQuote;
        private readonly ContextAccessor _contextAccessor;

        private readonly Helpers.Queue<QuoteSubscriptionUpdates> _queue;

        private ConcurrentBag<SelectedSymbol> _subscribedSymbols = [];
        public Quote(IConfiguration config, ContextAccessor contextAccessor, Api api, OnUpdate? onUpdate, ILoggerFactory loggerFactory)
        {
            _api = api;
            _config = config;
            _logger = loggerFactory.CreateLogger<Quote>();
            _contextAccessor = contextAccessor;
            _onQuote = onUpdate;
            _queue = new(50000, "QuoteUpdateQueue", OnQuoteUpdates, loggerFactory);
        }

        public async Task<bool> SubscribeQuoteAsync(IEnumerable<SelectedSymbol> selectedSymbols)
        {
            if (selectedSymbols is null || !selectedSymbols.Any())
            {
                _logger.LogWarning("No symbols provided for quote subscription.");
                return false;
            }

            if (_api.Subscription.QuoteSubscription.OnSubscriptionEvents is null)
                _api.Subscription.QuoteSubscription.OnSubscriptionEvents = OnQuoteUpdates;

            _subscribedSymbols = [.. _subscribedSymbols.Union(selectedSymbols)];
            var selectionProjection = _subscribedSymbols.Select(a => new KeyValuePair<Exchange, long>(a.Exchange, a.Token));

            var ok = await _api.Subscription.QuoteSubscription.SubscribeAsync(selectionProjection);
            _logger.LogInformation("Subscribed to quote updates {0}.", ok ? "successfully" : "failed");
            return ok;
        }

        public async Task<bool> UnSubscribeQuoteAsync(IEnumerable<SelectedSymbol> selectedSymbols)
        {
            if (selectedSymbols is null || !selectedSymbols.Any())
            {
                _logger.LogWarning("No symbols provided for quote un-subscription.");
                return false;
            }

            var removeSet = new HashSet<SelectedSymbol>(selectedSymbols);
            _subscribedSymbols = new ConcurrentBag<SelectedSymbol>(_subscribedSymbols.Where(item => !removeSet.Contains(item)));

            var selectionProjection = selectedSymbols.Select(a => new KeyValuePair<Exchange, long>(a.Exchange, a.Token));
            var ok = await _api.Subscription.QuoteSubscription.UnsubscribeAsync(selectionProjection);

            _logger.LogInformation("Un-Subscribed quote updates {0}.", ok ? "successfully" : "failed");
            return ok;
        }

        private async Task UpdateOhlcv(string tradingSymbol, Exchange exchange, long token, long dayVolume, decimal price, DateTime tradeDateTime)
        {
            if (string.IsNullOrEmpty(tradingSymbol) || token == 0 || (dayVolume == 0 && price == decimal.MinValue) || tradeDateTime == DateTime.MinValue)
                return;
            await _contextAccessor.Candle.UpdateCandlesWithQuotesAsync(tradingSymbol, exchange, token, price, dayVolume, tradeDateTime.ToLocalTime());
        }

        private async Task UpdateQuotes(QuoteSubscriptionUpdates quoteUpdates)
        {
            var details = GlobalDataSet.Subscriptions.GetOrAdd(quoteUpdates.Token, new SubscriptionDetails());
            var newQuoteUpdate = new QuoteSubscriptionRequestAck
            {
                Token = quoteUpdates.Token,
                Exchange = quoteUpdates.Exchange,
            };
            newQuoteUpdate.Update(quoteUpdates);
            var quoteAfterUpdate = details.QuoteSubscription.AddOrUpdate(newQuoteUpdate.Exchange, newQuoteUpdate, (_, existing) =>
            {
                lock (existing)
                {
                    existing.Update(quoteUpdates);
                    return existing;
                }
            });

            await UpdateOhlcv(quoteAfterUpdate.TradingSymbol,
                                quoteAfterUpdate.Exchange,
                                quoteAfterUpdate.Token,
                                quoteAfterUpdate.DayVolume,
                                quoteAfterUpdate.LastTradePrice,
                                quoteAfterUpdate.LastTradeDateTime);

            if (_onQuote is not null)
                await _onQuote(quoteAfterUpdate);
        }

        private async Task UpdateQuotes(QuoteSubscriptionRequestAck quoteUpdates)
        {
            var details = GlobalDataSet.Subscriptions.GetOrAdd(quoteUpdates.Token, new SubscriptionDetails());
            var quoteAfterUpdate = details.QuoteSubscription.AddOrUpdate(quoteUpdates.Exchange, quoteUpdates, (_, existing) =>
            {
                lock (existing)
                {
                    existing.Update(quoteUpdates);
                    return quoteUpdates;
                }
            });

            await UpdateOhlcv(quoteAfterUpdate.TradingSymbol,
                                quoteAfterUpdate.Exchange,
                                quoteAfterUpdate.Token,
                                quoteAfterUpdate.DayVolume,
                                quoteAfterUpdate.LastTradePrice,
                                quoteAfterUpdate.LastTradeDateTime);

            if (_onQuote is not null)
                await _onQuote(new StrategyOnQuoteSnapshot
                {
                    Token = quoteAfterUpdate.Token,
                    Exchange = quoteAfterUpdate.Exchange,
                    TradingSymbol = quoteUpdates.TradingSymbol
                });
        }

        private async Task OnQuoteUpdates(QuoteSubscriptionUpdates updates)
        {
            if (updates is not null)
                await UpdateQuotes(updates);
        }

        private async Task OnQuoteUpdates(object? _, SubscriptionType subscriptionType, string rawMessage, object? subscriptionObject)
        {
            var msg = string.Format("Message processed: '{0}'", rawMessage);

            switch (subscriptionType)
            {
                case SubscriptionType.ConnectAck:
                    _logger.LogInformation("[OnQuoteUpdates-ConnectAck] {msg}", msg);
                    await SubscribeQuoteAsync(_subscribedSymbols);
                    break;
                case SubscriptionType.SubscribeQuoteAck:
                    _logger.LogInformation("[OnQuoteUpdates-SubscribeQuoteAck] {msg}", msg);
                    await UpdateQuotes((QuoteSubscriptionRequestAck)subscriptionObject!);
                    break;
                case SubscriptionType.UnsubscribeQuoteAck:
                    _logger.LogInformation("[OnQuoteUpdates-UnSubscribeQuoteLineAck] {msg}", msg);
                    await UnSubscribeQuoteAsync(_subscribedSymbols);
                    break;
                case SubscriptionType.SubscribeQuoteUpdates:
                    _logger.LogDebug("[OnQuoteUpdates-SubscribeQuoteUpdates] {msg}", msg);
                    if (subscriptionObject is QuoteSubscriptionUpdates Object)
                        await _queue.WriteAsync(Object);
                    break;
                default:
                    _logger.LogWarning("[OnQuoteUpdates]: unknown message type '{type}' Msg '{msg}'", subscriptionType, msg);
                    break;
            }
        }

        public void Dispose()
        {
            DisposeAsync().AsTask().GetAwaiter().GetResult(); // Safe synchronous fallback
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            if (!_disposed)
            {
                // Dispose managed resources here
                await _queue.WriteComplete().ConfigureAwait(false);
                _queue.Dispose();

                _disposed = true;
            }

            GC.SuppressFinalize(this);
        }

        // Finalizer (only if you have unmanaged resources)
        ~Quote()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources (no async here)
                    _queue.Dispose();
                }

                // Free unmanaged resources here if any

                _disposed = true;
            }
        }
    }
}
