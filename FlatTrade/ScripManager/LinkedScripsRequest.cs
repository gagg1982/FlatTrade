using FlatTrade.Common.JsonConvertors;
using FlatTrade.Common.Types.Base;
using Newtonsoft.Json;
namespace FlatTrade.ScripManager
{
    public class LinkedScripsRequest
    {
        [JsonProperty("uid")]
        public required string UserId { get; set; }

        [JsonProperty("token")]
        [JsonConverter(typeof(LongAsStringConverter))]
        public required long Token { get; set; }

        [JsonProperty("exch")]
        public required Exchange Exchange { get; set; }
    }
}
