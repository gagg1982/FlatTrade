using FlatTrade.Common.Types.Base;
using Newtonsoft.Json;

namespace FlatTrade.SubscriptionManager.TouchLine
{
    public class TouchLineSubscriptionRequestAck : BaseSubscriptionRequest
    {
        [JsonProperty("e")]
        public Exchange Exchange { get; set; }

        [JsonProperty("tk")]
        public long Token { get; set; }

        [JsonProperty("ts")]
        public string TradingSymbol { get; set; } = string.Empty;

        [JsonProperty("pp")]
        public int PricePrecision { get; set; }

        [JsonProperty("ls")]
        public decimal LotSize { get; set; }

        [JsonProperty("ti")]
        public decimal TickSize { get; set; }

        [JsonProperty("lp")]
        public decimal LastTradePrice { get; set; }

        [JsonProperty("pc")]
        public decimal LastTradePricePercentageChange { get; set; }

        [JsonProperty("c")]
        public decimal Close { get; set; }

        [JsonProperty("o")]
        public decimal Open { get; set; }

        [JsonProperty("h")]
        public decimal High { get; set; }

        [JsonProperty("l")]
        public decimal Low { get; set; }

        [JsonProperty("v")]
        public long Volume { get; set; }

        [JsonProperty("ap")]
        public decimal AverageTradePrice { get; set; }

        [JsonProperty("bp1")]
        public decimal BuyPrice { get; set; }

        [JsonProperty("sp1")]
        public decimal SellPrice { get; set; }

        [JsonProperty("bq1")]
        public long BuyQuantity { get; set; }

        [JsonProperty("sq1")]
        public long SellQuantity { get; set; }

        [JsonProperty("toi")]
        public long IntervalIoChange { get; set; }


        public TouchLineSubscriptionRequestAck Update(TouchLineSubscriptionUpdates val)
        {
            if (val is null)
                return this;

            lock (this)
            {
                Exchange = val.Exchange;
                Token = val.Token == 0 ? Token : val.Token;
                LastTradePricePercentageChange = val.LastTradePricePercentageChange;
                LastTradePrice = val.LastTradePrice == decimal.MinValue ? LastTradePrice : val.LastTradePrice;
                BuyPrice = val.BuyPrice == decimal.MinValue ? BuyPrice : val.BuyPrice;
                SellPrice = val.SellPrice == decimal.MinValue ? SellPrice : val.SellPrice;
                BuyQuantity = val.BuyQuantity == 0 ? BuyQuantity : val.BuyQuantity;
                SellQuantity = val.SellQuantity == 0 ? SellQuantity : val.SellQuantity;
                Volume = val.Volume == 0 ? Volume : val.Volume;
                return this;
            }            
        }
    }
}
