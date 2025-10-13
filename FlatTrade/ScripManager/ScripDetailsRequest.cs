using FlatTrade.Common.Types.Base;
using Newtonsoft.Json;

namespace FlatTrade.ScripManager
{
    public class ScripDetailsRequest
    {
        [JsonProperty("uid")]
        public required string UserId { get; set; }

        [JsonProperty("stext")]
        public required string SearchText { get; set; }

        [JsonProperty("exch")]
        public required Exchange Exchange { get; set; }
    }
}
