using FlatTrade;
using FlatTrade.Common.Types.Base;
using FlatTrade.HoldingsManager;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StrategyEngine.Model;
using StrategyEngine.Strategies;

namespace StrategyEngine.BrokerData
{
    internal class Holding
    {
        private readonly IConfiguration _config;
        private readonly Api _api;
        private readonly ILogger _logger;
        private readonly ContextAccessor _contextAccessor;
        private event OnUpdate? _onHoldings;

        public Holding(IConfiguration config, ContextAccessor contextAccessor, Api api, OnUpdate? onUpdate, ILoggerFactory loggerFactory)
        {
            _api = api;
            _logger = loggerFactory.CreateLogger<Holding>();
            _config = config;
            _contextAccessor = contextAccessor;
            _onHoldings += onUpdate;
        }


        private async Task<IEnumerable<HoldingsResponse>> GetHoldingDetailsFromServerAsync()
        {

            var (holdings, mesg) = await _api.Holdings.GetHoldingsAsync();
            if (holdings is null || !holdings.Any())
            {
                if (mesg != Constants.StatusOk)
                    _logger.LogError("Error while fetching holdings details : {mesg}", mesg);
                else
                    _logger.LogInformation("No holdings found: {mesg}", mesg);
            }
            else
            {
                _logger.LogInformation("Fetched {holdingCount} holdings.", holdings.Count());
            }
            return holdings ?? [];
        }

        public async Task UpdateHoldingDetails()
        {
            var holdings = await GetHoldingDetailsFromServerAsync();
            if (!holdings.Any())
            {
                _logger.LogInformation("No holdings found to update.");
                return;
            }

            foreach (var holding in holdings!)
            {
                foreach (var exch in holding.ExchangeSymbolResponse)
                {
                    var details = GlobalDataSet.Data.GetOrAdd(exch.TradingSymbol, _ => new());
                    details!.HoldingInfo.AddOrUpdate(exch.Exchange, holding, (key, existingValue) => holding);
                }

                if (_onHoldings is not null)
                    await _onHoldings(new StrategyOnHoldingSnapshot(holding));
            }
        }
    }
}
