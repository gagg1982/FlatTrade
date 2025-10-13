using FlatTrade.Common.JsonConvertors;
using FlatTrade.Common.Types.Base;
using Newtonsoft.Json;

namespace FlatTrade.OrderManager
{
    public class PlaceGttOrderRequest
    {
        [JsonProperty("actid")]
        public required string AccountId { get; set; }

        [JsonProperty("uid")]
        public required string UserId { get; set; }

        [JsonProperty("exch")]
        public required Exchange Exchange { get; set; }

        [JsonProperty("tsym")]
        public required string TradingSymbol { get; set; }

        [JsonProperty("ai_t")]
        public required GTTAlertType AlertType { get; set; }

        [JsonProperty("validity")]
        public required RetentionType Validity { get; set; }

        [JsonProperty("d")]
        [JsonConverter(typeof(DecimalAsStringConverter))]
        public decimal DataToBeComparedWithLTP { get; set; }

        [JsonProperty("remarks")]
        public required string Remarks { get; set; }

        [JsonProperty("prd")]
        public required ProductType ProductType { get; set; }

        [JsonProperty("prctyp")]
        public required PriceType PriceType { get; set; } //LMT/MKT/SL-LMT/SL-MKT/DS/2L/3L

        [JsonProperty("trantype")]
        public required TransactionType TransactionType { get; set; }

        [JsonProperty("trgprc")]
        [JsonConverter(typeof(DecimalAsStringConverter))]
        public required decimal TriggeringPrice { get; set; }

        [JsonProperty("ret")]
        public required RetentionType RetentionType { get; set; } //DAY/IOC/EOS

        [JsonProperty("dscqty")]
        [JsonConverter(typeof(LongAsStringConverter))]
        public required long DisclosedQuantity { get; set; }

        [JsonProperty("qty")]
        [JsonConverter(typeof(LongAsStringConverter))]
        public required long Quantity { get; set; }

        [JsonProperty("prc")]
        [JsonConverter(typeof(DecimalAsStringConverter))]
        public required decimal Price { get; set; }
    }
}
