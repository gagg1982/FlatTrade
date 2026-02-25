using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace FlatTrade.Types.Base
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ProductName
    {
        [EnumMember(Value = "NRML")]
        Normal,
        [EnumMember(Value = "MIS")]
        MargingIntraDaySquareOff,
        [EnumMember(Value = "CO")]
        CoveredOrder,
        [EnumMember(Value = "BO")]
        BracketOrder,
        [EnumMember(Value = "CNC")]
        CashAndCarry
    }
}
