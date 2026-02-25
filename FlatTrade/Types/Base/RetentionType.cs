using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace FlatTrade.Types.Base
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RetentionType
    {
        [EnumMember(Value = "DAY")]
        DAY,
        [EnumMember(Value = "IOC")]
        IOC,
        EOS,
        GTT, // Good Till Triggered, used for GTT orders
        GTC, // Good Till Cancelled, used for OCO orders
        GTD, // Good Till Date, used for orders that are valid until a specific date
        GTM  // Good Till Modified, used for orders that can be modified before execution        
    }
}
