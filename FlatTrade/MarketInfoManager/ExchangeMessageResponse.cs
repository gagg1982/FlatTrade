using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FlatTrade.MarketInfoManager
{
    public class ExchangeMessageResponse : BaseErrorMessageResponse
    {
        [JsonProperty("exchmsg")]
        public string ExchangeMessage { get; set; } = string.Empty;

        [JsonProperty("exchtm")]
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime ExchangeTime { get; set; }
    }
}
