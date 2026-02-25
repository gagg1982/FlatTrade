using Common.JsonConvertors;
using FlatTrade.Types.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FlatTrade.OrderManager
{
    public class BasketRequest
    {
        [JsonProperty("exch")]
        [JsonConverter(typeof(StringEnumConverter))]
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

        [JsonProperty("introp_key")]
        public string OptionalIntropKey { get; set; } = string.Empty;

        [JsonProperty("introp_exch")]
        public Exchange OptionalIntropExchange { get; set; }


    }
}
