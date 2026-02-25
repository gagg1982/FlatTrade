using FlatTrade.ScripManager;
using FlatTrade.Types.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FlatTrade.SubscriptionManager.Quote
{
    public class QuoteSubscriptionUpdates : BaseSubscriptionRequest
    {
        [JsonProperty("e")]
        public Exchange Exchange { get; set; }

        [JsonProperty("tk")]
        public long Token { get; set; } = 0;

        [JsonProperty("ft")]
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime LastTradeDateTime { get; set; } = DateTime.MinValue;

        [JsonProperty("lp")]
        public decimal LastTradePrice { get; set; } = decimal.MinValue;

        [JsonProperty("v")]
        public long DayVolume { get; set; } = 0;

        [JsonProperty("ltq")]
        public long LastTradeQuantity { get; set; } = 0;

        [JsonProperty("ltt")]
        public string LastTradeTime { get; set; } = string.Empty;

        [JsonProperty("tsq")]
        public decimal TotalSellQuantity { get; set; } = 0;

        [JsonProperty("tbq")]
        public decimal TotalBuyQuantity { get; set; } = 0;

        [JsonConverter(typeof(QuotesResponseJsonConvertor))]
        public List<MarketDepthLevel> BestBids { get; set; } = [];

        [JsonConverter(typeof(QuotesResponseJsonConvertor))]
        public List<MarketDepthLevel> BestAsks { get; set; } = [];
    }
}
