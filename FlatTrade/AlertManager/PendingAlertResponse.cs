using FlatTrade.Common.JsonConvertors;
using FlatTrade.Common.Types.Base;
using Newtonsoft.Json;

namespace FlatTrade.AlertManager
{
    public class PendingAlertResponse : BaseErrorMessageResponse
    {
        [JsonProperty("exch")]
        public Exchange Exchange { get; set; }

        [JsonProperty("tsym")]
        public string TradingSymbol { get; set; } = string.Empty;

        [JsonProperty("remarks")]
        public string Remarks { get; set; } = string.Empty;

        [JsonProperty("token")]
        public long Token { get; set; }

        [JsonProperty("al_id")]
        public long AlertId { get; set; }

        [JsonProperty("ai_t")]
        public AlertType AlertType { get; set; }

        [JsonProperty("validity")]
        public RetentionType Validity { get; set; }

        [JsonProperty("d")]
        [JsonConverter(typeof(DecimalAsStringConverter))]
        public decimal DataToBeComparedWith { get; set; }
    }
}
