using FlatTrade.Types.Base;
using Newtonsoft.Json;

namespace FlatTrade.MarketInfoManager
{
    public class TopListNamesRequest
    {
        [JsonProperty("uid")]
        public required string UserId { get; set; }

        [JsonProperty("exch")]
        public required Exchange Exchange { get; set; }
    }
}
