using Newtonsoft.Json;

namespace FlatTrade.LimitsManager
{
    public class LimitsRequest
    {
        [JsonProperty("uid")]
        public required string UserId { get; set; }

        [JsonProperty("actid")]
        public required string AccountId { get; set; }

    }
}
