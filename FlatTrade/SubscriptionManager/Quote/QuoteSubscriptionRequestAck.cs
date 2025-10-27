using FlatTrade.Common.JsonConvertors;
using FlatTrade.Common.Types.Base;
using FlatTrade.ScripManager;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FlatTrade.SubscriptionManager.Quote
{
    public class QuoteSubscriptionRequestAck : BaseSubscriptionRequest
    {

        [JsonProperty("e")]
        public Exchange Exchange { get; set; }

        [JsonProperty("tk")]
        public long Token { get; set; } = 0;

        [JsonProperty("ts")]
        public string TradingSymbol { get; set; } = string.Empty;

        [JsonProperty("pp")]
        public int PricePrecision { get; set; } = 0;

        [JsonProperty("ls")]
        public decimal LotSize { get; set; } = decimal.MinValue;

        [JsonProperty("ti")]
        public decimal TickSize { get; set; } = decimal.MinValue;

        [JsonProperty("lp")]
        public decimal LastTradePrice { get; set; } = decimal.MinValue;

        [JsonProperty("pc")]
        public decimal LastTradePricePercentageChange { get; set; } = decimal.MinValue;

        [JsonProperty("ft")]
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime LastTradeDateTime { get; set; } = DateTime.MinValue;

        [JsonProperty("c")]
        public decimal DayClosePrice { get; set; } = decimal.MinValue;

        [JsonProperty("o")]
        public decimal DayOpenPrice { get; set; } = decimal.MinValue;

        [JsonProperty("h")]
        public decimal DayHighPrice { get; set; } = decimal.MinValue;

        [JsonProperty("l")]
        public decimal DayLowPrice { get; set; } = decimal.MaxValue;

        [JsonProperty("ap")]
        public decimal AverageTradePrice { get; set; } = decimal.MinValue;

        [JsonProperty("v")]
        public long DayVolume { get; set; } = 0;

        [JsonProperty("ltq")]
        public long LastTradeQuantity { get; set; } = 0;

        [JsonProperty("ltt")]
        public string LastTradeTime { get; set; } = string.Empty;

        [JsonProperty("tsq")]
        public decimal TotalSellQuantity { get; set; } = 0;

        [JsonProperty("tbq")]
        public decimal TotalBuyQuantity { get; set; } = 0;

        [JsonConverter(typeof(QuotesResponseJsonConvertor))]
        public List<MarketDepthLevel> BestBids { get; set; } = [];

        [JsonConverter(typeof(QuotesResponseJsonConvertor))]
        public List<MarketDepthLevel> BestAsks { get; set; } = [];

        [JsonProperty("uc")]
        public decimal UpperCircuitLimit { get; set; } = decimal.MinValue;

        [JsonProperty("lc")]
        public decimal LowerCircuitLimit { get; set; } = decimal.MaxValue;

        [JsonProperty("52h")]
        public decimal Wk52High { get; set; } = decimal.MinValue;

        [JsonProperty("52l")]
        public decimal Wk52Low { get; set; } = decimal.MaxValue;

        [JsonProperty("52hd")]
        [JsonConverter(typeof(DateOnlyAsStringConverterDdMonYyyy))]
        public DateOnly Wk52HighDate { get; set; } = DateOnly.MinValue;

        [JsonProperty("52ld")]
        [JsonConverter(typeof(DateOnlyAsStringConverterDdMonYyyy))]
        public DateOnly Wk52LowDate { get; set; } = DateOnly.MinValue;

        [JsonProperty("toi")]
        public long IntervalIoChange { get; set; } = 0;

        public bool IsValidOhlcv()
        {
            return DayClosePrice != decimal.MinValue &&  DayOpenPrice != decimal.MinValue &&
                   DayHighPrice != decimal.MinValue && DayLowPrice != decimal.MaxValue;
        }
        public QuoteSubscriptionRequestAck() { }
        public QuoteSubscriptionRequestAck Update(QuoteSubscriptionRequestAck val)
        {
            if (val is null)
                return this;

            lock(this)
            { 
                Exchange = val.Exchange;
                Token = val.Token == 0 ? Token : val.Token;
                LastTradeDateTime = val.LastTradeDateTime == DateTime.MinValue ? LastTradeDateTime : val.LastTradeDateTime;
                DayVolume = val.DayVolume == 0 ? DayVolume : val.DayVolume;
                LastTradeQuantity = val.LastTradeQuantity == 0 ? LastTradeQuantity : val.LastTradeQuantity;
                LastTradeTime = val.LastTradeTime == string.Empty ? LastTradeTime : val.LastTradeTime;
                TotalSellQuantity = val.TotalSellQuantity == 0 ? TotalSellQuantity : val.TotalSellQuantity;
                TotalBuyQuantity = val.TotalBuyQuantity == 0 ? TotalBuyQuantity : val.TotalBuyQuantity;
                LastTradePrice = val.LastTradePrice == decimal.MinValue ? LastTradePrice : val.LastTradePrice;
                PricePrecision = val.PricePrecision == 0 ? PricePrecision : val.PricePrecision;
                AverageTradePrice = val.AverageTradePrice == decimal.MinValue ? AverageTradePrice : val.AverageTradePrice;
                IntervalIoChange = val.IntervalIoChange == 0 ? IntervalIoChange : val.IntervalIoChange;
                Wk52LowDate = val.Wk52LowDate > Wk52LowDate ? val.Wk52LowDate : Wk52LowDate;
                Wk52HighDate = val.Wk52HighDate > Wk52HighDate ? val.Wk52HighDate : Wk52HighDate;
                Wk52Low = Math.Min(val.Wk52Low, Wk52Low);
                Wk52High = Math.Max(val.Wk52High, Wk52High);
                LowerCircuitLimit = val.LowerCircuitLimit == decimal.MaxValue ? LowerCircuitLimit : val.LowerCircuitLimit;
                UpperCircuitLimit = val.UpperCircuitLimit == decimal.MinValue ? UpperCircuitLimit : val.UpperCircuitLimit;
                TotalBuyQuantity = val.TotalBuyQuantity == 0 ? TotalBuyQuantity : val.TotalBuyQuantity;
                TotalSellQuantity = val.TotalSellQuantity == 0 ? TotalSellQuantity : val.TotalSellQuantity;
                DayLowPrice = Math.Min(val.DayLowPrice, DayLowPrice);
                DayHighPrice = Math.Max(val.DayHighPrice, DayHighPrice);
                DayClosePrice = val.DayClosePrice == decimal.MinValue ? DayClosePrice : val.DayClosePrice;
                DayOpenPrice = val.DayOpenPrice == decimal.MinValue ? DayOpenPrice : val.DayOpenPrice;
                LastTradePricePercentageChange = val.LastTradePricePercentageChange == decimal.MinValue ? LastTradePricePercentageChange : val.LastTradePricePercentageChange;
                TickSize = val.TickSize == decimal.MinValue ? TickSize : val.TickSize;
                LotSize = val.LotSize == decimal.MinValue ? LotSize : val.LotSize;
                TradingSymbol = val.TradingSymbol == string.Empty ? TradingSymbol : val.TradingSymbol;
                UpdateMarketDepth(val.BestBids, val.BestAsks);
            }
            return this;
        }
        public QuoteSubscriptionRequestAck Update(QuoteSubscriptionUpdates val)
        {
            if (val is null)
                return this;

            lock (this)
            {
                Exchange = val.Exchange;
                Token = val.Token == 0 ? Token : val.Token;
                LastTradeDateTime = val.LastTradeDateTime == DateTime.MinValue ? LastTradeDateTime: val.LastTradeDateTime;
                DayVolume = val.DayVolume == 0 ? DayVolume: val.DayVolume;
                LastTradeQuantity = val.LastTradeQuantity == 0 ? LastTradeQuantity : val.LastTradeQuantity;
                LastTradeTime = val.LastTradeTime == string.Empty ? LastTradeTime : val.LastTradeTime;
                TotalSellQuantity = val.TotalSellQuantity == 0 ? TotalSellQuantity : val.TotalSellQuantity;
                TotalBuyQuantity = val.TotalBuyQuantity == 0 ? TotalBuyQuantity : val.TotalBuyQuantity;
                LastTradePrice = val.LastTradePrice == decimal.MinValue ? LastTradePrice : val.LastTradePrice;
                UpdateMarketDepth(val.BestBids, val.BestAsks);
            }
            return this;
        }

        private void UpdateMarketDepth(List<MarketDepthLevel> bestBids, List<MarketDepthLevel> bestAsks)
        {
            int cnt = 0;
            foreach(var bid in bestBids)
            {
                BestBids[cnt].Price = bid.Price == decimal.MinValue ? BestAsks[cnt].Price : bid.Price;
                BestBids[cnt].Quantity = bid.Quantity == 0 ? BestBids[cnt].Quantity : bid.Quantity;
                BestBids[cnt].Orders = bid.Orders == 0 ? BestBids[cnt].Orders : bid.Orders;
                ++cnt;
            }
            cnt = 0;
            foreach (var ask in bestAsks)
            {
                BestAsks[cnt].Price = ask.Price == decimal.MinValue ? BestAsks[cnt].Price : ask.Price;
                BestAsks[cnt].Quantity = ask.Quantity == 0 ? BestAsks[cnt].Quantity : ask.Quantity;
                BestAsks[cnt].Orders = ask.Orders == 0 ? BestAsks[cnt].Orders : ask.Orders;
                ++cnt;
            }
        }
    }
}
