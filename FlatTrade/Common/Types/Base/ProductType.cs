using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace FlatTrade.Common.Types.Base
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ProductType
    {
        [EnumMember(Value = "C")]
        Delivery,  // delivery
        [EnumMember(Value = "I")]
        IntraDay,  //intraday
        [EnumMember(Value = "M")]
        Normal,    //NRML
        [EnumMember(Value = "H")]
        HighLeverage,   // high leverage
        [EnumMember(Value = "B")]
        BracketOrder,   // Bracket order
        [EnumMember(Value = "MTF")]
        MargingTradeFacility    // margin trade facility
    }
}
