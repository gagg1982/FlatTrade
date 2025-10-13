
using Newtonsoft.Json;

namespace FlatTrade.AlertManager
{
    public class ModifyAlertResponse : BaseErrorMessageResponse
    {
        [JsonProperty("al_id")]
        public long AlertId { get; set; }
    }
}
