using Common.JsonConvertors;
using Newtonsoft.Json;

namespace FlatTrade.MarketInfoManager
{
    public class EodChartDataRequest
    {
        [JsonProperty("sym")]
        public required string SymbolName { get; set; }

        [JsonProperty("from")]
        [JsonConverter(typeof(LongAsStringConverter))]
        public required long EpochStartDateTime { get; set; }

        [JsonProperty("to")]
        [JsonConverter(typeof(LongAsStringConverter))]
        public required long EpochEndDateTime { get; set; }
    }
}
