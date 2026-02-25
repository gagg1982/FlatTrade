using Common.JsonConvertors;  
using FlatTrade.Types.Base;
using Newtonsoft.Json;

namespace FlatTrade.AlertManager
{
    public class ModifyAlertRequest
    {
        [JsonProperty("uid")]
        public required string UserId { get; set; }

        [JsonProperty("exch")]
        public required Exchange Exchange { get; set; }

        [JsonProperty("tsym")]
        public required string TradingSymbol { get; set; }

        [JsonProperty("remarks")]
        public required string Remarks { get; set; }

        [JsonProperty("al_id")]
        [JsonConverter(typeof(LongAsStringConverter))]
        public required long AlertId { get; set; }

        [JsonProperty("ai_t")]
        public required AlertType AlertType { get; set; }

        [JsonProperty("validity")]
        public required RetentionType Validity { get; set; }

        [JsonProperty("d")]
        [JsonConverter(typeof(DecimalAsStringConverter))]
        public decimal DataToBeComparedWith { get; set; }
    }
}
