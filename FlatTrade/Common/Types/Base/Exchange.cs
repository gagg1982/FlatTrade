using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FlatTrade.Common.Types.Base
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Exchange
    {
        NSE,
        BSE,
        MCX,
        NFO,
        CDS,
        BFO,
        BCD,
        EQT,
    }
}
