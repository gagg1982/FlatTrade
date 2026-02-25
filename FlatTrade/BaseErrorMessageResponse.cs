namespace FlatTrade
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Common.Types;

    public class BaseErrorMessageResponse : BaseErrorMessage
    {
        [JsonProperty("stat")]
        public string Status { get; set; } = string.Empty;


        [JsonProperty("request_time")]
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime RequestTime { get; set; }
    }
}
