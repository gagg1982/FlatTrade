using Common.JsonConvertors;
using FlatTrade.Types.Base;
using Newtonsoft.Json;

namespace FlatTrade.ScripManager
{
    public class LinkedOptionResponse
    {
        [JsonProperty("exch")]
        public Exchange Exchange { get; set; }

        [JsonProperty("tsym")]
        public string TradingSymbol { get; set; } = string.Empty;

        [JsonProperty("exd")]
        [JsonConverter(typeof(DateOnlyAsStringConverterDdMonYyyy))]
        public DateOnly Expirydate { get; set; }
    }
}
