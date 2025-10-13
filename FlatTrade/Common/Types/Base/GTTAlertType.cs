using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace FlatTrade.Common.Types.Base
{

    [JsonConverter(typeof(StringEnumConverter))]
    public enum GTTAlertType
    {
        [EnumMember(Value = "LMT_BOS_O")]
        LimitBracketOrder,

        [EnumMember(Value = "LTP_A_O")]
        LtpGreaterThan,
        [EnumMember(Value = "LTP_B_O")]
        LtpLesserThan,

        [EnumMember(Value = "CH_PER_A_O")]
        PercentageChangeGreaterThan,
        [EnumMember(Value = "CH_PER_B_O")]
        PercentageChangeLesserThan,

        [EnumMember(Value = "ATP_A_O")]
        AverageTradePriceGreaterThan,
        [EnumMember(Value = "ATP_B_O")]
        AverageTradePriceLesserThan,

        [EnumMember(Value = "OI_A_O")]
        OIGreaterThan,
        [EnumMember(Value = "OI_B_O")]
        OILesserThan,

        [EnumMember(Value = "TOI_A_O")]
        TOIGreaterThan,
        [EnumMember(Value = "TOI_B_O")]
        TOILesserThan,

        [EnumMember(Value = "VOLUME_A_O")]
        VolumeGreaterThan,

        [EnumMember(Value = "LTP_A_52HIGH")]
        FiftyTwoWeekHigh,
        [EnumMember(Value = "LTP_B_52LOW")]
        FiftyTwoWeekLow
    }
}
