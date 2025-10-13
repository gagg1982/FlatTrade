using FlatTrade.Common.JsonConvertors;
using Newtonsoft.Json;

namespace FlatTrade.OrderManager
{
    public class CancelOcoOrderRequest
    {
        [JsonProperty("uid")]
        public required string UserId { get; set; }

        [JsonProperty("ai_id")]
        [JsonConverter(typeof(LongAsStringConverter))]
        public long AlertId { get; set; }
    }
}
