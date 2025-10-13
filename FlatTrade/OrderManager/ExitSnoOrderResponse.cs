using Newtonsoft.Json;

namespace FlatTrade.OrderManager
{
    public class ExitSnoOrderResponse : BaseErrorMessageResponse
    {        
        [JsonProperty("dmsg")]
        public string BrokerDetailedMessage { get; set; } = string.Empty;

    }
}
