using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;


namespace FlatTrade.Common.Types.Base
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OrderStatus
    {
        [EnumMember(Value = "OPEN")]
        Open,
        [EnumMember(Value = "AMO OPEN")]
        AmoOpen,
        [EnumMember(Value = "PENDING")]
        Pending,
        [EnumMember(Value = "COMPLETE")]
        Completed,
        [EnumMember(Value = "REJECTED")]
        Rejected,
        [EnumMember(Value = "CANCELED")]
        Cancelled,
        [EnumMember(Value = "TRIGGER_PENDING")]
        TriggerPending,
        [EnumMember(Value = "AMO CANCELED")]
        AmoCancelled,
        [EnumMember(Value = "MODIFY PENDING")]
        ModifyPending,
        [EnumMember(Value = "MODIFY ACK")]
        ModifyAck,
        [EnumMember(Value = "ORDER PENDING")]
        OrderPending,
        [EnumMember(Value = "ORDER ACK")]
        OrderAck,
        [EnumMember(Value = "CANCEL PENDING")]
        CancelPending,
        [EnumMember(Value = "CANCEL ACK")]
        CancelAck,
    }
}
