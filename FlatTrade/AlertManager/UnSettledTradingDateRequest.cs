using Newtonsoft.Json;

namespace FlatTrade.AlertManager
{
    public class UnSettledTradingDateRequest
    {
        [JsonProperty("uid")]
        public required string UserId { get; set; }
    }
}
