using Newtonsoft.Json;

namespace FlatTrade.AlertManager
{
    public class CancelAlertResponse : BaseErrorMessageResponse
    {
        [JsonProperty("al_id")]
        public long AlertId { get; set; }
    }
}
