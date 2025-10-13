using Newtonsoft.Json;

namespace FlatTrade.OrderManager
{
    public class OrderBookRequest
    {
        [JsonProperty("uid")]
        public required string UserId { get; set; }
    }
}
