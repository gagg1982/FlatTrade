using FlatTrade.Types.Base;
using Newtonsoft.Json;

namespace FlatTrade.SubscriptionManager
{
    public class ConnectRequest : BaseSubscriptionRequest
    {
        [JsonProperty("uid")]
        public string UserId { get; set; } = string.Empty;

        [JsonProperty("actid")]
        public string AccountId { get; set; } = string.Empty;

        [JsonProperty("source")]
        public AccessType Source { get; set; } = AccessType.API;

        [JsonProperty("susertoken")]
        public string UserSessionToken { get; set; } = string.Empty;

    }
}
