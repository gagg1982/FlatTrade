using Newtonsoft.Json;

namespace FlatTrade.SubscriptionManager.Quote
{
    public class QuoteUnsubscriptionRequestAck : BaseSubscriptionRequest
    {
        [JsonProperty("k")]
        public string SubscriptionScriptList { get; set; } = string.Empty;
    }
}
