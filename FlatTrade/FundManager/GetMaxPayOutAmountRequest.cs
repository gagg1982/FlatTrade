using Newtonsoft.Json;

namespace FlatTrade.FundManager
{
    public class GetMaxPayOutAmountRequest
    {
        [JsonProperty("actid")]
        public required string AccountId { get; set; }

        [JsonProperty("uid")]
        public required string UserId { get; set; }

    }
}
