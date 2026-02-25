using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FlatTrade.Types.Base
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Criteria
    {
        LTP,
        VOLUME,
        VALUE
    }
}
