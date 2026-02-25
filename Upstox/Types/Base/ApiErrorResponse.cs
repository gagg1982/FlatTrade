using Newtonsoft.Json;

namespace Upstox.Types.Base
{
    public class ApiErrorResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; } = string.Empty;
        [JsonProperty("errors")]
        public List<ApiError> Errors { get; set; } = [];
    }
}
