using FlatTrade.Common.JsonConvertors;
using FlatTrade.Common.Types.Base;
using FlatTrade.ScripManager;
using FlatTrade.SubscriptionManager.TouchLine;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Diagnostics;

namespace FlatTrade.SubscriptionManager.Quote
{
    public class QuoteSubscriptionRequestAck : BaseSubscriptionRequest
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

        [JsonProperty("ft")]
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime LastTradeDateTime { get; set; }

        [JsonProperty("c")]
        public decimal DayClosePrice { get; set; }

        [JsonProperty("o")]
        public decimal DayOpenPrice { get; set; }

        [JsonProperty("h")]
        public decimal DayHighPrice { get; set; }

        [JsonProperty("l")]
        public decimal DayLowPrice { get; set; }

        [JsonProperty("ap")]
        public decimal AverageTradePrice { get; set; }

        [JsonProperty("v")]
        public long DayVolume { get; set; }

        [JsonProperty("ltq")]
        public long LastTradeQuantity { get; set; }

        [JsonProperty("ltt")]
        public string LastTradeTime { get; set; } = string.Empty;

        [JsonProperty("tsq")]
        public decimal TotalSellQuantity { get; set; }

        [JsonProperty("tbq")]
        public decimal TotalBuyQuantity { get; set; }

        [JsonConverter(typeof(QuotesResponseJsonConvertor))]
        public List<MarketDepthLevel> BestBids { get; set; } = [];

        [JsonConverter(typeof(QuotesResponseJsonConvertor))]
        public List<MarketDepthLevel> BestAsks { get; set; } = [];

        [JsonProperty("uc")]
        public decimal UpperCircuitLimit { get; set; }

        [JsonProperty("lc")]
        public decimal LowerCircuitLimit { get; set; }

        [JsonProperty("52h")]
        public decimal Wk52High { get; set; }

        [JsonProperty("52l")]
        public decimal Wk52Low { get; set; }

        [JsonProperty("52hd")]
        [JsonConverter(typeof(DateOnlyAsStringConverterDdMonYyyy))]
        public DateOnly Wk52HighDate { get; set; }

        [JsonProperty("52ld")]
        [JsonConverter(typeof(DateOnlyAsStringConverterDdMonYyyy))]
        public DateOnly Wk52LowDate { get; set; }

        [JsonProperty("toi")]
        public long IntervalIoChange { get; set; }

        public QuoteSubscriptionRequestAck Update(QuoteSubscriptionUpdates val)
        {
            if (val is null)
                return this;

            lock (this)
            {
                Exchange = val.Exchange;
                Token = val.Token == -1 ? Token : val.Token;
                LastTradeDateTime = val.LastTradeDateTime == DateTime.MinValue ? LastTradeDateTime: val.LastTradeDateTime;
                DayVolume = val.DayVolume == -1 ? DayVolume: val.DayVolume;
                LastTradeQuantity = val.LastTradeQuantity == -1 ? LastTradeQuantity : val.LastTradeQuantity;
                LastTradeTime = val.LastTradeTime == string.Empty ? LastTradeTime : val.LastTradeTime;
                TotalSellQuantity = val.TotalSellQuantity == -1 ? TotalSellQuantity : val.TotalSellQuantity;
                TotalBuyQuantity = val.TotalBuyQuantity == -1 ? TotalBuyQuantity : val.TotalBuyQuantity;
                LastTradePrice = val.LastTradePrice == -1 ? LastTradePrice : val.LastTradePrice;
                UpdateMarketDepth(val);
            }
            return this;
        }

        private void UpdateMarketDepth(QuoteSubscriptionUpdates val)
        {
            int cnt = 0;
            foreach(var bid in val.BestBids)
            {
                BestBids[cnt].Price = bid.Price == -1 ? BestBids[cnt].Price : bid.Price;
                BestBids[cnt].Quantity = bid.Quantity == -1 ? BestBids[cnt].Quantity : bid.Quantity;
                BestBids[cnt].Orders = bid.Orders == -1 ? BestBids[cnt].Orders : bid.Orders;
                ++cnt;
            }            
        }
    }
}
