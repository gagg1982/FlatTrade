using FlatTrade.Common.Types.Base;

namespace StrategyEngine.Model
{
    internal class StrategyOnTradeSnapshot
    {
        internal required string TradingSymbol { get; set; } = string.Empty;
        internal required long Fillid { get; set; }
        internal required long NorenOrderNumber { get; set; }
        internal required string ExchangeOrderNumber { get; set; } = string.Empty;
        internal required Exchange Exchange { get; set; }
    }
}
