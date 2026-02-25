using Common.JsonConvertors;
using FlatTrade.Types.Base;
using Newtonsoft.Json;

namespace FlatTrade.AlertManager
{
    public class SetAlertRequest
    {
        [JsonProperty("uid")]
        public required string UserId { get; set; }

        [JsonProperty("exch")]
        public Exchange Exchange { get; set; }

        [JsonProperty("tsym")]
        public string TradingSymbol { get; set; } = string.Empty;

        [JsonProperty("remarks")]
        public string Remarks { get; set; } = string.Empty;

        [JsonProperty("ai_t")]
        public required AlertType AlertType { get; set; }

        [JsonProperty("validity")]
        public required RetentionType Validity { get; set; }

        [JsonProperty("d")]
        [JsonConverter(typeof(DecimalAsStringConverter))]
        public decimal DataToBeComparedWith { get; set; }
    }
}

