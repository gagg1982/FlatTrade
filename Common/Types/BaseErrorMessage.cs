using Newtonsoft.Json;

namespace Common.Types
{
    public class BaseErrorMessage
    {

        [JsonProperty("emsg")]
        public string ErrorMsg { get; set; } = string.Empty;
    }
}
