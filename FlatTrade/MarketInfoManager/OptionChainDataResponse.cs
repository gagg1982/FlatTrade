using FlatTrade.Common.Types.Base;
using Newtonsoft.Json;

namespace FlatTrade.MarketInfoManager
{
    public class OptionChainDataResponse
    {
        [JsonProperty("tsym")]
        public string TradingSymbol { get; set; } = string.Empty;

        [JsonProperty("exch")]
        public Exchange Exchange { get; set; }

        [JsonProperty("token")]
        public long Token { get; set; }

        [JsonProperty("optt")]
        public OptionType OptionType { get; set; }

        [JsonProperty("strprc")]
        public decimal StrikePrice { get; set; }

        [JsonProperty("ls")]
        public decimal LotSize { get; set; }

        [JsonProperty("pp")]
        public int PricePrecision { get; set; }

        [JsonProperty("ti")]
        public decimal TickSize { get; set; }

    }
}
