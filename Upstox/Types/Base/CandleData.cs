using Common.Types;
using Newtonsoft.Json;

namespace Upstox.Types.Base
{
    public class CandleData
    {
        [JsonProperty("candles")]
        public List<PriceCandle> Candles { get; set; } = [];
    }
}
