using FlatTrade.Types.Base;

namespace StrategyEngine.Model
{
    internal record StrategyOnPositionSnapshot(string TradingSymbol, long Token, Exchange Exchange);
}
