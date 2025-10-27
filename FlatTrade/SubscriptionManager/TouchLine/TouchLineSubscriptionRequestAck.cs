using FlatTrade.Common.Types.Base;
using FlatTrade.OrderManager;
using Newtonsoft.Json;

namespace FlatTrade.SubscriptionManager.TouchLine
{
    public class TouchLineSubscriptionRequestAck : BaseSubscriptionRequest
    {
        [JsonProperty("e")]
        public Exchange Exchange { get; set; }

        [JsonProperty("tk")]
        public long Token { get; set; } = 0;

        [JsonProperty("ts")]
        public string TradingSymbol { get; set; } = string.Empty;

        [JsonProperty("pp")]
        public int PricePrecision { get; set; } = 0;

        [JsonProperty("ls")]
        public decimal LotSize { get; set; } = decimal.MinValue;

        [JsonProperty("ti")]
        public decimal TickSize { get; set; } = decimal.MinValue;

        [JsonProperty("lp")]
        public decimal LastTradePrice { get; set; } = decimal.MinValue;

        [JsonProperty("pc")]
        public decimal LastTradePricePercentageChange { get; set; } = decimal.MinValue;

        [JsonProperty("c")]
        public decimal Close { get; set; } = decimal.MinValue;

        [JsonProperty("o")]
        public decimal Open { get; set; } = decimal.MinValue;

        [JsonProperty("h")]
        public decimal High { get; set; } = decimal.MinValue;

        [JsonProperty("l")]
        public decimal Low { get; set; } = decimal.MinValue;

        [JsonProperty("v")]
        public long Volume { get; set; } = 0;

        [JsonProperty("ap")]
        public decimal AverageTradePrice { get; set; } = decimal.MinValue;

        [JsonProperty("bp1")]
        public decimal BuyPrice { get; set; } = decimal.MinValue;

        [JsonProperty("sp1")]
        public decimal SellPrice { get; set; } = decimal.MinValue;

        [JsonProperty("bq1")]
        public long BuyQuantity { get; set; } = 0;

        [JsonProperty("sq1")]
        public long SellQuantity { get; set; } = 0;

        [JsonProperty("toi")]
        public long IntervalIoChange { get; set; } = 0;

        public TouchLineSubscriptionRequestAck Update(TouchLineSubscriptionRequestAck val)
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

                TradingSymbol = val.TradingSymbol == string.Empty ? TradingSymbol : val.TradingSymbol;
                PricePrecision = val.PricePrecision == decimal.MinValue? PricePrecision: val.PricePrecision;
                LotSize = val.LotSize == decimal.MinValue? LotSize: val.LotSize;
                TickSize = val.TickSize;
                Close = val.Close == decimal.MinValue ? Close : val.Close;
                Open = val.Open == decimal.MinValue ? Open : val.Open;
                High = val.High == decimal.MinValue ? High : val.High;
                Low = val.Low == decimal.MinValue ? Low : val.Low;
                AverageTradePrice = val.AverageTradePrice;
                IntervalIoChange = val.IntervalIoChange;
            }
            return this;
        }

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
            }
            return this;
        }
    }
}
