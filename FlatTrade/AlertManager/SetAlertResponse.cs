using Newtonsoft.Json;

namespace FlatTrade.AlertManager
{
    public class SetAlertResponse : BaseErrorMessageResponse
    {
        [JsonProperty("al_id")]
        public long AlertId { get; set; }
    }
}
