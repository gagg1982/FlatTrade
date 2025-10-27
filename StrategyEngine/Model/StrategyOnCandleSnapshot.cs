using FlatTrade.Common.Types;
using FlatTrade.Common.Types.Base;

namespace StrategyEngine.Model
{
    internal class StrategyOnCandleSnapshot
    {
        internal required ChartInterval ChartInterval { get; set; }
        internal required string TradingSymbol { get; set; } = string.Empty;
        internal required long Token { get; set; }
        internal required Exchange Exchange { get; set; }
        internal SortedSet<PriceCandle> Candles { get; set; } = [];
    }
}
