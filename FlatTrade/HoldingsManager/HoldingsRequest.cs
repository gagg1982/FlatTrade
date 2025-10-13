using FlatTrade.Common.Types.Base;
using Newtonsoft.Json;

namespace FlatTrade.HoldingsManager
{
    public class HoldingsRequest
    {
        [JsonProperty("uid")]
        public required string UserId { get; set; }

        [JsonProperty("actid")]
        public required string AccountId { get; set; }

        [JsonProperty("prd")]
        public required ProductType ProductType { get; set; }

    }
}
