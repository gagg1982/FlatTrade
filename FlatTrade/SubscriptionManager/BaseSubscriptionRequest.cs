using Newtonsoft.Json;

namespace FlatTrade.SubscriptionManager
{
    public class BaseSubscriptionRequest
    {
        [JsonProperty("t")]
        public SubscriptionType RequestType { get; set; }
    }
}
