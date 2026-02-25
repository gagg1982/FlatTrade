using Common.JsonConvertors;
using Newtonsoft.Json;

namespace FlatTrade.FundManager
{
    public class GetPayInReportRequest
    {
        [JsonProperty("actid")]
        public required string AccountId { get; set; }

        [JsonProperty("from_date")]
        [JsonConverter(typeof(DateOnlyAsStringConverter))]
        public required DateOnly FromDate { get; set; }

        [JsonProperty("to_date")]
        [JsonConverter(typeof(DateOnlyAsStringConverter))]
        public required DateOnly ToDate { get; set; }
    }
}
