using FlatTrade.Common.Types.Base;
using FlatTrade.HoldingsManager;
using FlatTrade.SubscriptionManager.Quote;
using FlatTrade.SubscriptionManager.TouchLine;
using FlatTrade.TradeManager;
using System.Collections.Concurrent;

namespace StrategyEngine.Model
{
    public class SubscriptionDetails
    {
        public ConcurrentDictionary<Exchange, TouchLineSubscriptionRequestAck> TouchLineSubscription { get; set; } = [];
        public ConcurrentDictionary<Exchange, QuoteSubscriptionRequestAck> QuoteSubscription { get; set; } = [];
    }
    public class Details
    {
        

        // key noren order number        
        public ConcurrentDictionary<long, OrderInfo> OpenOrders { get; } = [];
        public ConcurrentDictionary<long, OrderInfo> ClosedOrders { get; } = [];

        public ConcurrentDictionary<ProductType, PositionBookResponse> OpenPositions { get; } = [];
        public ConcurrentDictionary<ProductType, PositionBookResponse> ClosedPositions { get; } = [];
        public ConcurrentDictionary<Exchange, ConcurrentBag<TradeBookResponse>> TradeInfo { get; } = [];
        public ConcurrentDictionary<Exchange, HoldingsResponse> HoldingInfo { get; } = [];
        public ConcurrentDictionary<Exchange, ScripInfo> SecurityInfo { get; } = [];
        public ConcurrentDictionary<Exchange, ConcurrentDictionary<ChartInterval, SortedSet<PriceCandle>>> PriceCandleInfo { get; } = [];        
    }

    public static class GlobalDataSet
    {
        //key trading symbol
        public static ConcurrentDictionary<string, Details> Data { get; set; }  = [];
        public static ConcurrentDictionary<long, SubscriptionDetails> Subscriptions { get; set; } = [];
    }


}
