using FlatTrade.Common.Types.Base;
using FlatTrade.ScripManager;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FlatTrade.SubscriptionManager.Quote
{
    public class QuoteSubscriptionUpdates : BaseSubscriptionRequest
    {
        [JsonProperty("e")]
        public Exchange Exchange { get; set; }

        [JsonProperty("tk")]
        public long Token { get; set; } = -1;

        [JsonProperty("ft")]
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime LastTradeDateTime { get; set; } = DateTime.MinValue;

        [JsonProperty("lp")]
        public decimal LastTradePrice { get; set; } = -1;

        [JsonProperty("v")]
        public long DayVolume { get; set; } = -1;

        [JsonProperty("ltq")]
        public long LastTradeQuantity { get; set; } = -1;

        [JsonProperty("ltt")]
        public string LastTradeTime { get; set; } = string.Empty;

        [JsonProperty("tsq")]
        public decimal TotalSellQuantity { get; set; } = -1;

        [JsonProperty("tbq")]
        public decimal TotalBuyQuantity { get; set; } = -1;

        [JsonConverter(typeof(QuotesResponseJsonConvertor))]
        public List<MarketDepthLevel> BestBids { get; set; } = [];

        [JsonConverter(typeof(QuotesResponseJsonConvertor))]
        public List<MarketDepthLevel> BestAsks { get; set; } = [];
    }
}
