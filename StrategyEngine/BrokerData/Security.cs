using FlatTrade;
using FlatTrade.Common.Types.Base;
using FlatTrade.ScripManager;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StrategyEngine.Model;
using StrategyEngine.Strategies;
using System.Collections.Concurrent;

namespace StrategyEngine.BrokerData
{
    internal class Security
    {
        private readonly IConfiguration _config;
        private readonly Api _api;
        private readonly ILogger _logger;
        private readonly ContextAccessor _contextAccessor;

        public static event OnUpdate? OnSecurities;

        public Security(IConfiguration config, ContextAccessor contextAccessor, Api api, ILoggerFactory loggerFactory)
        {
            _api = api;
            _logger = loggerFactory.CreateLogger<Security>();
            _config = config;
            _contextAccessor = contextAccessor;
        }

        private static async Task<IEnumerable<LinkedScrip>> GetLinkedScripDetailsFromServerAsync(Api api, ILogger logger, IEnumerable<KeyValuePair<Exchange, long>> exchangeToken)
        {
            List<Task<(LinkedScripsResponse?, string)>> linkedScripTasks = [];

            foreach (var exchToken in exchangeToken)
                linkedScripTasks.Add(api.Scrips.GetLinkedScripsAsync(exchToken.Key, exchToken.Value));

            var linkedScripResults = await Task.WhenAll(linkedScripTasks);

            ConcurrentDictionary<KeyValuePair<Exchange, long>, LinkedScrip> linkedScrips = [];
            foreach (var (linkedScrip, mesg) in linkedScripResults)
            {
                if (linkedScrip is null)
                {
                    if (mesg != Constants.StatusOk)
                        logger.LogError("Error while fetching linked scrips : {mesg}", mesg);
                    else
                        logger.LogInformation("No linked scrips found : {mesg}", mesg);
                }
                else
                {
                    foreach (var equity in linkedScrip.LinkedEquities)
                    {
                        var key = new KeyValuePair<Exchange, long>(equity.Exchange, equity.Token);
                        var value = new LinkedScrip
                        {
                            TradingSymbol = equity.TradingSymbol,
                            Exchange = equity.Exchange,
                            Token = equity.Token,
                            IsFutureAllowed = linkedScrip.LinkedFutures.Count != 0,
                            IsOptionAllowed = linkedScrip.LinkedOptions.Count != 0,
                        };
                        linkedScrips.AddOrUpdate(key, value, (_, _) => value);
                    }
                }
            }
            return linkedScrips.Values;
        }

        private async Task<IEnumerable<ScripInfo>> GetSecurityInfoFromServerAsync(IEnumerable<SelectedSymbol> selection)
        {
            var selectionProjection = selection.Select(a => new KeyValuePair<Exchange, long>(a.Exchange, a.Token));
            var quoteResponse = await GetQuoteDetailsFromServerAsync(_api, _logger, selectionProjection);
            var linkedScrips = await GetLinkedScripDetailsFromServerAsync(_api, _logger, selectionProjection);
            return ScripInfo.ConvertFrom(quoteResponse ?? [], linkedScrips ?? []);
        }

        private static async Task<IEnumerable<QuotesResponse>> GetQuoteDetailsFromServerAsync(Api api, ILogger logger, IEnumerable<KeyValuePair<Exchange, long>> exchangeToken)
        {
            List<Task<(QuotesResponse?, string)>> quoteTasks = [];
            List<QuotesResponse> quotes = [];

            foreach (var exchToken in exchangeToken)
                quoteTasks.Add(api.Scrips.GetQuotesAsync(exchToken.Key, exchToken.Value));

            var quotesResults = await Task.WhenAll(quoteTasks);

            foreach (var (quote, mesg) in quotesResults)
            {
                if (quote is null)
                {
                    if (mesg != Constants.StatusOk)
                        logger.LogError("Error while fetching quote : {mesg}", mesg);
                    else
                        logger.LogInformation("No quotes found : {mesg}", mesg);
                }
                else
                {
                    quotes.Add(quote);
                }
            }
            return quotes;
        }
        
        public async Task UpdateSecurityInfo(IEnumerable<SelectedSymbol> selection)
        {
            var securityResponse = await GetSecurityInfoFromServerAsync(selection);

            if (securityResponse.Count() != selection.Count())
            {
                _logger.LogInformation("Unable to get the details of all securitites. Missing securities details are as follows:.");
            }

            foreach (var scrip in securityResponse)
            {
                var details = GlobalDataSet.Data.GetOrAdd(scrip.TradingSymbol, _ => new());
                var updatedScrip = details!.SecurityInfo.AddOrUpdate(scrip.Exchange, scrip, (_, _) => { return scrip; });

                if (OnSecurities is not null)
                    await OnSecurities.Invoke(new StrategyOnScripSnapshot(updatedScrip));
            }
        }
    }
}
