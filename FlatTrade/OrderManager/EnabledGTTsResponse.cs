using FlatTrade.Types.Base;
using Newtonsoft.Json;

namespace FlatTrade.OrderManager
{
    public class EnabledGTTsResponse : BaseErrorMessageResponse
    {
        [JsonProperty("ai_ts")]
        public List<AlertType> AlertTypes { get; set; } = [];
    }
}
