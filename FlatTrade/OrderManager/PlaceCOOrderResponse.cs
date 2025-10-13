using Newtonsoft.Json;

namespace FlatTrade.OrderManager
{
    public class PlaceCOOrderResponse : BaseErrorMessageResponse
    {
        [JsonProperty("norenordno")]
        public long NorenOrderNumber { get; set; }

        //[JsonProperty("al_id")]
        //public long AlertId { get; set; }
    }
}
