using FlatTrade.Common.JsonConvertors;
using FlatTrade.Common.Types.Base;
using Newtonsoft.Json;

namespace FlatTrade.OrderManager
{
    public class ModifyOrderRequest
    {
        [JsonProperty("norenordno")]
        [JsonConverter(typeof(LongAsStringConverter))]
        public long NorenOrderNumber { get; set; }

        [JsonProperty("exch")]
        public Exchange Exchange { get; set; }

        [JsonProperty("prctyp")]
        public PriceType PriceType { get; set; }  //LMT/MKT

        [JsonProperty("prc")]
        [JsonConverter(typeof(DecimalAsStringConverter))]
        public decimal Price { get; set; }

        [JsonProperty("qty")]
        [JsonConverter(typeof(LongAsStringConverter))]
        public long Quantity { get; set; }

        [JsonProperty("tsym")]
        public string TradingSymbol { get; set; } = string.Empty;

        [JsonProperty("ret")]
        public RetentionType RetentionType { get; set; } //DAY/IOC/EOS

        [JsonProperty("trgprc")]
        [JsonConverter(typeof(DecimalAsStringConverter))]
        public decimal TriggerPrice { get; set; }

        [JsonProperty("uid")]
        public required string UserId { get; set; }

        [JsonProperty("bpprc")]
        [JsonConverter(typeof(DecimalAsStringConverter))]
        public decimal BookProfitPrice { get; set; }

        [JsonProperty("blprc")]
        [JsonConverter(typeof(DecimalAsStringConverter))]
        public decimal BookLossPrice { get; set; }

        [JsonProperty("trailprc")]
        [JsonConverter(typeof(DecimalAsStringConverter))]
        public decimal TrailingPrice { get; set; }

        [JsonProperty("mkt_protection")]
        [JsonConverter(typeof(DecimalAsStringConverter))]
        public decimal MarketProtectionPercentage { get; set; } = 0.0m;
    }
}
