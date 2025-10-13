using Newtonsoft.Json;

namespace FlatTrade.OrderManager
{
    public class EnabledGTTsRequest
    {

        [JsonProperty("uid")]
        public required string UserId { get; set; }
    }
}
