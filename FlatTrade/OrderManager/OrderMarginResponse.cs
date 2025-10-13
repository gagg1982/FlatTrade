using Newtonsoft.Json;

namespace FlatTrade.OrderManager
{
    public class OrderMarginResponse : BaseErrorMessageResponse
    {
        [JsonProperty("remarks")]
        public string Remarks { get; set; } = string.Empty;

        [JsonProperty("cash")]
        public decimal TotalCashAvailable { get; set; }

        [JsonProperty("marginused")]
        public decimal TotalMarginUsed { get; set; }
    }
}
