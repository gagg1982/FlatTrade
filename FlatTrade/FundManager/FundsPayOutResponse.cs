using Newtonsoft.Json;

namespace FlatTrade.FundManager
{
    public class FundsPayOutResponse : BaseErrorMessageResponse
    {
        [JsonProperty("trn_id")]
        public long TransactionId { get; set; }

        private string _rejectionReason = string.Empty;

        [JsonProperty("rejreason")]
        public string RejectionReason
        {
            get => _rejectionReason; // Refactored to refer to the field '_errorMsg'
            set
            {
                base.ErrorMsg = value;
                _rejectionReason = value;
            }
        }
    }
}
