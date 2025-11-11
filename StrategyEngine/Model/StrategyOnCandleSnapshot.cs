using FlatTrade.Common.Types;
using FlatTrade.Common.Types.Base;

namespace StrategyEngine.Model
{
    internal record StrategyOnCandleSnapshot(ChartInterval ChartInterval,
                                            string TradingSymbol,
                                            long Token,
                                            Exchange Exchange,
                                            SortedSet<PriceCandle> Candles
                                            );
}
