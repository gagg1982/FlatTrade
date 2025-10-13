using FlatTrade.Common.Types.Base;
using FlatTrade.HoldingsManager;
using FlatTrade.TradeManager;

using System.Collections.Concurrent;

namespace StrategyEngine.Model
{
    public class Details
    {
        public Exchange Exchange { get; } 
        // key noren order number        
        public ConcurrentDictionary<long, OrderInfo> OpenOrders { get; } = [];
        public ConcurrentDictionary<long, OrderInfo> ClosedOrders { get; } = [];

        public ConcurrentDictionary<ProductType, PositionBookResponse> OpenPositions { get; } = [];
        public ConcurrentDictionary<ProductType, PositionBookResponse> ClosedPositions { get; } = [];
        public ConcurrentDictionary<Exchange, TradeBookResponse> TradeInfo { get; } = [];
        public ConcurrentDictionary<Exchange, HoldingsResponse> HoldingInfo { get; } = [];
        public ConcurrentDictionary<Exchange, ScripInfo> SecurityInfo { get; } = [];
        public ConcurrentDictionary<Exchange, ConcurrentDictionary<ChartInterval, SortedSet<PriceCandle>>> PriceCandleInfo { get; } = [];        
    }

    public static class GlobalDataSet
    {
        //key trading symbol
        public static ConcurrentDictionary<string, Details> Data { get; set; }  = [];
    }


}
