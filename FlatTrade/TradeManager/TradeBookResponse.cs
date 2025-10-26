using FlatTrade.Common.Types.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using static FlatTrade.Common.Helpers.DataReaderHelper;

namespace FlatTrade.TradeManager
{
    public class TradeBookResponse : BaseErrorMessageResponse
    {
        [JsonProperty("exch")]
        [Transform(typeof(EnumTransformer<Exchange>))]
        public Exchange Exchange { get; set; }

        [JsonProperty("tsym")]
        public string TradingSymbol { get; set; } = string.Empty;

        [JsonProperty("snonum")]        
        public long SnoOrderNumber { get; set; }

        [JsonProperty("remarks")]
        public string Remarks { get; set; } = string.Empty;

        [JsonProperty("snoordt")]
        public long SnoOrderDt { get; set; }

        [JsonProperty("norenordno")]
        public long NorenOrderNumber { get; set; }

        [JsonProperty("uid")]
        public string UserId { get; set; } = string.Empty;

        [JsonProperty("actid")]
        public string AccountId { get; set; } = string.Empty;

        [JsonProperty("prctyp")]
        [Transform(typeof(EnumTransformer<PriceType>))]
        public PriceType PriceType { get; set; }  //LMT/MKT

        [JsonProperty("ret")]
        [Transform(typeof(EnumTransformer<RetentionType>))]
        public RetentionType RetentionType { get; set; }  //DAY/IOC/EOS

        [JsonProperty("s_prdt_ali")]
        [Transform(typeof(EnumTransformer<ProductName>))]
        public ProductName ProductDisplayName { get; set; }

        [JsonProperty("prd")]
        [Transform(typeof(EnumTransformer<ProductType>))]
        public ProductType ProductType { get; set; }

        [JsonProperty("fltm")]
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime FillDateTime { get; set; }

        [JsonProperty("flid")]
        public long FillId { get; set; } 

        [JsonProperty("trantype")]
        [Transform(typeof(EnumTransformer<TransactionType>))]
        public TransactionType TransactionType { get; set; }

        [JsonProperty("qty")]
        public long Quantity { get; set; }

        [JsonProperty("token")]
        public long Token { get; set; }

        [JsonProperty("fillshares")]
        public long TotalFilled { get; set; }

        [JsonProperty("flqty")]
        public long FillQuantity { get; set; }

        [JsonProperty("mult")]
        public decimal Multiplier { get; set; }

        [JsonProperty("pp")]
        public int PricePrecision { get; set; }

        [JsonProperty("ti")]
        public decimal TickSize { get; set; }

        [JsonProperty("ls")]
        public decimal LotSize { get; set; }

        [JsonProperty("prc")]
        public decimal Price { get; set; }

        [JsonProperty("prcftr")]
        public string PriceFactor { get; set; } = string.Empty;

        [JsonProperty("flprc")]
        public decimal FillPrice { get; set; }

        [JsonProperty("norentm")]
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime NorenTime { get; set; }

        [JsonProperty("avgprc")]
        public decimal AveragePrice { get; set; }

        [JsonProperty("exchordid")]
        public string ExchangeOrderNumber { get; set; } = string.Empty;

        [JsonProperty("exch_tm")]
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime ExchangeTime { get; set; }
    }
}
