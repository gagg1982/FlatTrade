using Common.JsonConvertors;
using FlatTrade.Types.Base;
using Newtonsoft.Json;

namespace FlatTrade.ScripManager
{
    public class LinkedFutureResponse
    {
        [JsonProperty("exch")]
        public Exchange Exchange { get; set; }

        [JsonProperty("tsym")]
        public string TradingSymbol { get; set; } = string.Empty;

        [JsonProperty("token")]
        public string Token { get; set; } = string.Empty;

        [JsonProperty("pp")]
        public int PricePrecision { get; set; }

        [JsonProperty("ls")]
        public decimal LotSize { get; set; }

        [JsonProperty("ti")]
        public decimal TickSize { get; set; }

        [JsonProperty("mult")]
        public decimal Multiplier { get; set; }

        [JsonProperty("exd")]
        [JsonConverter(typeof(DateOnlyAsStringConverterDdMonYyyy))]
        public DateOnly Expirydate { get; set; }
    }
}
