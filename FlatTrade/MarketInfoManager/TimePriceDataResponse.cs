using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FlatTrade.MarketInfoManager
{
    public class TimePriceDataResponse : BaseErrorMessageResponse
    {
        [JsonProperty("time")]
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime StartDateTime { get; set; }

        [JsonProperty("intvwap")]
        public decimal IntervalVWap { get; set; }

        [JsonProperty("into")]
        public decimal OpenPrice { get; set; }

        [JsonProperty("ssboe")]
        public long EpochStartDateTime { get; set; }

        [JsonProperty("inth")]
        public decimal HighPrice { get; set; }


        [JsonProperty("intl")]
        public decimal LowPrice { get; set; }

        [JsonProperty("intc")]
        public decimal ClosePrice { get; set; }

        [JsonProperty("intv")]
        public decimal Volume { get; set; }

        [JsonProperty("v")]
        public decimal TotalVolume { get; set; }

        [JsonProperty("intoi")]
        public decimal OpenInterest { get; set; }

        [JsonProperty("oi")]
        public decimal TotalOpenInterest { get; set; }
    }
}
