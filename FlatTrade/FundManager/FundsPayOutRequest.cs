using Common.JsonConvertors;
using Newtonsoft.Json;

namespace FlatTrade.FundManager
{
    public class FundsPayOutRequest
    {
        [JsonProperty("actid")]
        public required string AccountId { get; set; }

        [JsonProperty("uid")]
        public required string UserId { get; set; }

        [JsonProperty("payout")]
        [JsonConverter(typeof(DecimalAsStringConverter))]
        public required decimal PayOutAmount { get; set; }

        [JsonProperty("remarks")]
        public string Remarks { get; set; } = string.Empty;
    }
}
