using Newtonsoft.Json;
using System.Text.Json;

namespace FlatTrade.UserManager
{
    public class UserDetailsRequest
    {
        [JsonProperty("uid")]
        public required string UserId { get; set; }
    }
}
