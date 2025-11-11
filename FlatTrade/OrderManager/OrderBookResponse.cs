using FlatTrade.Common.JsonConvertors;
using FlatTrade.Common.Types.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FlatTrade.OrderManager
{
    public class OrderBookResponse : BaseErrorMessageResponse
    {
        private string _rejectionReason = string.Empty;

        [JsonProperty("uid")]
        public string UserId { get; set; } = string.Empty;

        [JsonProperty("actid")]
        public string AccountId { get; set; } = string.Empty;

        [JsonProperty("kidid")]
        public int KidId { get; set; }

        [JsonProperty("norenordno")]
        public long NorenOrderNumber { get; set; }

        [JsonProperty("exch")]
        public Exchange Exchange { get; set; }

        [JsonProperty("tsym")]
        public string TradingSymbol { get; set; } = string.Empty;

        [JsonProperty("rejby")]
        public string RejectionBy { get; set; } = string.Empty;

        [JsonProperty("src_uid")]
        public string SourceUid { get; set; } = string.Empty;

        [JsonProperty("cname")]
        public string CompanyName { get; set; } = string.Empty;

        [JsonProperty("qty")]
        public decimal Quantity { get; set; }

        [JsonProperty("fillshares")]
        public long TotalFilled { get; set; }

        [JsonProperty("rorgprc")]
        public decimal RemainingOriginalPriceFromModify { get; set; }

        [JsonProperty("rtrgprc")]
        public decimal RemainingTriggerPrice { get; set; }

        [JsonProperty("rqty")]
        public decimal RemainingQuantity { get; set; }

        [JsonProperty("rorgqty")]
        public long RemaningOriginalQuantityFromModify { get; set; } // used in modify

        [JsonProperty("status")]
        public OrderStatus OrderStatus { get; set; }

        [JsonProperty("st_intrn")]
        public OrderStatus InternalOrderStatus { get; set; }

        [JsonProperty("ipaddr")]
        public string IpAddress { get; set; } = string.Empty;

        [JsonProperty("ordenttm")]
        public long EpochOrderEntryDateTime { get; set; }

        [JsonProperty("trgprc")]
        public decimal TriggerPrice { get; set; }

        [JsonProperty("remarks")]
        public string Remarks { get; set; } = string.Empty;

        [JsonProperty("trantype")]
        public TransactionType TransactionType { get; set; }

        [JsonProperty("prctyp")]
        public PriceType PriceType { get; set; } //LMT/MKT

        [JsonProperty("ret")]
        public RetentionType RetentionType { get; set; } //DAY/IOC/EOS

        [JsonProperty("rejreason")]
        public string RejectionReason
        {
            get => _rejectionReason; // Refactored to refer to the field '_errorMsg'
            set
            {
                base.ErrorMsg = value;
                _rejectionReason = value;
            }
        }

        [JsonProperty("token")]
        [JsonConverter(typeof(LongAsStringConverter))]
        public long Token { get; set; }

        [JsonProperty("mult")]
        public decimal Multiplier { get; set; }

        [JsonProperty("prcftr")]
        public string PriceFactor { get; set; } = string.Empty;

        [JsonProperty("instname")]
        public InstrumentName InstrumentName { get; set; }

        [JsonProperty("ordersource")]
        public AccessType OrderSource { get; set; } //	MOB / WEB / TT	Used to generate exchange info fields.

        [JsonProperty("pp")]
        public int PricePrecision { get; set; }

        [JsonProperty("ti")]
        public decimal TickSize { get; set; }

        [JsonProperty("ls")]
        public decimal LotSize { get; set; }

        [JsonProperty("prc")]
        public decimal Price { get; set; }

        [JsonProperty("avgprc")]
        public decimal AveragePrice { get; set; }

        [JsonProperty("rprc")]
        public decimal RPrice { get; set; }

        [JsonProperty("bpprc")]
        public decimal BookProfitPrice { get; set; }

        [JsonProperty("blprc")]
        public decimal BookLossPrice { get; set; }

        [JsonProperty("rblprc")]
        public decimal RBookLossPrice { get; set; }

        [JsonProperty("trailprc")]
        public decimal TrailingPrice { get; set; }

        [JsonProperty("dscqty")]
        public long DisclosedQuantity { get; set; }

        [JsonProperty("cancelqty")]
        public long CancelledQuantity { get; set; }

        [JsonProperty("sno_fillid")]
        public long SnoFillId { get; set; }

        [JsonProperty("snonum")]
        public long SnoOrderNumber { get; set; }

        [JsonProperty("snoordt")]
        public long SnoOrderDt { get; set; }

        [JsonProperty("brnchid")]
        public string BranchId { get; set; } = string.Empty;

        [JsonProperty("C")]
        public ProductType C { get; set; }

        [JsonProperty("s_prdt_ali")]
        public ProductName ProductDisplayName { get; set; }

        [JsonProperty("prd")]
        public ProductType ProductType { get; set; }

        [JsonProperty("norentm")]
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime NorenTime { get; set; }

        [JsonProperty("exch_tm")]
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime ExchangeTime { get; set; }

        [JsonProperty("algo_id")]
        public long AlgoId { get; set; }

        [JsonProperty("exchordid")]
        public string ExchangeOrderNumber { get; set; } = string.Empty;

        [JsonProperty("amo")]
        public string Amo { get; set; } = string.Empty; // value ="Yes"

        [JsonProperty("mkt_protection")]
        public decimal MarketProtectionPercentage { get; set; }
    }
}
