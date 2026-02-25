using Common.JsonConvertors;
using Newtonsoft.Json;

namespace FlatTrade.AlertManager
{
    public class UnSettledTradingDateResponse : BaseErrorMessageResponse
    {
        [JsonProperty("trd_date")]
        [JsonConverter(typeof(DdMmYyyyListConverter))]
        public List<DateTime> TradeDate { get; set; } = [];
    }
}
