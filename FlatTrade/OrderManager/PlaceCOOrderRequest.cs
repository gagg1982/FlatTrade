using FlatTrade.Common.Types.Base;
using Newtonsoft.Json;

namespace FlatTrade.OrderManager
{
    public class PlaceCOOrderRequest
    {
        [JsonProperty("uid")]
        public required string UserId { get; set; }

        [JsonProperty("actid")]
        public required string AccountId { get; set; }

        [JsonProperty("exch")]
        public required Exchange Exchange { get; set; }

        [JsonProperty("tsym")]
        public required string TradingSymbol { get; set; }

        [JsonProperty("qty")]
        public long Quantity { get; set; }

        [JsonProperty("prc")]
        public decimal Price { get; set; }

        [JsonProperty("prd")]
        public required ProductType ProductType { get; set; }
        
        [JsonProperty("trantype")]
        public required TransactionType TransactionType { get; set; }

        [JsonProperty("prctyp")]
        public PriceType PriceType { get; set; }  //LMT/MKT

        [JsonProperty("blprc")]        
        public decimal BookLossProfit { get; set; }

        [JsonProperty("ret")]
        public required RetentionType RetentionType { get; set; }

        [JsonProperty("trgprc")]        
        public decimal TriggerPrice { get; set; }

        [JsonProperty("amo")]
        public string Amo { get; set; } = string.Empty; // value ="Yes"

        //[JsonProperty("ai_t")]
        //public required GTTAlertType AlertType { get; set; }

        //[JsonProperty("exchsym")]
        //public string ExchangeSymbol { get; set; } = string.Empty;

        //[JsonProperty("oivariable")]
        //public List<OIVariableRequest> OIVariables { get; set; } = [];

        //[JsonProperty("place_oder_params")]
        //public required List<PlaceOrderParameterRequest> PlaceOrderParameters { get; set; } = [];

        //[JsonProperty("place_oder_params_leg2")]
        //public required List<PlaceOrderParameterRequest> PlaceOrderParameters2 { get; set; } = [];
    }
}
