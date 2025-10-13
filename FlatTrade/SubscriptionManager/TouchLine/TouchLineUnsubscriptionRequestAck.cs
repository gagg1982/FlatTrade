using Newtonsoft.Json;

namespace FlatTrade.SubscriptionManager.TouchLine
{
    public class TouchLineUnsubscriptionRequestAck : BaseSubscriptionRequest
    {
        [JsonProperty("k")]
        public string SubscriptionScriptList { get; set; } = string.Empty;
    }
}
