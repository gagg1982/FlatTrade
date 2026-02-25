using Common.JsonConvertors;
using Newtonsoft.Json;

namespace FlatTrade.OrderManager
{
    public class SingleOrderHistoryRequest
    {
        [JsonProperty("uid")]
        public required string UserId { get; set; }

        [JsonProperty("norenordno")]
        [JsonConverter(typeof(LongAsStringConverter))]
        public required long NorenOrderNumber { get; set; }
    }
}
