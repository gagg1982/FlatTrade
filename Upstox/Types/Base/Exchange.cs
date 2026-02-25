using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Upstox.Types.Base
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Exchange
    {
        NSE,
        BSE
    }
}
