using FlatTrade.Common.JsonConvertors;
using FlatTrade.Common.Types.Base;
using Newtonsoft.Json;

namespace FlatTrade.MarketInfoManager
{
    public class BrokerageRequest
    {
        [JsonProperty("uid")]
        public required string UserId { get; set; }

        [JsonProperty("actid")]
        public required string AccountId { get; set; } = string.Empty;

        [JsonProperty("exch")]
        public required Exchange Exchange { get; set; }

        [JsonProperty("tsym")]
        public required string TradingSymbol { get; set; } = string.Empty;

        [JsonProperty("prd")]
        public required ProductType ProductType { get; set; }

        [JsonProperty("qty")]
        [JsonConverter(typeof(LongAsStringConverter))]
        public required long Quantity { get; set; }

        [JsonProperty("prc")]
        [JsonConverter(typeof(DecimalAsStringConverter))]
        public required decimal Price { get; set; }

        [JsonProperty("trantype")]
        public required TransactionType TransactionType { get; set; }
    }
}
