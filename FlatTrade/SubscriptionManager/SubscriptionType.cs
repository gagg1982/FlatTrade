using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace FlatTrade.SubscriptionManager
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SubscriptionType
    {
        [EnumMember(Value = "c")]
        Connect,
        [EnumMember(Value = "ck")]
        ConnectAck,

        [EnumMember(Value = "am")]
        SubscribeAlertMessages,

        [EnumMember(Value = "o")]
        SubscribeOrder,

        [EnumMember(Value = "ok")]
        SubscribeOrderAck,

        [EnumMember(Value = "om")]
        SubscribeOrderUpdate,

        [EnumMember(Value = "uo")]
        UnsubscribeOrder,

        [EnumMember(Value = "uok")]
        UnsubscribeOrderAck,

        [EnumMember(Value = "d")]
        SubscribeQuote,

        [EnumMember(Value = "dk")]
        SubscribeQuoteAck,

        [EnumMember(Value = "df")]
        SubscribeQuoteUpdates,

        [EnumMember(Value = "ud")]
        UnsubscribeQuote,

        [EnumMember(Value = "udk")]
        UnsubscribeQuoteAck,

        [EnumMember(Value = "t")]
        SubscribeTouchLine,

        [EnumMember(Value = "tk")]
        SubscribeTouchLineAck,

        [EnumMember(Value = "tf")]
        SubscribeTouchLineUpdates,

        [EnumMember(Value = "u")]
        UnsubscribeTouchLine,

        [EnumMember(Value = "uk")]
        UnsubscribeTouchLineAck,
    }
}
