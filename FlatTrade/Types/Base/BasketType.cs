using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FlatTrade.Types.Base
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum BasketType
    {
        NSEALL,
        NSEEQ,
        NSEBL
    }
}
