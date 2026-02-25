using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace FlatTrade.Types.Base
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PriceType
    {
        [EnumMember(Value = "LMT")]
        Limit,

        [EnumMember(Value = "MKT")]
        Market,

        [EnumMember(Value = "SL-LMT")]
        StopLossLimit,

        [EnumMember(Value = "SL-MKT")]
        StopLossMarket,
    }
}
