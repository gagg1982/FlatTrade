using FlatTrade.Common.JsonConvertors;
using FlatTrade.Common.Types.Base;
using Newtonsoft.Json;

namespace FlatTrade.TradeManager
{
    public class ProductConversionRequest
    {
        [JsonProperty("actid")]
        public required string AccountId { get; set; }

        [JsonProperty("uid")]
        public required string UserId { get; set; }

        [JsonProperty("exch")]
        public required Exchange Exchange { get; set; }

        [JsonProperty("TradingSymbol")]
        public required string Symbol { get; set; }

        [JsonProperty("qty")]
        [JsonConverter(typeof(LongAsStringConverter))]
        public long Quantity { get; set; }

        [JsonProperty("prd")]
        public required ProductType ProductType { get; set; }

        [JsonProperty("prevprd")]
        public required ProductType OriginalPositionProductType { get; set; }

        [JsonProperty("trantype")]
        public required TransactionType TransactionType { get; set; }

        [JsonProperty("postype")]
        public required string PositionType { get; set; } //DAY/Carry forward

        [JsonProperty("ordersource")]
        public AccessType OrderSource { get; set; } = AccessType.API;

    }
}
