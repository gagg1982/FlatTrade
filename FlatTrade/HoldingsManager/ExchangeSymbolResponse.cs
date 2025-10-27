using FlatTrade.Common.Types.Base;
using Newtonsoft.Json;

namespace FlatTrade.HoldingsManager
{
    public class ExchangeSymbolResponse
    {
        [JsonProperty("exch")]
        public Exchange Exchange { get; set; }

        [JsonProperty("tsym")]
        public string TradingSymbol { get; set; } = string.Empty;

        [JsonProperty("token")]
        public long Token { get; set; }

        [JsonProperty("pp")]
        public int PricePrecision { get; set; }

        [JsonProperty("ti")]
        public decimal TickSize { get; set; }

        [JsonProperty("ls")]
        public decimal LotSize { get; set; }

        [JsonProperty("isin")]
        public string Isin { get; set; } = string.Empty;

        [JsonProperty("cname")]
        public string CompanyName { get; set; } = string.Empty;

        [JsonProperty("cm_e")]
        public string CashMarketEquity { get; set; } = string.Empty;

        [JsonProperty("fo_e")]
        public string FutureOptionEquity { get; set; } = string.Empty;

        [JsonProperty("cur_e")]
        public string CurrencyEquity { get; set; } = string.Empty;

        public ExchangeSymbolResponse ShallowCopy()
        {
            return (ExchangeSymbolResponse)this.MemberwiseClone();
        }
    }
}
