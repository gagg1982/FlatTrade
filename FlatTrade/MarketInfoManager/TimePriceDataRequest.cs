using Common.JsonConvertors;
using FlatTrade.Types.Base;
using Newtonsoft.Json;

namespace FlatTrade.MarketInfoManager
{
    public class TimePriceDataRequest
    {
        [JsonProperty("uid")]
        public required string UserId { get; set; }

        [JsonProperty("exch")]
        public required Exchange Exchange { get; set; }

        [JsonProperty("token")]
        public required string TradingSymbol { get; set; }

        [JsonProperty("st")]
        [JsonConverter(typeof(LongAsStringConverter))]
        public required long EpochStartDateTime { get; set; }

        [JsonProperty("et")]
        [JsonConverter(typeof(LongAsStringConverter))]
        public required long EpochEndDateTime { get; set; }

        [JsonProperty("intrv")]
        public required ChartInterval Interval { get; set; }  //1,3,5,10,15,30,60,120
    }
}
