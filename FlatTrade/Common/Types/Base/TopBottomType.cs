using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace FlatTrade.Common.Types.Base
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TopBottomType
    {
        [EnumMember(Value = "T")]
        Top,  // top
        [EnumMember(Value = "B")]
        Bottom   // bottom
    }
}
