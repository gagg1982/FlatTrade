using FlatTrade.Common.JsonConvertors;
using FlatTrade.Common.Types.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FlatTrade.ScripManager
{
    public class SecurityInfoResponse : BaseErrorMessageResponse
    {        
        [JsonProperty("exch")]
        public Exchange Exchange { get; set; }

        [JsonProperty("tsym")]
        public string TradingSymbol { get; set; } = string.Empty;

        [JsonProperty("cname")]
        public string CompanyName { get; set; } = string.Empty;

        [JsonProperty("instname")]
        [JsonConverter(typeof(StringEnumConverter))]
        public InstrumentName InstrumentName { get; set; }

        [JsonProperty("symname")]
        public string SymbolName { get; set; } = string.Empty;

        [JsonProperty("seg")]
        public string Segment { get; set; } = string.Empty;

        [JsonProperty("ord_msg")]
        public string Message { get; set; } = string.Empty;

        [JsonProperty("gsmind")]
        public string GsmIndicator { get; set; } = string.Empty;

        [JsonProperty("pp")]
        public int PricePrecision { get; set; }

        [JsonProperty("ti")]
        public decimal TickSize { get; set; }

        [JsonProperty("ls")]
        public decimal LotSize { get; set; }

        [JsonProperty("isin")]
        public string Isin { get; set; } = string.Empty;

        [JsonProperty("mult")]
        public decimal Multiplier { get; set; }

        [JsonProperty("issue_d")]
        [JsonConverter(typeof(DateOnlyAsStringConverter))]
        public DateOnly IssueDate { get; set; }

        [JsonProperty("listing_d")]
        [JsonConverter(typeof(DateOnlyAsStringConverter))]
        public DateOnly ListingDate { get; set; }

        [JsonProperty("uc")]
        public decimal UpperCircuitLimit { get; set; }

        [JsonProperty("lc")]
        public decimal LowerCircuitLimit { get; set; }

        [JsonProperty("token")]
        public long Token { get; set; }

        [JsonProperty("prcftr")]
        public string PriceFactor { get; set; } = string.Empty;

        [JsonProperty("prcftr_d")]
        public string PriceFactorD { get; set; } = string.Empty;

        [JsonProperty("nontrd")]
        public string NonTradeable { get; set; } = string.Empty;

        [JsonProperty("delmrg")]
        public decimal DelMargin { get; set; }

        [JsonProperty("varmrg")]
        public decimal VarMargin { get; set; }

        [JsonProperty("elmmrg")]
        public decimal ElmMargin { get; set; }

        [JsonProperty("frzqty")]
        public long FreezeQuantity { get; set; }
    }
}
