using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace FlatTrade.Types.Base
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ChartInterval
    {
        [EnumMember(Value = "1")]
        One = 1,
        [EnumMember(Value = "3")]
        Three = 3,
        [EnumMember(Value = "5")]
        Five = 5,
        [EnumMember(Value = "10")]
        Ten = 10,
        [EnumMember(Value = "15")]
        Fifteen = 15,
        [EnumMember(Value = "30")]
        Thirty = 30,
        [EnumMember(Value = "60")]
        Sixty = 60,
        [EnumMember(Value = "120")]
        OneHundredTwenty = 120,
        [EnumMember(Value = "1440")]
        Daily = 1440,
    }
}
