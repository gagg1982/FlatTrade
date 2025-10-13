using FlatTrade.Common.Types.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FlatTrade.ScripManager
{
    public class QuotesResponse : BaseErrorMessageResponse
    {
        [JsonProperty("exch")]
        public Exchange Exchange { get; set; }


        [JsonProperty("tsym")]
        public string TradingSymbol { get; set; } = string.Empty;


        [JsonProperty("cname")]
        public string CompanyName { get; set; } = string.Empty;


        [JsonProperty("symname")]
        public string SymbolName { get; set; } = string.Empty;


        [JsonProperty("seg")]
        public string Segment { get; set; } = string.Empty;

        [JsonProperty("instname")]
        public InstrumentName InstrumentName { get; set; }

        [JsonProperty("isin")]
        public string Isin { get; set; } = string.Empty;

        [JsonProperty("pp")]
        public int PricePrecision { get; set; }

        [JsonProperty("ls")]
        public decimal LotSize { get; set; }

        [JsonProperty("ti")]
        public decimal TickSize { get; set; }

        [JsonProperty("mult")]
        public decimal Multiplier { get; set; }

        [JsonProperty("lut")]
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime LastUpdateTime { get; set; }

        [JsonProperty("uc")]
        public decimal UpperCircuitLimit { get; set; }

        [JsonProperty("lc")]
        public decimal LowerCircuitLimit { get; set; }

        [JsonProperty("toi")]
        public long IntervalIoChange { get; set; }

        [JsonProperty("cutof_all")]
        public bool CutOffAll { get; set; }

        [JsonProperty("prcftr_d")]
        public string PriceFactor { get; set; } = string.Empty;

        [JsonProperty("token")]
        public long Token { get; set; }

        [JsonProperty("c")]
        public decimal DailyClose { get; set; }

        [JsonProperty("o")]
        public decimal DailyOpen { get; set; }

        [JsonProperty("h")]
        public decimal DailyHighPrice { get; set; }

        [JsonProperty("l")]
        public decimal DailyLowPrice { get; set; }

        [JsonProperty("v")]
        public long Volume { get; set; }

        [JsonProperty("ltq")]
        public long LastTradeQuantity { get; set; }

        [JsonProperty("lp")]
        public decimal LastTradePrice { get; set; }

        [JsonProperty("ap")]
        public decimal AverageTradePrice { get; set; }

        [JsonProperty("ltt")]
        public string LastTradeTime { get; set; } = string.Empty;

        [JsonProperty("ltd")]
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime LastTradeDate { get; set; }

        public DateTime LastTradeDateTime
        {
            get { return LastTradeDate.Date + TimeSpan.Parse(string.IsNullOrEmpty(LastTradeTime) ? "00:00:00" : LastTradeTime); }
        }

        [JsonConverter(typeof(QuotesResponseJsonConvertor))]
        public List<MarketDepthLevel> BestBids { get; set; } = [];

        [JsonConverter(typeof(QuotesResponseJsonConvertor))]
        public List<MarketDepthLevel> BestAsks { get; set; } = [];

        [JsonProperty("und_exch")]
        public string UnderlyingExchangeSegment { get; set; } = string.Empty;

        [JsonProperty("und_tk")]
        public long UnderlyingToken { get; set; }

        [JsonProperty("ord_msg")]
        public string OrderMessage { get; set; } = string.Empty;

        [JsonProperty("sptprc")]
        public decimal StopPrice { get; set; }

        [JsonProperty("issuecap")]
        public decimal IssueCapital { get; set; }

        [JsonProperty("e_date")]
        public DateTime EndDate { get; set; }

        [JsonProperty("wk52_h")]
        public decimal Wk52High { get; set; }

        [JsonProperty("wk52_l")]
        public decimal Wk52Low { get; set; }

        [JsonProperty("tsq")]
        public decimal TotalSellQuantity { get; set; }

        [JsonProperty("tbq")]
        public decimal TotalBuyQuantity { get; set; }

        //public QuotesResponse()
        //{
        //    var isoDateTimeConverter = new IsoDateTimeConverter
        //    {
        //        DateTimeFormat = "HH:mm:ss dd-MM-yyyy",
        //        Culture = CultureInfo.InvariantCulture
        //    };
        //}
    }
}
