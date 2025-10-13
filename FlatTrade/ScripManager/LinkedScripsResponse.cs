using Newtonsoft.Json;

namespace FlatTrade.ScripManager
{
    public class LinkedScripsResponse : BaseErrorMessageResponse
    {     
        [JsonProperty("fut")]
        public List<LinkedFutureResponse> LinkedFutures { get; set; } = [];

        [JsonProperty("equls")]
        public List<LinkedEquityResponse> LinkedEquities { get; set; } = [];

        [JsonProperty("opt_exp")]
        public List<LinkedOptionResponse> LinkedOptions { get; set; } = [];

    }
}
