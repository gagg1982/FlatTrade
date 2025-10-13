using Newtonsoft.Json;

namespace FlatTrade.AlertManager
{
    public class EnabledAlertTypesRequest
    {
        [JsonProperty("uid")]
        public required string UserId { get; set; }
    }
}
