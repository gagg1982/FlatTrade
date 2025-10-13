using Newtonsoft.Json;

namespace FlatTrade.OrderManager
{
    public class CancelGttOrderResponse : BaseErrorMessageResponse
    {
        [JsonProperty("al_id")]
        public long AlertId { get; set; }
    }
}
