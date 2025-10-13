using Newtonsoft.Json;

namespace FlatTrade.SubscriptionManager.Quote
{
    public class QuoteSubscriptionRequest : BaseSubscriptionRequest
    {
        [JsonProperty("k")]
        public string SubscriptionScriptList { get; set; } = string.Empty;
    }
}