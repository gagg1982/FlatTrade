using Newtonsoft.Json;

namespace FlatTrade.OrderManager
{
    public class PlaceGttOrderResponse : BaseErrorMessageResponse
    {
        [JsonProperty("al_id")]
        public long AlertId { get; set; }

    }
}
