using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Upstox.Types.Base
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ChartInterval
    {
        [EnumMember(Value = "minutes")]
        One = 1,
        [EnumMember(Value = "days")]
        Daily = 1440,
    }
}
