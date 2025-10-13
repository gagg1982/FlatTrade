using Newtonsoft.Json;

namespace FlatTrade.OrderManager
{
    public class BasketMarginResponse : BaseErrorMessageResponse
    {

        [JsonProperty("remarks")]
        public string Remarks { get; set; } = string.Empty;

        [JsonProperty("marginused")]
        public decimal MarginUsed { get; set; }

        [JsonProperty("marginunusedtrade")]
        public decimal MarginUsedAfterTrade { get; set; }
    }
}
