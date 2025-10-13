using Newtonsoft.Json;

namespace FlatTrade.AuthenticationManager
{
    public class TokenResponse : BaseErrorMessageResponse
    {
        [JsonProperty("token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonProperty("client")]
        public string Client { get; set; } = string.Empty;
    }
}
