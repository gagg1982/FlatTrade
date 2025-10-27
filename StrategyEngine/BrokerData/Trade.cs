using FlatTrade;
using FlatTrade.Common.Types.Base;
using FlatTrade.TradeManager;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StrategyEngine.Model;
using StrategyEngine.Strategy;

namespace StrategyEngine.BrokerData
{
    internal class Trade
    {
        private readonly IConfiguration _config;
        private readonly Api _api;
        private readonly ILogger _logger;
        private readonly ContextAccessor _contextAccessor;

        private event OnUpdate? _onTrades;

        public Trade(IConfiguration config, ContextAccessor contextAccessor, Api api, OnUpdate? onUpdate, ILoggerFactory loggerFactory)
        {
            _api = api;
            _logger = loggerFactory.CreateLogger<Trade>();
            _config = config;
            _contextAccessor = contextAccessor;
            _onTrades += onUpdate;
        }

        private async Task<IEnumerable<TradeBookResponse>> GetTradeDetailsFromServerAsync()
        {

            var (tradeBook, mesg) = await _api.Trade.GetTradeBookAsync();
            if (tradeBook is null || !tradeBook.Any())
            {
                if (mesg != Constants.StatusOk)
                    _logger.LogError("Error while fetching trade book : {mesg}", mesg);
                else
                    _logger.LogInformation("No trades found: {mesg}", mesg);
            }
            else
            {
                _logger.LogInformation("Fetched {tradeBookCount} trades from the trade book.", tradeBook.Count());
            }
            return tradeBook ?? [];
        }

        public async Task UpdateTradeDetails(Exchange? exchange = null, string? tradingSymbol = null, long? norenOrderNumber = null)
        {
            var trades = await GetTradeDetailsFromServerAsync();
            if (!trades.Any())
            {
                _logger.LogInformation("No trades found to update.");
                return;
            }

            foreach (var trade in trades!)
            {
                var details = GlobalDataSet.Data.GetOrAdd(trade.TradingSymbol, _ => new());
                details!.TradeInfo.AddOrUpdate(trade.Exchange, [trade], (_, existingValue) =>
                {
                    lock (existingValue)
                    {
                        existingValue.Add(trade);
                        return existingValue;
                    }
                });   //always replace            


                if (_onTrades is not null &&
                    (tradingSymbol is null || tradingSymbol == trade.TradingSymbol) &&
                    (exchange is null || exchange == trade.Exchange) &&
                    (norenOrderNumber is null || norenOrderNumber == trade.NorenOrderNumber))
                    await _onTrades(new StrategyOnTradeSnapshot
                    {
                        Exchange = trade.Exchange,
                        TradingSymbol = trade.TradingSymbol,
                        Fillid = trade.FillId,
                        NorenOrderNumber = trade.NorenOrderNumber,
                        ExchangeOrderNumber = trade.ExchangeOrderNumber
                    });
            }
        }
    }
}
