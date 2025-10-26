using FlatTrade.Common.Types.Base;
using Newtonsoft.Json;

namespace FlatTrade.TradeManager
{
    public class PositionBookResponse : BaseErrorMessageResponse
    {
        [JsonProperty("token")]
        public long Token { get; set; }

        [JsonProperty("exch")]
        public Exchange Exchange { get; set; }

        [JsonProperty("tsym")]
        public string TradingSymbol { get; set; } = string.Empty;

        [JsonProperty("actid")]
        public string AccountId { get; set; } = string.Empty;

        [JsonProperty("uid")]
        public string UserId { get; set; } = string.Empty;

        [JsonProperty("s_prdt_ali")]
        public ProductName ProductDisplayName { get; set; }

        [JsonProperty("prd")]
        public ProductType ProductType { get; set; }

        [JsonProperty("netqty")]
        public long NetPositionQuantity { get; set; }

        [JsonProperty("netavgprc")]
        public decimal NetAveragePositionPrice { get; set; }

        [JsonProperty("daybuyqty")]
        public long DayBuyQuantity { get; set; }

        [JsonProperty("daysellqty")]
        public int DaySellQuantity { get; set; }

        [JsonProperty("dayavgprc")]
        public decimal DayAveragePrice { get; set; }

        [JsonProperty("daybuyavgprc")]
        public decimal DayAverageBuyPrice { get; set; }

        [JsonProperty("daysellavgprc")]
        public decimal DayAverageSellPrice { get; set; }

        [JsonProperty("frzqty")]
        public long FreezeQuantity { get; set; }

        [JsonProperty("cname")]
        public string CompanyName { get; set; } = string.Empty;

        [JsonProperty("daybuyamt")]
        public decimal DayBuyAmount { get; set; }

        [JsonProperty("daysellamt")]
        public decimal DaySellAmount { get; set; }

        [JsonProperty("cfbuyqty")]
        public long CarrfyFwdBuyQuantity { get; set; }

        [JsonProperty("cfsellqty")]
        public long CarryFwdSellQuantity { get; set; }

        [JsonProperty("cforgavgprc")]
        public decimal CarryFwdOriginalAveragePrice { get; set; }

        [JsonProperty("cfbuyavgprc")]
        public decimal CarryFwdAverageBuyPrice { get; set; }

        [JsonProperty("cfsellavgprc")]
        public decimal CarryFwdAverageSellPrice { get; set; }

        [JsonProperty("cfbuyamt")]
        public decimal CarryFwdBuyAmount { get; set; }

        [JsonProperty("cfsellamt")]
        public decimal CarryFwdSellAmount { get; set; }

        [JsonProperty("totbuyamt")]
        public decimal TotalBuyAmount { get; set; }

        [JsonProperty("totsellamt")]
        public decimal TotalSellAmount { get; set; }

        [JsonProperty("totbuyavgprc")]
        public decimal TotalBuyAveragePrice { get; set; }

        [JsonProperty("totsellavgprc")]
        public decimal TotalSellAveragePrice { get; set; }

        [JsonProperty("lp")]
        public decimal LastTradePrice { get; set; }

        [JsonProperty("rpnl")]
        public decimal RealizedPnl { get; set; }

        [JsonProperty("urmtom")]
        public decimal UnRealizedMTM { get; set; }

        [JsonProperty("bep")]
        public decimal BreakEvenPrice { get; set; }

        [JsonProperty("upldprc")]
        public decimal AvgPriceUploadedAlongWithHoldings { get; set; }

        [JsonProperty("netupldprc")]
        public decimal NetAvgPriceUploadedAlongWithHoldings { get; set; }


        [JsonProperty("openbuyqty")]
        public int OpenBuyQuantity { get; set; }

        [JsonProperty("opensellqty")]
        public int OpenSellQuantity { get; set; }

        [JsonProperty("openbuyamt")]
        public decimal OpenBuyAmount { get; set; }

        [JsonProperty("opensellamt")]
        public decimal OpenSellAmount { get; set; }

        [JsonProperty("openbuyavgprc")]
        public decimal OpenAverageBuyPrice { get; set; }

        [JsonProperty("opensellavgprc")]
        public decimal OpenAverageSellPrice { get; set; }

        [JsonProperty("mult")]
        public decimal Multiplier { get; set; }

        [JsonProperty("pp")]
        public int PricePrecision { get; set; }

        [JsonProperty("ti")]
        public decimal TickSize { get; set; }

        [JsonProperty("ls")]
        public decimal LotSize { get; set; }

        [JsonProperty("prcftr")]
        public string PriceFactor { get; set; } = string.Empty;

        [JsonProperty("instname")]
        public InstrumentName InstrumentName { get; set; }

        public DateTime CreatedAt {
            get
            {
                var currDateTime = DateTime.Now.ToLocalTime();
                var StartHourOfCurrentDate = currDateTime.Date;
                var EndHourOfCurrentDate = StartHourOfCurrentDate.Date.AddHours(9);

                if (currDateTime >= StartHourOfCurrentDate && currDateTime <= EndHourOfCurrentDate)
                    return EndHourOfCurrentDate.Date.AddHours(-10).Date.AddMinutes(50);
                return currDateTime;
            }
        }
    }
}
