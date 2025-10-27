using FlatTrade.Common.Types.Base;

namespace StrategyEngine.Model
{
    internal class StrategyOnTouchLineSnapshot
    {
        internal required string TradingSymbol { get; set; } = string.Empty;
        internal required long Token { get; set; }
        internal required Exchange Exchange { get; set; }
    }
}
