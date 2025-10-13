using Newtonsoft.Json;

namespace FlatTrade.MarketInfoManager
{
    public class OptionChainResponse : BaseErrorMessageResponse
    {
        [JsonProperty("values")]
        public IEnumerable<OptionChainDataResponse> OptionChainDataResponse { get; set; } = [];
    }
}
