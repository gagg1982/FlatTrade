using Common.JsonConvertors;
using FlatTrade.Types.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FlatTrade.SubscriptionManager.Order
{
    public class OrderSubscriptionUpdates : BaseSubscriptionRequest
    {
        [JsonProperty("norenordno")]
        public long NorenOrderNumber { get; set; }

        [JsonProperty("uid")]
        public string UserId { get; set; } = string.Empty;

        [JsonProperty("actid")]
        public string AccountId { get; set; } = string.Empty;

        [JsonProperty("exch")]
        public Exchange Exchange { get; set; }

        [JsonProperty("cancelqty")]
        public long CancelledQuantity { get; set; }

        [JsonProperty("tsym")]
        public string TradingSymbol { get; set; } = string.Empty;

        [JsonProperty("trantype")]
        public TransactionType TransactionType { get; set; }

        [JsonProperty("qty")]
        public long Quantity { get; set; }

        [JsonProperty("trgprc")]
        public decimal TriggerPrice { get; set; }

        [JsonProperty("prc")]
        public decimal Price { get; set; }

        [JsonProperty("pcode")]
        public ProductType ProductType { get; set; }

        [JsonProperty("remarks")]
        public string Remarks { get; set; } = string.Empty;

        [JsonProperty("rejreason")]
        public string RejectionReason { get; set; } = string.Empty;

        [JsonProperty("status")]
        public OrderStatus OrderStatus { get; set; }

        [JsonProperty("reporttype")]
        public string ReportType { get; set; } = string.Empty;

        [JsonProperty("prctyp")]
        public PriceType PriceType { get; set; }  //LMT/MKT

        [JsonProperty("trailprc")]
        public decimal TrailingPrice { get; set; }

        [JsonProperty("ret")]
        public RetentionType RetentionType { get; set; } //DAY/IOC/EOS

        [JsonProperty("exchordid")]
        public string ExchangeOrderNumber { get; set; } = string.Empty;

        [JsonProperty("dscqty")]
        public long DisclosedQuantity { get; set; }

        [JsonProperty("flqty")]
        public long FillQuantity { get; set; }

        [JsonProperty("flprc")]
        public decimal FillPrice { get; set; }

        [JsonProperty("flid")]
        public long FillId { get; set; }

        [JsonProperty("fltm")]
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime FillDateTime { get; set; }

        [JsonProperty("avgprc")]
        public decimal AvgPriceOfTradedQuantity { get; set; }

        [JsonProperty("exch_tm")]
        [JsonConverter(typeof(CustomIsoDateTimeConverter))]
        public DateTime ExchangeTime { get; set; }

        [JsonProperty("amo")]
        public string Amo { get; set; } = string.Empty;

        [JsonProperty("bpprc")]
        public decimal BookProfitPrice { get; set; }

        [JsonProperty("blprc")]
        public decimal BookLossPrice { get; set; }

        [JsonProperty("fillshares")]
        [JsonConverter(typeof(LongAsStringConverter))]
        public long TotalFilled { get; set; }
    }
}
