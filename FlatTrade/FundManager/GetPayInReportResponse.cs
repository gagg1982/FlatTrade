using Newtonsoft.Json;

namespace FlatTrade.FundManager
{
    public class GetPayInReportResponse : BaseErrorMessageResponse
    {
        [JsonProperty("actid")]
        public required string AccountId { get; set; }

        [JsonProperty("trans_ref_num")]
        public long TransactionReferenceNumber { get; set; }

        [JsonProperty("tran_status")]
        public string TransactionStatus { get; set; } = string.Empty;

        [JsonProperty("amt")]
        public decimal Amount { get; set; }
    }
}
