using Newtonsoft.Json;

namespace FlatTrade.SubscriptionManager
{
    public class ConnectResponse : BaseSubscriptionRequest
    {
        [JsonProperty("uid")]
        public string UserId { get; set; } = string.Empty;


        [JsonProperty("s")]
        public string Status { get; set; } = string.Empty;
    }
}
