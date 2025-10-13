using Newtonsoft.Json;

namespace FlatTrade.MarketInfoManager
{
    public class TopListResponse : BaseErrorMessageResponse
    {
        [JsonProperty("values")]
        public IEnumerable<TopBottomContracts> TopBottomContracts { get; set; } = [];

    }
}
