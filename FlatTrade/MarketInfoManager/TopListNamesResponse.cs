using Newtonsoft.Json;

namespace FlatTrade.MarketInfoManager
{
    public class TopListNamesResponse : BaseErrorMessageResponse
    {
        [JsonProperty("values")]
        public List<BasketCriteriaPair> BasketCriteriaPair { get; set; } = [];
    }
}
