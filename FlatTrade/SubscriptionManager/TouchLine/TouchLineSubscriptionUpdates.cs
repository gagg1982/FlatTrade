using FlatTrade.Common.Types.Base;
using Newtonsoft.Json;

namespace FlatTrade.SubscriptionManager.TouchLine
{
    public class TouchLineSubscriptionUpdates : BaseSubscriptionRequest
    {
        [JsonProperty("e")]
        public Exchange Exchange { get; set; }

        [JsonProperty("tk")]
        public long Token { get; set; } = 0;

        [JsonProperty("pc")]
        public decimal LastTradePricePercentageChange { get; set; }

        [JsonProperty("lp")]
        public decimal LastTradePrice { get; set; } = decimal.MinValue;

        [JsonProperty("bp1")]
        public decimal BuyPrice { get; set; } = decimal.MinValue;

        [JsonProperty("sp1")]
        public decimal SellPrice { get; set; } = decimal.MinValue;

        [JsonProperty("bq1")]
        public long BuyQuantity { get; set; } = 0;

        [JsonProperty("sq1")]
        public long SellQuantity { get; set; } = 0;

        [JsonProperty("v")]
        public long Volume { get; set; } = 0;
    }
}
