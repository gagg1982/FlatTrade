using Newtonsoft.Json;

namespace FlatTrade.SubscriptionManager.Order
{
    public class OrderSubscriptionRequest : BaseSubscriptionRequest
    {
        [JsonProperty("actid")]
        public string AccountId { get; set; } = string.Empty;
    }
}