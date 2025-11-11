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
    internal sealed class Quote : IAsyncDisposable
    {
        private bool _disposed = false;
        private readonly Api _api;
        private readonly ILogger<Quote> _logger;
        private readonly IConfiguration _config;

        public static event OnUpdate? OnQuote;
        private readonly ContextAccessor _contextAccessor;

        private readonly Helpers.Queue<QuoteSubscriptionUpdates> _queue;

        private ConcurrentBag<SelectedSymbol> _subscribedSymbols = [];
        public Quote(IConfiguration config, ContextAccessor contextAccessor, Api api, ILoggerFactory loggerFactory)
        {
            _api = api;
            _config = config;
            _logger = loggerFactory.CreateLogger<Quote>();
            _contextAccessor = contextAccessor;
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
            _logger.LogDebug("Subscribed to quote updates {0}.", ok ? "successfully" : "failed");
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

        private async Task UpdateOhlcv(string tradingSymbol, Exchange exchange, long token, long currentVolume, long previousVolume, decimal latestPrice, decimal previousPrice, DateTime tradeDateTime)
        {
            if(currentVolume == 0 && latestPrice == decimal.MinValue)
            {
                _logger.LogWarning("{0}:UpdateOhlcv: None of them is available. CurrentVolume: {1}, PriceToUpdate: {2}. Skipping update...", GetType().Name, currentVolume, latestPrice);
                return;
            }

            if(
                token == 0 ||
                previousVolume == 0 ||
                previousPrice == decimal.MinValue ||
                string.IsNullOrEmpty(tradingSymbol) ||              
                tradeDateTime == DateTime.MinValue
              )
            {
                _logger.LogWarning("{0}:UpdateOhlcv: One of the mandatory field missing. Skipping update...", GetType().Name);
                return;
            }
                
            await _contextAccessor.Candle.UpdateCandlesWithQuotesAsync(tradingSymbol, exchange, token, latestPrice, previousPrice, currentVolume == 0 ? previousVolume: currentVolume, tradeDateTime.ToLocalTime());
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

            long prevVolume = 0;
            decimal prevPrice = decimal.MinValue;
            details.QuoteSubscription.TryGetValue(quoteUpdates.Exchange, out QuoteSubscriptionRequestAck? beforeUpdate);
            if (beforeUpdate is not null)
            {
                prevVolume = beforeUpdate.DayVolume;
                prevPrice = beforeUpdate.LastTradePrice;
            }

            var quoteAfterUpdate = details.QuoteSubscription.AddOrUpdate(newQuoteUpdate.Exchange, newQuoteUpdate, (_, existing) =>
            {
                lock (existing)
                {
                    existing.Update(quoteUpdates);
                    return existing;
                }
            });

            await UpdateOhlcv(  quoteAfterUpdate.TradingSymbol,
                                quoteAfterUpdate.Exchange,
                                quoteAfterUpdate.Token,                                
                                quoteUpdates.DayVolume, //current volume
                                prevVolume,
                                quoteUpdates.LastTradePrice, //latest
                                prevPrice,
                                quoteAfterUpdate.LastTradeDateTime);

            if (OnQuote is not null)
                await OnQuote.Invoke(quoteAfterUpdate);
        }

        private async Task UpdateQuotes(QuoteSubscriptionRequestAck quoteUpdates)
        {
            var details = GlobalDataSet.Subscriptions.GetOrAdd(quoteUpdates.Token, new SubscriptionDetails());
            long prevVolume = 0;
            decimal prevPrice = decimal.MinValue;
            details.QuoteSubscription.TryGetValue(quoteUpdates.Exchange, out QuoteSubscriptionRequestAck? beforeUpdate);
            if (beforeUpdate is not null)
            {
                prevVolume = beforeUpdate.DayVolume;
                prevPrice = beforeUpdate.LastTradePrice;
            }

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
                                quoteUpdates.DayVolume,
                                prevVolume,
                                quoteUpdates.LastTradePrice, //latest
                                prevPrice,
                                quoteUpdates.LastTradeDateTime);

            if (OnQuote is not null)
                await OnQuote.Invoke(new StrategyOnQuoteSnapshot(quoteUpdates.TradingSymbol, quoteAfterUpdate.Token, quoteAfterUpdate.Exchange));
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
            OnQuote = null;
            _subscribedSymbols.Clear();
            await _queue.DisposeAsync();

            _logger.LogInformation("{0}: Disposed gracefully", GetType().Name);
            // Dispose other sync-only resources here (e.g., timers, files)
        }
    }
}
