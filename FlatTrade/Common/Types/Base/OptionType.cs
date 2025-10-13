using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace FlatTrade.Common.Types.Base
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OptionType
    {
        [EnumMember(Value = "CE")]
        Call_European,

        [EnumMember(Value = "PE")]
        Put_European,

    }
}
