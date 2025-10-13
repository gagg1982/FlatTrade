using Newtonsoft.Json;

namespace FlatTrade.FundManager
{
    public class MaxPayOutAmountResponse : BaseErrorMessageResponse
    {
        [JsonProperty("actid")]
        public required string AccountId { get; set; }

        [JsonProperty("payout")]
        public required decimal MaximumPayOutAmount { get; set; }
    }
}
