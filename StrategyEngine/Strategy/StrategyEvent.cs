using FlatTrade.Common.Types.Base;

namespace StrategyEngine.Strategy
{
    internal class StrategyEvent
    {
        internal required string TradingSymbol { get; set; } = string.Empty;
        internal required Exchange Exchange {  get; set; }
        internal required long Token { get; set; }
    }
}
