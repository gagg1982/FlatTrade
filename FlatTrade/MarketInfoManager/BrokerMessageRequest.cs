using Newtonsoft.Json;

namespace FlatTrade.MarketInfoManager
{
    public class BrokerMessageRequest
    {
        [JsonProperty("uid")]
        public required string UserId { get; set; }

    }
}
