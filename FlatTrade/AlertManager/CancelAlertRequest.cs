using FlatTrade.Common.JsonConvertors;
using Newtonsoft.Json;

namespace FlatTrade.AlertManager
{
    public class CancelAlertRequest
    {
        [JsonProperty("uid")]
        public required string UserId { get; set; }

        [JsonProperty("al_id")]
        [JsonConverter(typeof(LongAsStringConverter))]
        public required long AlertId { get; set; }

    }
}
