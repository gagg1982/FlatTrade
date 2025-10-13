using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FlatTrade
{
    public class BaseErrorMessageResponse
    {
        [JsonProperty("stat")]
        public string Status { get; set; } = string.Empty;

        [JsonProperty("emsg")]
        public string ErrorMsg { get; set; } = string.Empty;

        [JsonProperty("request_time")]
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime RequestTime { get; set; }
    }
}
