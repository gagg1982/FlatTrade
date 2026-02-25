using Newtonsoft.Json;

namespace FlatTrade.SubscriptionManager.Order
{
    public class OrderSubscriptionRequestAck : BaseSubscriptionRequest
    {
        [JsonProperty("dmsg")]
        public string BrokerDetailedMessage { get; set; } = string.Empty;

    }
}
