using Newtonsoft.Json;
using Upstox.Types.Base;

namespace Upstox.MarketInfoManager
{
    public class TimePriceDataResponse: ApiErrorResponse
    {
        [JsonProperty("data")]
        public CandleData Data { get; set; } = new();
    }
}
