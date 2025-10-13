using FlatTrade.Common.JsonConvertors;
using Newtonsoft.Json;

namespace FlatTrade.FundManager
{
    public class CancelPayOutRequest
    {
        [JsonProperty("actid")]
        public required string AccountId { get; set; }

        [JsonProperty("uid")]
        public required string UserId { get; set; }

        [JsonProperty("brkname")]
        public string BrokerName { get; set; } = string.Empty;

        [JsonProperty("trans_ref_num")]
        [JsonConverter(typeof(LongAsStringConverter))]
        public required long TransactionReferenceNumber { get; set; }

    }
}
