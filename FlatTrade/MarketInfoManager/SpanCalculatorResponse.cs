using Newtonsoft.Json;

namespace FlatTrade.MarketInfoManager
{
    public class SpanCalculatorResponse : BaseErrorMessageResponse
    {

        [JsonProperty("span")]
        public decimal SpanValue { get; set; }

        [JsonProperty("expo")]
        public decimal ExposureMargin { get; set; }

        [JsonProperty("span_trade")]
        public decimal SpanValueIgnoringInputBuySellQty { get; set; }

        [JsonProperty("expo_trade")]
        public decimal ExposureMarginIgnoringInputBuySellQty { get; set; }

    }
}
