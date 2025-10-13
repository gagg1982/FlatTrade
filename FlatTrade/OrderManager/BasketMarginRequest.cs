using FlatTrade.Common.JsonConvertors;
using FlatTrade.Common.Types.Base;
using Newtonsoft.Json;

namespace FlatTrade.OrderManager
{
    public class BasketMarginRequest
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
        public PriceType PriceType { get; set; }  //LMT/MKT

        [JsonProperty("blprc")]
        [JsonConverter(typeof(DecimalAsStringConverter))]
        public decimal BookLossProfit { get; set; }

        [JsonProperty("rorgqty")]
        [JsonConverter(typeof(DecimalAsStringConverter))]
        public long RemaningOriginalQuantityFromModify { get; set; } // used in modify

        [JsonProperty("fillshares")]
        [JsonConverter(typeof(LongAsStringConverter))]
        public long TotalFilled { get; set; }

        [JsonProperty("rorgprc")]
        [JsonConverter(typeof(DecimalAsStringConverter))]
        public decimal RemainingOriginalPriceFromModify { get; set; }

        [JsonProperty("orgtrgprc")]
        [JsonConverter(typeof(DecimalAsStringConverter))]
        public decimal MarginTriggeringPriceFromModify { get; set; }

        [JsonProperty("norenordno")]
        [JsonConverter(typeof(LongAsStringConverter))]
        public long NorenOrderNumber { get; set; }

        [JsonProperty("snonum")]
        [JsonConverter(typeof(LongAsStringConverter))]
        public long SnoOrderNumber { get; set; }

        [JsonProperty("basketlists")]
        public List<BasketRequest> BasketListRequest { get; set; } = [];

    }
}
