using Common.JsonConvertors;
using FlatTrade.Types.Base;
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
        public required Exchange Exchange { get; set; }

        [JsonProperty("tsym")]
        public required string TradingSymbol { get; set; } = string.Empty;

        [JsonProperty("qty")]
        [JsonConverter(typeof(LongAsStringConverter))]
        public required long Quantity { get; set; }

        [JsonProperty("dscqty")]
        [JsonConverter(typeof(LongAsStringConverter))]
        public long DisclosedQuantity { get; set; } = 0;

        [JsonProperty("prc")]
        [JsonConverter(typeof(DecimalAsStringConverter))]
        public decimal Price { get; set; } = 0.0m;

        [JsonProperty("trgprc")]
        [JsonConverter(typeof(DecimalAsStringConverter))]
        public decimal TriggerPrice { get; set; } = 0.0m;

        [JsonProperty("prd")]
        public required ProductType ProductType { get; set; }

        [JsonProperty("trantype")]
        public required TransactionType TransactionType { get; set; }

        [JsonProperty("prctyp")]
        public required PriceType PriceType { get; set; } //LMT/MKT

        [JsonProperty("ret")]
        public required RetentionType RetentionType { get; set; } //DAY/IOC/EOS

        [JsonProperty("remarks")]
        public string Remarks { get; set; } = string.Empty;

        [JsonProperty("ordersource")]
        public AccessType OrderSource { get; set; } = AccessType.API;

        [JsonProperty("blprc")]
        [JsonConverter(typeof(DecimalAsStringConverter))]
        public decimal BookLossPrice { get; set; } = 0;

        [JsonProperty("bpprc")]
        [JsonConverter(typeof(DecimalAsStringConverter))]
        public decimal BookProfitPrice { get; set; } = 0;

        [JsonProperty("trailprc")]
        [JsonConverter(typeof(DecimalAsStringConverter))]
        public decimal TrailingPrice { get; set; }

        [JsonProperty("rorgqty")]
        [JsonConverter(typeof(DecimalAsStringConverter))]
        public long RemaningOriginalQuantityFromModify { get; set; } = 0;// used in modify

        [JsonProperty("rorgprc")]
        [JsonConverter(typeof(DecimalAsStringConverter))]
        public decimal RemainingOriginalPriceFromModify { get; set; } = 0;

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
        public decimal MarketProtectionPercentage { get; set; } = 0.0m;

    }
}
