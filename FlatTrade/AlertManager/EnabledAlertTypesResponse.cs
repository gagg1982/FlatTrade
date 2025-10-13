using FlatTrade.Common.Types.Base;
using Newtonsoft.Json;

namespace FlatTrade.AlertManager
{
    public class EnabledAlertTypesResponse : BaseErrorMessageResponse
    {      
        [JsonProperty("ai_ts")]
        public List<AlertType> AlertTypes { get; set; } = [];
    }
}
