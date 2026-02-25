using FlatTrade.Types.Base;
using Newtonsoft.Json;

namespace FlatTrade.OrderManager
{
    public class MultiLegOrderBookRequest
    {
        [JsonProperty("uid")]
        public required string UserId { get; set; }

        [JsonProperty("prd")]
        public required ProductType ProductType { get; set; }
    }
}
