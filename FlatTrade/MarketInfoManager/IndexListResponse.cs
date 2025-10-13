using Newtonsoft.Json;

namespace FlatTrade.MarketInfoManager
{
    public class IndexListResponse : BaseErrorMessageResponse
    {
        [JsonProperty("values")]
        public required List<IndexNameTokenPair> IndexNameTokenList { get; set; }
    }
}
