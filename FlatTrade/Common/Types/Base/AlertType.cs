using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace FlatTrade.Common.Types.Base
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AlertType
    {
        [EnumMember(Value = "LTP_A")]
        LtpGreaterThan,
        [EnumMember(Value = "LTP_B")]
        LtpLesserThan,

        [EnumMember(Value = "CH_PER_A")]
        PercentageChangeGreaterThan,
        [EnumMember(Value = "CH_PER_B")]
        PercentageChangeLesserThan,

        [EnumMember(Value = "ATP_A")]
        AverageTradePriceGreaterThan,
        [EnumMember(Value = "ATP_B")]
        AverageTradePriceLesserThan,

        [EnumMember(Value = "OI_A")]
        OIGreaterThan,
        [EnumMember(Value = "OI_B")]
        OILesserThan,

        [EnumMember(Value = "TOI_A")]
        TOIGreaterThan,
        [EnumMember(Value = "TOI_B")]
        TOILesserThan,

        [EnumMember(Value = "VOLUME_A")]
        VolumeGreaterThan,

        [EnumMember(Value = "LTP_A_52HIGH")]
        FiftyTwoWeekHigh,
        [EnumMember(Value = "LTP_B_52LOW")]
        FiftyTwoWeekLow

    }
}
