using Common.JsonConvertors;
using FlatTrade.Types.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FlatTrade.OrderManager
{
    public class SingleOrderHistoryResponse : BaseErrorMessageResponse
    {
        [JsonProperty("exch")]
        public Exchange Exchange { get; set; }

        [JsonProperty("tsym")]
        public string TradingSymbol { get; set; } = string.Empty;

        [JsonProperty("norenordno")]
        public long NorenOrderNumber { get; set; }

        [JsonProperty("st_intrn")]
        public OrderStatus InternalOrderStatus { get; set; }

        [JsonProperty("snoordt")]
        public long SnoOrderDt { get; set; }

        [JsonProperty("snonum")]
        public long SnoOrderNumber { get; set; }

        [JsonProperty("exch_tm")]
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime ExchangeTime { get; set; }

        [JsonProperty("introp_exch")]
        public Exchange OptionalIntropExchange { get; set; }

        [JsonProperty("norentm")]
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime NorenTime { get; set; }

        [JsonProperty("s_prdt_ali")]
        public ProductName ProductDisplayName { get; set; }

        [JsonProperty("flqty")]
        public long FillQuantity { get; set; }

        [JsonProperty("flprc")]
        public decimal FillPrice { get; set; }

        [JsonProperty("flid")]
        public long FillId { get; set; }

        [JsonProperty("qty")]
        public long Quantity { get; set; }

        [JsonProperty("prc")]
        public decimal Price { get; set; }

        [JsonProperty("prd")]
        public ProductType ProductType { get; set; }

        [JsonProperty("kidid")]
        public int KidId { get; set; }

        [JsonProperty("ordersource")]
        public AccessType OrderSource { get; set; } //	MOB / WEB / TT	Used to generate exchange info fields.

        [JsonProperty("rejby")]
        public string RejectionBy { get; set; } = string.Empty;

        [JsonProperty("pan")]
        public string Pan { get; set; } = string.Empty;

        [JsonProperty("src_uid")]
        public string SourceUid { get; set; } = string.Empty;

        [JsonProperty("status")]
        public string OrderStatus { get; set; } = string.Empty;

        [JsonProperty("rpt")]
        public string ReportType { get; set; } = string.Empty;

        [JsonProperty("trantype")]
        public required TransactionType TransactionType { get; set; }

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
        public long DisclosedQuantity { get; set; }

        [JsonProperty("trgprc")]
        public decimal TriggerPrice { get; set; }

        [JsonProperty("ret")]
        public required RetentionType RetentionType { get; set; } //DAY/IOC/EOS

        [JsonProperty("uid")]
        public string UserId { get; set; } = string.Empty;

        [JsonProperty("actid")]
        public string AccountId { get; set; } = string.Empty;

        [JsonProperty("bpprc")]
        public decimal BookProfitPrice { get; set; }

        [JsonProperty("blprc")]
        public decimal BookLossProfit { get; set; }

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

        [JsonProperty("token")]
        [JsonConverter(typeof(LongAsStringConverter))]
        public long Token { get; set; }

        [JsonProperty("orddtm")]
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime OrderDateTime { get; set; }

        [JsonProperty("ordenttm")]
        //[JsonConverter(typeof(IsoDateTimeConverter))]
        public long EpochOrderEntryDateTime { get; set; }

        [JsonProperty("extm")]
        public string Extm { get; set; } = string.Empty;
    }
}
