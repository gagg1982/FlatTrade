using Newtonsoft.Json;

namespace FlatTrade.AlertManager
{
    public class PendingAlertRequest
    {
        [JsonProperty("uid")]
        public required string UserId { get; set; }
    }
}
