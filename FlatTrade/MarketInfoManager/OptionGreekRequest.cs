using Common.JsonConvertors;
using FlatTrade.Types.Base;
using Newtonsoft.Json;

namespace FlatTrade.MarketInfoManager
{
    public class OptionGreekRequest
    {
        [JsonProperty("exd")]
        [JsonConverter(typeof(DateOnlyAsStringConverter))]
        public required DateOnly ExpiryDate { get; set; }

        [JsonProperty("strprc")]
        [JsonConverter(typeof(DecimalAsStringConverter))]
        public required decimal StrikePrice { get; set; }

        [JsonProperty("sptprc")]
        [JsonConverter(typeof(DecimalAsStringConverter))]
        public required decimal SpotPrice { get; set; }

        [JsonProperty("int_rate")]
        [JsonConverter(typeof(DecimalAsStringConverter))]
        public required decimal InterestRate { get; set; }

        [JsonProperty("volatility")]
        [JsonConverter(typeof(DecimalAsStringConverter))]
        public required decimal Volaitility { get; set; }

        [JsonProperty("optt")]
        public required OptionType OptionType { get; set; }
    }
}
