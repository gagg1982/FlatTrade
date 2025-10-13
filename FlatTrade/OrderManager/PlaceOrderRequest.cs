using FlatTrade.Common.JsonConvertors;
using FlatTrade.Common.Types.Base;
using Newtonsoft.Json;

namespace FlatTrade.OrderManager
{
    public class PlaceOrderRequest
    {
        [JsonProperty("uid")]
        public required string UserId { get; set; }

        [JsonProperty("actid")]
        public required string AccountId { get; set; }

        [JsonProperty("exch")]
        public Exchange Exchange { get; set; }

        [JsonProperty("tsym")]
        public string TradingSymbol { get; set; } = string.Empty;

        [JsonProperty("qty")]
        [JsonConverter(typeof(LongAsStringConverter))]
        public long Quantity { get; set; }

        [JsonProperty("prc")]
        [JsonConverter(typeof(DecimalAsStringConverter))]
        public decimal Price { get; set; }

        [JsonProperty("trgprc")]
        [JsonConverter(typeof(DecimalAsStringConverter))]
        public decimal TriggerPrice { get; set; }

        [JsonProperty("prd")]
        public ProductType ProductType { get; set; }

        [JsonProperty("trantype")]
        public TransactionType TransactionType { get; set; }

        [JsonProperty("prctyp")]
        public PriceType PriceType { get; set; } //LMT/MKT

        [JsonProperty("ret")]
        public RetentionType RetentionType { get; set; } //DAY/IOC/EOS

        [JsonProperty("remarks")]
        public string Remarks { get; set; } = string.Empty;

        [JsonProperty("ordersource")]
        public AccessType OrderSource { get; set; } = AccessType.API;

        [JsonProperty("blprc")]
        [JsonConverter(typeof(DecimalAsStringConverter))]
        public decimal BookLossProfit { get; set; }

        [JsonProperty("bpprc")]
        [JsonConverter(typeof(DecimalAsStringConverter))]
        public decimal BookProfitPrice { get; set; }

        [JsonProperty("trailprc")]
        [JsonConverter(typeof(DecimalAsStringConverter))]
        public decimal TrailingPrice { get; set; }

        [JsonProperty("amo")]
        public string Amo { get; set; } = string.Empty; // value ="Yes"

        //----------------------------------------------------------------------

        [JsonProperty("tsym2")]
        public string Symbol2 { get; set; } = string.Empty;

        [JsonProperty("trantype2")]
        public TransactionType TransactionType2 { get; set; }

        [JsonProperty("qty2")]
        [JsonConverter(typeof(LongAsStringConverter))]
        public long Quantity2 { get; set; }

        [JsonProperty("prc2")]
        [JsonConverter(typeof(DecimalAsStringConverter))]
        public decimal Price2 { get; set; }

        //----------------------------------------------------------------------

        [JsonProperty("tsym3")]
        public string Symbol3 { get; set; } = string.Empty;

        [JsonProperty("trantype3")]
        public TransactionType TransactionType3 { get; set; }

        [JsonProperty("qty3")]
        [JsonConverter(typeof(LongAsStringConverter))]
        public long Quantity3 { get; set; }

        [JsonProperty("prc3")]
        [JsonConverter(typeof(DecimalAsStringConverter))]
        public decimal Price3 { get; set; }

        [JsonProperty("mkt_protection")]
        [JsonConverter(typeof(DecimalAsStringConverter))]
        public decimal MarketProtectionPercentage { get; set; }

    }
}
