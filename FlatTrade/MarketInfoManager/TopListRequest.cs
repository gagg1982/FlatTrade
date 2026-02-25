using FlatTrade.Types.Base;
using Newtonsoft.Json;

namespace FlatTrade.MarketInfoManager
{
    public class TopListRequest
    {
        [JsonProperty("uid")]
        public required string UserId { get; set; }

        [JsonProperty("exch")]
        public required Exchange Exchange { get; set; }

        [JsonProperty("tb")]
        public required TopBottomType TopOrBottom { get; set; }

        [JsonProperty("bskt")]
        public required BasketType BasketName { get; set; }

        [JsonProperty("crt")]
        public required Criteria Criteria { get; set; }
    }
}
