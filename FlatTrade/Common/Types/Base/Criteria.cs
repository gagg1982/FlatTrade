using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FlatTrade.Common.Types.Base
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Criteria
    {
        LTP,
        VOLUME,
        VALUE
    }
}
