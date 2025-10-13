using Newtonsoft.Json;

namespace FlatTrade.TradeManager
{
    public class PositionBookRequest
    {
        [JsonProperty("actid")]
        public required string AccountId { get; set; }

        [JsonProperty("uid")]
        public required string UserId { get; set; }
    }
}
