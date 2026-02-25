using FlatTrade.Types.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FlatTrade.OrderManager
{
    public class PendingGttOrderResponse : BaseErrorMessageResponse
    {
        [JsonProperty("actid")]
        public string AccountId { get; set; } = string.Empty;

        [JsonProperty("brkname")]
        public string BrokerName { get; set; } = string.Empty;

        [JsonProperty("pp")]
        public int PricePrecision { get; set; }

        [JsonProperty("ti")]
        public decimal TickSize { get; set; }

        [JsonProperty("ls")]
        public decimal LotSize { get; set; }

        [JsonProperty("exch")]
        public Exchange Exchange { get; set; }

        [JsonProperty("tsym")]
        public string TradingSymbol { get; set; } = string.Empty;

        [JsonProperty("remarks")]
        public string Remarks { get; set; } = string.Empty;

        [JsonProperty("token")]
        public string Token { get; set; } = string.Empty;

        [JsonProperty("al_id")]
        public long AlertId { get; set; }

        [JsonProperty("upd_tm")]
        public long EpochUpdateTime { get; set; }

        [JsonProperty("norentm")]
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime NorenDateTime { get; set; }

        [JsonProperty("ai_t")]
        public GTTAlertType AlertType { get; set; }

        [JsonProperty("validity")]
        public RetentionType Validity { get; set; }

        [JsonProperty("d")]
        public decimal DataToBeComparedWithLTP { get; set; }

        [JsonProperty("prd")]
        public ProductType ProductType { get; set; }

        [JsonProperty("C")]
        public ProductType C { get; set; }

        [JsonProperty("prctyp")]
        public PriceType PriceType { get; set; } //LMT/MKT/SL-LMT/SL-MKT/DS/2L/3L

        [JsonProperty("trantype")]
        public TransactionType TransactionType { get; set; }

        [JsonProperty("ret")]
        public RetentionType RetentionType { get; set; } //DAY/IOC/EOS

        [JsonProperty("qty")]
        public long Quantity { get; set; }

        [JsonProperty("prc")]
        public required decimal Price { get; set; }

        [JsonProperty("oivariable")]
        public IEnumerable<OIVariableRequest> OiVariable { get; set; } = [];

        [JsonProperty("place_order_params")]
        public PlaceOrderParameterResponse OrderParameters { get; set; } = new();

        [JsonProperty("place_order_params_leg2")]
        public PlaceOrderParameterResponse? OrderParametersLeg2 { get; set; }
    }
}
