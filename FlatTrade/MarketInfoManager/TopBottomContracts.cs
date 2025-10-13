using FlatTrade.Common.Types.Base;
using Newtonsoft.Json;

namespace FlatTrade.MarketInfoManager
{
    public class TopBottomContracts
    {
        [JsonProperty("tsym")]
        public string TradingSymbol { get; set; } = string.Empty;

        [JsonProperty("token")]
        public long Token { get; set; }

        [JsonProperty("exch")]
        public Exchange Exchange { get; set; }

        [JsonProperty("pp")]
        public int PricePrecision { get; set; }

        [JsonProperty("ti")]
        public decimal TickSize { get; set; }

        [JsonProperty("ls")]
        public decimal LotSize { get; set; }

        [JsonProperty("lp")]
        public decimal LastTradePrice { get; set; }

        [JsonProperty("c")]
        public decimal PreviousClosePrice { get; set; }

        [JsonProperty("v")]
        public string Volume { get; set; } = string.Empty;

        [JsonProperty("value")]
        public decimal TotalTradedValue { get; set; }

        [JsonProperty("oi")]
        public long OpenInterest { get; set; }

        [JsonProperty("pc")]
        public decimal LastTradePricePercentageChange { get; set; }
    }
}
