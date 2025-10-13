using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FlatTrade.MarketInfoManager
{
    public class EodChartDataResponse : BaseErrorMessageResponse
    {
        [JsonProperty("time")]
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime StartDateTime { get; set; }

        [JsonProperty("into")]
        public decimal OpenPrice { get; set; }

        [JsonProperty("inth")]
        public decimal HighPrice { get; set; }

        [JsonProperty("intl")]
        public decimal LowPrice { get; set; }

        [JsonProperty("intc")]
        public decimal ClosePrice { get; set; }

        [JsonProperty("intv")]
        public decimal Volume { get; set; }

        [JsonProperty("ssboe")]
        public long EpochStartDateTime { get; set; }

    }
}
