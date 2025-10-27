using FlatTrade;
using FlatTrade.Common.Types.Base;
using FlatTrade.TradeManager;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StrategyEngine.Model;
using StrategyEngine.Strategy;

namespace StrategyEngine.BrokerData
{
    internal class Position
    {
        private readonly IConfiguration _config;
        private readonly Api _api;
        private readonly ILogger _logger;
        private readonly ContextAccessor _contextAccessor;

        private event OnUpdate? _onPositons;

        public Position(IConfiguration config, ContextAccessor contextAccessor, Api api, OnUpdate? onUpdate, ILoggerFactory loggerFactory)
        {
            _api = api;
            _logger = loggerFactory.CreateLogger<Position>();
            _config = config;
            _contextAccessor = contextAccessor;
            _onPositons += onUpdate;
        }

        private async Task<IEnumerable<PositionBookResponse>> GetPositionsFromServerAsync()
        {

            var (positionBook, mesg) = await _api.Trade.GetPositionBookAsync();
            if (positionBook is null || !positionBook.Any())
            {
                if (mesg != Constants.StatusOk)
                    _logger.LogError("Error while fetching positions : {mesg}", mesg);
                else
                    _logger.LogInformation("No positions found: {mesg}", mesg);
            }
            else
            {
                _logger.LogInformation("Fetched {positionBookCount} position from position book.", positionBook.Count());
            }
            return positionBook ?? [];
        }

        public async Task UpdatePositions(Exchange? exchange = null, string? tradingSymbol = null)
        {
            var positions = await GetPositionsFromServerAsync();
            if (positions is null || !positions.Any())
            {
                _logger.LogInformation("No positions found to update.");
                return;
            }

            foreach (var position in positions!)
            {
                var details = GlobalDataSet.Data.GetOrAdd(position.TradingSymbol, _ => new());
                if (position.NetPositionQuantity == 0)
                {
                    details!.OpenPositions.Remove(position.ProductType, out PositionBookResponse? _);
                    details!.ClosedPositions.AddOrUpdate(position.ProductType, position, (_, _) => position); //always update to latest
                }
                else
                {
                    details!.ClosedPositions.Remove(position.ProductType, out PositionBookResponse? _);
                    details!.OpenPositions.AddOrUpdate(position.ProductType, position, (_, _) => position); //always update to latest
                }

                if (_onPositons is not null &&
                    (exchange is null || position.Exchange == exchange) &&
                    (tradingSymbol is null || position.TradingSymbol == tradingSymbol))
                    await _onPositons(new StrategyOnPositionSnapshot
                    {
                        Exchange = position.Exchange,
                        Token = position.Token,
                        TradingSymbol = position.TradingSymbol
                    });
            }
        }
    }
}
