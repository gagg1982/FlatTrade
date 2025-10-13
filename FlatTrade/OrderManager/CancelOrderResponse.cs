
using Newtonsoft.Json;

namespace FlatTrade.OrderManager
{
    public class CancelOrderResponse : BaseErrorMessageResponse
    {
        [JsonProperty("result")]
        public long NorenOrderNumber { get; set; }
    }
}
