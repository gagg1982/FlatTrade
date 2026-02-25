//using Common.JsonConvertors;
//using FlatTrade.Common.Types.Base;
//using Newtonsoft.Json;

//namespace FlatTrade.OrderManager
//{
//    public class ModifyOcoOrderRequest
//    {
//        [JsonProperty("uid")]
//        public required string UserId { get; set; }

//        [JsonProperty("actid")]
//        public required string AccountId { get; set; }

//        [JsonProperty("exch")]
//        public required Exchange Exchange { get; set; }

//        [JsonProperty("tsym")]
//        public required string TradingSymbol { get; set; }

//        [JsonProperty("validity")]
//        public required RetentionType Validity { get; set; }

//        [JsonProperty("ai_t")]
//        public required GTTAlertType AlertType { get; set; }

//        [JsonProperty("al_id")]
//        [JsonConverter(typeof(LongAsStringConverter))]
//        public required long AlertId { get; set; }

//        [JsonProperty("exchsym")]
//        public string ExchangeSymbol { get; set; } = string.Empty;

//        [JsonProperty("oivariable")]
//        public List<OIVariableRequest> OIVariables { get; set; } = [];

//        [JsonProperty("place_oder_params")]
//        public required List<PlaceOrderParameterRequest> PlaceOrderParameters { get; set; } = [];
//    }
//}
