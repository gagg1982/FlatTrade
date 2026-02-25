using FlatTrade.Types.Base;
using Newtonsoft.Json;

namespace FlatTrade.ScripManager
{
    public class LinkedEquityResponse
    {
        [JsonProperty("exch")]
        public Exchange Exchange { get; set; }

        [JsonProperty("tsym")]
        public string TradingSymbol { get; set; } = string.Empty;

        [JsonProperty("token")]
        public long Token { get; set; }

        [JsonProperty("pp")]
        public int PricePrecision { get; set; }

        [JsonProperty("ls")]
        public decimal LotSize { get; set; }

        [JsonProperty("ti")]
        public decimal TickSize { get; set; }

        [JsonProperty("mult")]
        public decimal Multiplier { get; set; }
    }
}
