using Newtonsoft.Json;

namespace FlatTrade.MarketInfoManager
{
    public class IndexNameTokenPair
    {
        [JsonProperty("idxname")]
        public string IndexName { get; set; } = string.Empty;

        [JsonProperty("token")]
        public long Token { get; set; }
    }
}
