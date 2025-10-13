using FlatTrade.Common.JsonConvertors;
using Newtonsoft.Json;

namespace FlatTrade.OrderManager
{
    public class CancelOrderRequest
    {

        [JsonProperty("uid")]
        public required string UserId { get; set; }

        [JsonProperty("norenordno")]
        [JsonConverter(typeof(LongAsStringConverter))]
        public required long NorenOrderNumber { get; set; }

    }
}
