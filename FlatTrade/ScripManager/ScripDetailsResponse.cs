using Newtonsoft.Json;

namespace FlatTrade.ScripManager
{
    public class ScripDetailsResponse : BaseErrorMessageResponse
    {
        [JsonProperty("values")]
        public List<ScripDetails> ScripDetails { get; set; } = [];
    }
}
