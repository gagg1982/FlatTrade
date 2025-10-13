using FlatTrade.Common.JsonConvertors;
using FlatTrade.Common.Types.Base;
using Newtonsoft.Json;

namespace FlatTrade.SubscriptionManager.TouchLine
{
    public class TouchLineSubscriptionUpdates : BaseSubscriptionRequest
    {

        [JsonProperty("ts")]
        public string TradingSymbol { get; set; } = string.Empty;

        [JsonProperty("e")]
        public Exchange Exchange { get; set; }

        [JsonProperty("seg")]
        public string Segment { get; set; } = string.Empty;

        [JsonProperty("instname")]
        public InstrumentName InstrumentName { get; set; }

        [JsonProperty("isin")]
        public string Isin { get; set; } = string.Empty;

        [JsonProperty("tk")]
        public long Token { get; set; }

        [JsonProperty("pp")]
        public int PricePrecision { get; set; }

        [JsonProperty("ti")]
        public decimal TickSize { get; set; }

        [JsonProperty("ls")]
        public decimal LotSize { get; set; }

        [JsonProperty("toi")]
        public long IntervalIoChange { get; set; }

        [JsonProperty("pc")]
        public decimal LastTradePricePercentageChange { get; set; }

        [JsonProperty("lp")]
        public decimal LastTradePrice { get; set; }

        [JsonProperty("prcftr")]
        public string PriceFactor { get; set; } = string.Empty;

        [JsonProperty("c")]
        public decimal C { get; set; } 

        [JsonProperty("mult")]
        public decimal Multiplier { get; set; }

        [JsonProperty("issue_d")]
        [JsonConverter(typeof(DateOnlyAsStringConverter))]
        public DateOnly IssueDate { get; set; }

        [JsonProperty("listing_d")]
        [JsonConverter(typeof(DateOnlyAsStringConverter))]
        public DateOnly ListingDate { get; set; }

        [JsonProperty("uc")]
        public decimal UpperCircuitLimit { get; set; }

        [JsonProperty("lc")]
        public decimal LowerCircuitLimit { get; set; }

        [JsonProperty("prcftr_d")]
        public string PriceFactorD { get; set; } = string.Empty;

        [JsonProperty("ord_msg")]
        public string Message { get; set; } = string.Empty;

        [JsonProperty("delmrg")]
        public decimal DelMargin { get; set; }

        [JsonProperty("varmrg")]
        public decimal VarMargin { get; set; }

        [JsonProperty("elmmrg")]
        public decimal ElmMargin { get; set; }

        [JsonProperty("frzqty")]
        public long FreezeQuantity { get; set; }
    }
}
