using FlatTrade.Common.Types.Base;

namespace StrategyEngine.Model
{
    internal class StrategyOnOrderSnapshot
    {
        internal required string TradingSymbol { get; set; } = string.Empty;
        internal required Exchange Exchange { get; set; }
        internal required long NorenOrderNumber { get; set; }
        internal required OrderStatus OrderStatus { get; set; }
    }
}
