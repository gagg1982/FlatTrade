using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FlatTrade.Common.Types.Base
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AccessType
    {
        TT,
        WEB,
        MOB,
        WEBE,
        WEBS,
        API,
        RED,
        ORA,
        SNO,
    }
}
