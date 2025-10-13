using Newtonsoft.Json;

namespace FlatTrade.FundManager
{
    public class CancelPayOutResponse : BaseErrorMessageResponse
    {
        [JsonProperty("actid")]
        public string AccountId { get; set; } = string.Empty;

        [JsonProperty("tran_status")]
        public string TransactionStatus { get; set; } = string.Empty;

    }
}
