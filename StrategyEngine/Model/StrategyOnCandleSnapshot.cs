using Common.Types;
using FlatTrade.Types;
using FlatTrade.Types.Base;

namespace StrategyEngine.Model
{
    internal record StrategyOnCandleSnapshot(ChartInterval ChartInterval,
                                            string TradingSymbol,
                                            long Token,
                                            Exchange Exchange,
                                            SortedSet<PriceCandle> Candles
                                            );
}
