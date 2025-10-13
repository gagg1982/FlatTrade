using Newtonsoft.Json;

namespace FlatTrade.OrderManager
{
    public class ModifyGttOrderResponse : BaseErrorMessageResponse
    {
        [JsonProperty("al_id")]
        public long AlertId { get; set; }
    }
}
