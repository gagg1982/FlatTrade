using Newtonsoft.Json;

namespace FlatTrade.OrderManager
{
    public class PendingGttOrderRequest
    {
        [JsonProperty("uid")]
        public required string UserId { get; set; }
    }
}
