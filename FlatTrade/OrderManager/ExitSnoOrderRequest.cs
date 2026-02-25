using Common.JsonConvertors;
using FlatTrade.Types.Base;
using Newtonsoft.Json;

namespace FlatTrade.OrderManager
{
    public class ExitSnoOrderRequest
    {
        [JsonProperty("uid")]
        public required string UserId { get; set; }

        [JsonProperty("norenordno")]
        [JsonConverter(typeof(LongAsStringConverter))]
        public required long NorenOrderNumber { get; set; }

        [JsonProperty("prd")]
        public required ProductType ProductType { get; set; }
    }
}
