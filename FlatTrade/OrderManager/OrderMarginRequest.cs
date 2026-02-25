using Common.JsonConvertors;
using FlatTrade.Types.Base;
using Newtonsoft.Json;

namespace FlatTrade.OrderManager
{
    public class OrderMarginRequest
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
        public long Quantity { get; set; } = 0;

        [JsonProperty("prc")]
        [JsonConverter(typeof(DecimalAsStringConverter))]
        public decimal Price { get; set; } = 0;

        [JsonProperty("trgprc")]
        [JsonConverter(typeof(DecimalAsStringConverter))]
        public decimal TriggerPrice { get; set; } = 0;

        [JsonProperty("prd")]
        public ProductType ProductType { get; set; }

        [JsonProperty("trantype")]
        public TransactionType TransactionType { get; set; }

        [JsonProperty("prctyp")]
        public PriceType PriceType { get; set; } //LMT/MKT

        [JsonProperty("blprc")]
        [JsonConverter(typeof(DecimalAsStringConverter))]
        public decimal BookLossProfit { get; set; } = 0;

        [JsonProperty("rorgqty")]
        public long RemaningOriginalQuantityFromModify { get; set; } = 0;// used in modify

        [JsonProperty("fillshares")]
        [JsonConverter(typeof(LongAsStringConverter))]
        public long TotalFilled { get; set; } = 0;

        [JsonProperty("rorgprc")]
        [JsonConverter(typeof(DecimalAsStringConverter))]
        public decimal RemainingOriginalPriceFromModify { get; set; } = 0;

        [JsonProperty("orgtrgprc")]
        [JsonConverter(typeof(DecimalAsStringConverter))]
        public decimal MarginTriggeringPriceFromModify { get; set; } = 0;

        [JsonProperty("norenordno")]
        [JsonConverter(typeof(LongAsStringConverter))]
        public long NorenOrderNumber { get; set; } = 0;

        [JsonProperty("snonum")]
        [JsonConverter(typeof(LongAsStringConverter))]
        public long SnoOrderNumber { get; set; } = 0;
    }
}
