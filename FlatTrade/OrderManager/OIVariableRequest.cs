using Common.JsonConvertors;
using Newtonsoft.Json;

namespace FlatTrade.OrderManager
{
    public class OIVariableRequest
    {
        [JsonProperty("d")]
        [JsonConverter(typeof(DecimalAsStringConverter))]
        public required decimal DataToBeComparedWithLTP { get; set; }

        [JsonProperty("var_name")]
        public required string VariableName { get; set; }
    }
}
