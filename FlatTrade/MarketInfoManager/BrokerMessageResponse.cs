using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FlatTrade.MarketInfoManager
{
    public class BrokerMessageResponse : BaseErrorMessageResponse
    {
        [JsonProperty("dmsg")]
        public string BrokerDetailedMessage { get; set; } = string.Empty;

        [JsonProperty("msgtyp")]
        public string MessageType { get; set; } = string.Empty;

        [JsonProperty("norentm")]
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime NorenTime { get; set; }
    }
}
