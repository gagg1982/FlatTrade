using FlatTrade.Types.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FlatTrade.OrderManager
{
    public class MultiLegOrderBookResponse : BaseErrorMessageResponse
    {
        [JsonProperty("exch")]
        [JsonConverter(typeof(StringEnumConverter))]
        public Exchange Exchange { get; set; }

        [JsonProperty("tsym")]
        public string TradingSymbol { get; set; } = string.Empty;

        [JsonProperty("norenordno")]
        public long NorenOrderNumber { get; set; }

        [JsonProperty("qty")]
        public long Quantity { get; set; }

        [JsonProperty("prc")]
        public decimal Price { get; set; }

        [JsonProperty("prd")]
        public ProductType ProductType { get; set; }

        [JsonProperty("status")]
        public OrderStatus OrderStatus { get; set; }

        [JsonProperty("trantype")]
        public TransactionType TransactionType { get; set; }

        [JsonProperty("prctyp")]
        public PriceType PriceType { get; set; }  //LMT/MKT

        [JsonProperty("fillshares")]
        public long TotalFilled { get; set; }

        [JsonProperty("avgprc")]
        public decimal AvgPriceOfTradedQuantity { get; set; }

        [JsonProperty("rejreason")]
        public string RejectionReason { get; set; } = string.Empty;

        [JsonProperty("exchordid")]
        public string ExchangeOrderNumber { get; set; } = string.Empty;

        [JsonProperty("cancelqty")]
        public long CancelledQuantity { get; set; }

        [JsonProperty("remarks")]
        public string Remarks { get; set; } = string.Empty;

        [JsonProperty("dscqty")]
        public required long DisclosedQuantity { get; set; }

        [JsonProperty("trgprc")]
        public decimal TriggerPrice { get; set; }

        [JsonProperty("ret")]
        public required RetentionType RetentionType { get; set; }  //DAY/IOC/EOS

        [JsonProperty("uid")]
        public string UserId { get; set; } = string.Empty;

        [JsonProperty("actid")]
        public string AccountId { get; set; } = string.Empty;

        [JsonProperty("bpprc")]
        public decimal BookProfitPrice { get; set; }

        [JsonProperty("blprc")]
        public decimal BookLossPrice { get; set; }

        [JsonProperty("trailprc")]
        public decimal TrailingPrice { get; set; }

        [JsonProperty("amo")]
        public string Amo { get; set; } = string.Empty; // value ="Yes"

        [JsonProperty("pp")]
        public int PricePrecision { get; set; }

        [JsonProperty("ti")]
        public decimal TickSize { get; set; }

        [JsonProperty("ls")]
        public decimal LotSize { get; set; }

        [JsonProperty("tsym2")]
        public string Symbol2 { get; set; } = string.Empty;

        [JsonProperty("qty2")]
        public long Quantity2 { get; set; }

        [JsonProperty("prc2")]
        public decimal Price2 { get; set; }

        [JsonProperty("trantype2")]
        public TransactionType TransactionType2 { get; set; }

        [JsonProperty("tsym3")]
        public string Symbol3 { get; set; } = string.Empty;

        [JsonProperty("qty3")]
        public long Quantity3 { get; set; }

        [JsonProperty("prc3")]
        public decimal Price3 { get; set; }

        [JsonProperty("trantype3")]
        public TransactionType TransactionType3 { get; set; }

        [JsonProperty("fillshares2")]
        public long TotalFilled2 { get; set; }

        [JsonProperty("avgprc2")]
        public decimal AvgPriceOfTradedQuantity2 { get; set; }

        [JsonProperty("fillshares3")]
        public long TotalFilled3 { get; set; }

        [JsonProperty("avgprc3")]
        public decimal AvgPriceOfTradedQuantity3 { get; set; }

    }
}
