using Newtonsoft.Json;

namespace FlatTrade.OrderManager
{
    public class ModifyOrderResponse : BaseErrorMessageResponse
    {
        [JsonProperty("result")]
        public long NorenOrderNumber { get; set; }
    }
}
