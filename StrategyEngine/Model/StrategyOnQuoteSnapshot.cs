using FlatTrade.Common.Types.Base;

namespace StrategyEngine.Model
{
    internal record StrategyOnQuoteSnapshot(string TradingSymbol, long Token, Exchange Exchange);
}
