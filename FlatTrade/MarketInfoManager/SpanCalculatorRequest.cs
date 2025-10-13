using Newtonsoft.Json;

namespace FlatTrade.MarketInfoManager
{
    public class SpanCalculatorRequest
    {
        [JsonProperty("pos")]
        public required List<PositionRequest> Positions { get; set; }

        [JsonProperty("actid")]
        public required string AccountId { get; set; }

    }
}
