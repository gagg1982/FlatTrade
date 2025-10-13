using FlatTrade.Common.JsonConvertors;
using FlatTrade.Common.Types.Base;
using FlatTrade.ScripManager;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FlatTrade.SubscriptionManager.Quote
{
    public class QuoteSubscriptionUpdates : BaseSubscriptionRequest
    {
        [JsonProperty("e")]
        public Exchange Exchange { get; set; }

        [JsonProperty("tk")]
        public long Token { get; set; }

        [JsonProperty("ts")]
        public string TradingSymbol { get; set; } = string.Empty;

        [JsonProperty("pp")]
        public int PricePrecision { get; set; }

        [JsonProperty("ls")]
        public decimal LotSize { get; set; }

        [JsonProperty("ti")]
        public decimal TickSize { get; set; }

        [JsonProperty("lp")]
        public decimal LastTradePrice { get; set; }

        [JsonProperty("pc")]
        public decimal LastTradePricePercentageChange { get; set; }

        [JsonProperty("ft")]
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime LastTradeDateTime { get; set; }

        [JsonProperty("c")]
        public decimal DayClosePrice { get; set; }

        [JsonProperty("o")]
        public decimal DayOpenPrice { get; set; }

        [JsonProperty("h")]
        public decimal DayHighPrice { get; set; }

        [JsonProperty("l")]
        public decimal DayLowPrice { get; set; }

        [JsonProperty("ap")]
        public decimal AverageTradePrice { get; set; }

        [JsonProperty("v")]
        public long DayVolume { get; set; }

        [JsonProperty("ltq")]
        public long LastTradeQuantity { get; set; }

        [JsonProperty("ltt")]
        public string LastTradeTime { get; set; } = string.Empty;

        [JsonProperty("tsq")]
        public decimal TotalSellQuantity { get; set; }

        [JsonProperty("tbq")]
        public decimal TotalBuyQuantity { get; set; }

        [JsonConverter(typeof(QuotesResponseJsonConvertor))]
        public List<MarketDepthLevel> BestBids { get; set; } = [];

        [JsonConverter(typeof(QuotesResponseJsonConvertor))]
        public List<MarketDepthLevel> BestAsks { get; set; } = [];

        [JsonProperty("uc")]
        public decimal UpperCircuitLimit { get; set; }

        [JsonProperty("lc")]
        public decimal LowerCircuitLimit { get; set; }

        [JsonProperty("52h")]
        public decimal Wk52High { get; set; }

        [JsonProperty("52l")]
        public decimal Wk52Low { get; set; }

        [JsonProperty("52hd")]
        [JsonConverter(typeof(DateOnlyAsStringConverterDdMonYyyy))]
        public DateOnly Wk52HighDate { get; set; }

        [JsonProperty("52ld")]
        [JsonConverter(typeof(DateOnlyAsStringConverterDdMonYyyy))]
        public DateOnly Wk52LowDate { get; set; }

    }
}
