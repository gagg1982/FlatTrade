using FlatTrade.Types.Base;
using Newtonsoft.Json;

namespace FlatTrade.HoldingsManager
{
    public class HoldingsResponse : BaseErrorMessageResponse
    {
        [JsonProperty("exch_tsym")]
        public List<ExchangeSymbolResponse> ExchangeSymbolResponse { get; set; } = [];

        [JsonProperty("holdqty")]
        public long HoldingQuantity { get; set; }

        [JsonProperty("npoadqty")]
        public long NonPoaDisplayQuantity { get; set; }

        [JsonProperty("npoadt1qty")]
        public long NonPoaDisplayT1Quantity { get; set; }

        [JsonProperty("benqty")]
        public long BeneficiaryQuantity { get; set; }

        [JsonProperty("eqtbrkcollqty")]
        public long BrokerEquityPledgedAsCollateralQuantity { get; set; }


        [JsonProperty("fxbrkcollqty")]
        public long BrokerForexPledgedAsCollateralQuantity { get; set; }

        [JsonProperty("combrkcollqty")]
        public long BrokerAllMarketPledgedAsCollateralQuantity { get; set; }

        [JsonProperty("derbrkcollqty")]
        public long BrokerDerivativeMarketPledgedAsCollateralQuantity { get; set; }

        [JsonProperty("btstqty")]
        public long BuyTodaySellTommorrowQuantity { get; set; }

        [JsonProperty("usedqty")]
        public long HoldingQuantityUsedToday { get; set; }

        [JsonProperty("dpqty")]
        public long DpHoldingQuantity { get; set; }

        [JsonProperty("upldprc")]
        public decimal AvgPriceUploadedAlongWithHoldings { get; set; }

        [JsonProperty("brk_hair_cut")]
        public decimal HairCutPercOnPledgedSecurities { get; set; }

        [JsonProperty("sell_amt")]
        public decimal TodaySellAmount { get; set; }

        [JsonProperty("s_prdt_ali")]
        public ProductName ProductDisplayName { get; set; }

        [JsonProperty("trdqty")]
        public long TradeQuantity { get; set; }

        [JsonProperty("prd")]
        public ProductType ProductType { get; set; }

        [JsonProperty("epi_done_qty")]
        public long ExchangePendingInstructionDoneQuantity { get; set; }

        [JsonProperty("c")]
        public decimal DailyClose { get; set; }

        public HoldingsResponse() { }
        protected HoldingsResponse(HoldingsResponse other)
        {
            other.ExchangeSymbolResponse.ForEach(exch => ExchangeSymbolResponse.Add(exch.ShallowCopy()));
            HoldingQuantity = other.HoldingQuantity;
            NonPoaDisplayQuantity = other.NonPoaDisplayQuantity;
            NonPoaDisplayT1Quantity = other.NonPoaDisplayT1Quantity;
            BeneficiaryQuantity = other.BeneficiaryQuantity;
            BrokerEquityPledgedAsCollateralQuantity = other.BrokerEquityPledgedAsCollateralQuantity;
            BrokerForexPledgedAsCollateralQuantity = other.BrokerForexPledgedAsCollateralQuantity;
            BrokerAllMarketPledgedAsCollateralQuantity = other.BrokerAllMarketPledgedAsCollateralQuantity;
            BrokerDerivativeMarketPledgedAsCollateralQuantity = other.BrokerDerivativeMarketPledgedAsCollateralQuantity;
            BuyTodaySellTommorrowQuantity = other.BuyTodaySellTommorrowQuantity;
            HoldingQuantityUsedToday = other.HoldingQuantityUsedToday;
            DpHoldingQuantity = other.DpHoldingQuantity;
            AvgPriceUploadedAlongWithHoldings = other.AvgPriceUploadedAlongWithHoldings;
            HairCutPercOnPledgedSecurities = other.HairCutPercOnPledgedSecurities;
            TodaySellAmount = other.TodaySellAmount;
            ProductDisplayName = other.ProductDisplayName;
            TradeQuantity = other.TradeQuantity;
            ProductType = other.ProductType;
            ExchangePendingInstructionDoneQuantity = other.ExchangePendingInstructionDoneQuantity;
            DailyClose = other.DailyClose;
        }
    }
}
