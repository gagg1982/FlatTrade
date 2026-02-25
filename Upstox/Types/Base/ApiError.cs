
using Newtonsoft.Json;

namespace Upstox.Types.Base
{
    public class ApiError
    {
        [JsonProperty("errorCode")]
        public string ErrorCode { get; set; } = string.Empty;
        [JsonProperty("message")]
        public string Message { get; set; } = string.Empty;
        [JsonProperty("propertyPath")]
        public string PropertyPath { get; set; } = string.Empty;
        [JsonProperty("invalidValue")]
        public string InvalidValue { get; set; } = string.Empty;
    }
}
