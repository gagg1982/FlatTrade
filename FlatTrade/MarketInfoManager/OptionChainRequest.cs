using FlatTrade.Common.JsonConvertors;
using FlatTrade.Common.Types.Base;
using Newtonsoft.Json;

namespace FlatTrade.MarketInfoManager
{
    public class OptionChainRequest
    {
        [JsonProperty("uid")]
        public required string UserId { get; set; }

        [JsonProperty("tsym")]
        public required string TradingSymbol { get; set; }

        [JsonProperty("exch")]
        public required Exchange Exchange { get; set; }

        [JsonProperty("strprc")]
        [JsonConverter(typeof(DecimalAsStringConverter))]
        public required decimal StrikePrice { get; set; }

        [JsonProperty("cnt")]
        [JsonConverter(typeof(LongAsStringConverter))]
        public required long OneSideCountForPutAndCall { get; set; } = 10;
    }
}
