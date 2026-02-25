using Common.JsonConvertors;
using Newtonsoft.Json;

namespace FlatTrade.OrderManager
{
    public class CancelGttOrderRequest
    {
        [JsonProperty("uid")]
        public required string UserId { get; set; }

        [JsonProperty("al_id")]
        [JsonConverter(typeof(LongAsStringConverter))]
        public long AlertId { get; set; }
    }
}
