using Newtonsoft.Json;

namespace FlatTrade.OrderManager
{
    public class PlaceOrderResponse : BaseErrorMessageResponse
    {        
        [JsonProperty("norenordno")]
        public long NorenOrderNumber { get; set; }

    }
}
