using FlatTrade.Common.Types.Base;
using Newtonsoft.Json;

namespace FlatTrade.LimitsManager
{
    public class LimitsResponse : BaseErrorMessageResponse
    {
        [JsonProperty("prd")]
        public ProductType ProductType { get; set; }

        [JsonProperty("prfname")]
        public string PlatformName { get; set; } = string.Empty;

        [JsonProperty("actid")]
        public string AccountId { get; set; } = string.Empty;

        [JsonProperty("seg")]
        public string Segment { get; set; } = string.Empty;

        [JsonProperty("exch")]
        public Exchange Exchange { get; set; }

        //-------------------Cash primary Fields--------------------    

        [JsonProperty("cash")]
        public decimal MarginCashAvailable { get; set; }

        [JsonProperty("payin")]
        public decimal TotAmntTrnfdUsingPayinsToday { get; set; }

        [JsonProperty("payout")]
        public decimal TotAmntReqtdForWithdrawalToday { get; set; }

        //-------------------Cash additional Fields--------------------    

        [JsonProperty("brkcollamt")]
        public decimal PrevaluedCollateralAmount { get; set; }

        [JsonProperty("unclearedcash")]
        public decimal UnclearedCash { get; set; }

        [JsonProperty("daycash")]
        public decimal AmountAddedByBrokerToHandleErrors { get; set; }

        //-------------------Margin Utilized--------------------    

        [JsonProperty("marginused")]
        public decimal TotalMarginUsedToday { get; set; }

        [JsonProperty("mtomcurper")]
        public decimal MTMCurrentPercentage { get; set; }


        //-------------------Margin used components--------------------    

        [JsonProperty("cbu")]
        public decimal CashAndCarryBuyUsed { get; set; }

        [JsonProperty("csc")]
        public decimal CashAndCarrySellCredits { get; set; }

        [JsonProperty("rpnl")]
        public decimal CurrentRealizedPNL { get; set; }

        [JsonProperty("unmtom")]
        public decimal CurrentUnrealizedMTM { get; set; }

        [JsonProperty("marprt")]
        public decimal CoveredProductMargins { get; set; }

        [JsonProperty("span")]
        public decimal SpanUsed { get; set; }

        [JsonProperty("expo")]
        public decimal ExposureMargin { get; set; }

        [JsonProperty("premium")]
        public decimal PremiumUsed { get; set; }

        [JsonProperty("varelm")]
        public decimal VarElmMargin { get; set; }

        [JsonProperty("grexpo")]
        public decimal GrossExposure { get; set; }

        [JsonProperty("grexpo_d")]
        public decimal GrossExposureDeivative { get; set; }

        [JsonProperty("scripbskmar")]
        public decimal ScripBasketMargin { get; set; }

        [JsonProperty("addscripbskmrg")]
        public decimal AdditionalScripBasketMargin { get; set; }

        [JsonProperty("brokerage")]
        public decimal BrokerageAmount { get; set; }

        [JsonProperty("collateral")]
        public decimal CollateralCalculatedBasedOnUploadedHoldings { get; set; }

        [JsonProperty("cash_coll")]
        public decimal CashCollateral { get; set; }

        [JsonProperty("grcoll")]
        public decimal ValudationOfUploadedHoldingPreHaircut { get; set; }

        //-------------------Additional Risk Limits--------------------    

        [JsonProperty("turnoverlmt")]
        public decimal TurnOverLimit { get; set; }

        [JsonProperty("pendordvallmt")]
        public decimal PendingOrderValueLimit { get; set; }

        //-------------------Additional Risk Indicators----------------    

        [JsonProperty("turnover")]
        public decimal Turnover { get; set; }

        [JsonProperty("pendordval")]
        public decimal PendingOrderValue { get; set; }

        //----------------Margin used detailed breakup fields---------------- 

        [JsonProperty("rzpnl_e_i")]
        public decimal CurrentRealizedPNLForEquityIntraday { get; set; } // Current realized PNL(Equity Intraday)

        [JsonProperty("decimalrzpnl_e_m")]
        public decimal CurrentRealizedPNLForEquityMargin { get; set; }  //Current realized PNL(Equity Margin)

        [JsonProperty("rzpnl_e_c")]
        public decimal CurrentRealizedPNLForCashNCarry { get; set; }  //Current realized PNL(Equity Cash n Carry)

        [JsonProperty("rzpnl_d_i")]
        public decimal CurrentRealizedPNLForDerivativeIntraday { get; set; }  //Current realized PNL(Derivative Intraday)

        [JsonProperty("rzpnl_d_m")]
        public decimal CurrentRealizedPNLForDerivativeMargin { get; set; }  //Current realized PNL(Derivative Margin)

        [JsonProperty("rzpnl_f_i")]
        public decimal CurrentRealizedPNLForFxIntraday { get; set; }  //Current realized PNL(FX Intraday)

        [JsonProperty("rzpnl_f_m")]
        public decimal CurrentRealizedPNLForFxmargin { get; set; }  //Current realized PNL(FX Margin)

        [JsonProperty("rzpnl_c_i")]
        public decimal CurrentRealizedPNLForCommodityIntraday { get; set; }  //Current realized PNL(Commodity Intraday)

        [JsonProperty("rzpnl_c_m")]
        public decimal CurrentRealizedPNLForCommodityMargin { get; set; }  //Current realized PNL(Commodity Margin)

        [JsonProperty("uzpnl_e_i")]
        public decimal CurrentUnrealizedMTMForEquityIntraday { get; set; }  //Current unrealized MTOM(Equity Intraday)

        [JsonProperty("uzpnl_e_m")]
        public decimal CurrentUnrealizedMTMForEquityMargin { get; set; }  //Current unrealized MTOM(Equity Margin)

        [JsonProperty("uzpnl_e_c")]
        public decimal CurrentUnrealizedMTMForEquityCashNCarry { get; set; }  //Current unrealized MTOM(Equity Cash n Carry)

        [JsonProperty("uzpnl_d_i")]
        public decimal CurrentUnrealizedMTMForDerivativeIntraday { get; set; }  //Current unrealized MTOM(Derivative Intraday)

        [JsonProperty("uzpnl_d_m")]
        public decimal CurrentUnrealizedMTMForDerivativbeMargin { get; set; }  //Current unrealized MTOM(Derivative Margin)

        [JsonProperty("uzpnl_f_i")]
        public decimal CurrentUnrealizedMTMForFxIntraday { get; set; }  //Current unrealized MTOM(FX Intraday)

        [JsonProperty("uzpnl_f_m")]
        public decimal CurrentUnrealizedMTMForFxMargin { get; set; }  //Current unrealized MTOM(FX Margin)

        [JsonProperty("uzpnl_c_i")]
        public decimal CurrentUnrealizedMTMForCommodityIntraday { get; set; }  //Current unrealized MTOM(Commodity Intraday)

        [JsonProperty("uzpnl_c_m")]
        public decimal CurrentUnrealizedMTMForCommodityMargin { get; set; }  //Current unrealized MTOM(Commodity Margin)

        [JsonProperty("span_d_i")]
        public decimal SpanMarginForDeriviativeIntraday { get; set; }  //Span Margin(Derivative Intraday)

        [JsonProperty("span_d_m")]
        public decimal SpanMarginForDeriviativeMargin { get; set; }  //Span Margin(Derivative Margin)

        [JsonProperty("span_f_i")]
        public decimal SpanMarginForFxIntraday { get; set; }  //Span Margin(FX Intraday)

        [JsonProperty("span_f_m")]
        public decimal SpanMarginForFxMargin { get; set; }  //Span Margin(FX Margin)

        [JsonProperty("span_c_i")]
        public decimal SpanMarginForCommodityIntraday { get; set; }  //Span Margin(Commodity Intraday)

        [JsonProperty("span_c_m")]
        public decimal SpanMarginForCommodityMargin { get; set; }  //Span Margin(Commodity Margin)

        [JsonProperty("expo_d_i")]
        public decimal ExposureMarginForDerivativeIntraday { get; set; }  //Exposure Margin(Derivative Intraday)

        [JsonProperty("expo_d_m")]
        public decimal ExposureMarginForDerivativeMargin { get; set; }  //Exposure Margin(Derivative Margin)

        [JsonProperty("expo_f_i")]
        public decimal ExposureMarginForFxIntraday { get; set; }  //Exposure Margin(FX Intraday)

        [JsonProperty("expo_f_m")]
        public decimal ExposureMarginForFxMargin { get; set; }  //Exposure Margin(FX Margin)

        [JsonProperty("expo_c_i")]
        public decimal ExposureMarginForCommodityIntraday { get; set; }  //Exposure Margin(Commodity Intraday)

        [JsonProperty("expo_c_m")]
        public decimal ExposureMarginForCommodityMargin { get; set; }  //Exposure Margin(Commodity Margin)

        [JsonProperty("premium_d_i")]
        public decimal OptionPremiumForDerivativeIntraday { get; set; }  //Option premium(Derivative Intraday)

        [JsonProperty("premium_d_m")]
        public decimal OptionPremiumForDerivativeMargin { get; set; }  //Option premium(Derivative Margin)

        [JsonProperty("premium_f_i")]
        public decimal OptionPremiumForFxIntraday { get; set; }  //Option premium(FX Intraday)

        [JsonProperty("premium_f_m")]
        public decimal OptionPremiumForFxMargin { get; set; }  //Option premium(FX Margin)

        [JsonProperty("premium_c_i")]
        public decimal OptionPremiumForCommodityIntraday { get; set; }  //Option premium(Commodity Intraday)

        [JsonProperty("premium_c_m")]
        public decimal OptionPremiumForCommodityMargin { get; set; }  //Option premium(Commodity Margin)

        [JsonProperty("varelm_e_i")]
        public decimal VarElmForEquityIntraday { get; set; }  //Var Elm(Equity Intraday)

        [JsonProperty("varelm_e_m")]
        public decimal VarElmForEquityMargin { get; set; }  //Var Elm(Equity Margin)

        [JsonProperty("varelm_e_c")]
        public decimal VarElmForEquityCashNCarry { get; set; }  //Var Elm(Equity Cash n Carry)

        [JsonProperty("marprt_e_h")]
        public decimal CoveredProductMarginsForEquityHighLeverage { get; set; }  //Covered Product margins(Equity High leverage)

        [JsonProperty("marprt_e_b")]
        public decimal CoveredProductMarginsForEquityBracketOrder { get; set; }  //Covered Product margins(Equity Bracket Order)

        [JsonProperty("marprt_d_h")]
        public decimal CoveredProductMarginsForDerivativeHighLeverage { get; set; }  //Covered Product margins(Derivative High leverage)

        [JsonProperty("marprt_d_b")]
        public decimal CoveredProductMarginsForDerivativeBracketOrder { get; set; }  //Covered Product margins(Derivative Bracket Order)

        [JsonProperty("marprt_f_h")]
        public decimal CoveredProductMarginsForFxHighLeverage { get; set; }  //Covered Product margins(FX High leverage)

        [JsonProperty("marprt_f_b")]
        public decimal CoveredProductMarginsForFxBracketOrder { get; set; }  //Covered Product margins(FX Bracket Order)

        [JsonProperty("marprt_c_h")]
        public decimal CoveredProductMarginsForCommodityHighLeverage { get; set; }  //Covered Product margins(Commodity High leverage)

        [JsonProperty("marprt_c_b")]
        public decimal CoveredProductMarginsForCommodityBracketOrder { get; set; }  //Covered Product margins(Commodity Bracket Order)

        [JsonProperty("scripbskmar_e_i")]
        public decimal ScripBasketMarginForEquityIntraday { get; set; }  //Scrip basket margin(Equity Intraday)

        [JsonProperty("scripbskmar_e_m")]
        public decimal ScripBasketMarginForEquityMargin { get; set; }  //Scrip basket margin(Equity Margin)

        [JsonProperty("scripbskmar_e_c")]
        public decimal ScripBasketMarginForEquityCashNCarry { get; set; }  //Scrip basket margin(Equity Cash n Carry)

        [JsonProperty("addscripbskmrg_d_i")]
        public decimal AddlScripBasketMarginForDerivativeIntraday { get; set; }  // Additional scrip basket margin(Derivative Intraday)

        [JsonProperty("addscripbskmrg_d_m")]
        public decimal AddlScripBasketMarginForDerivativeMargin { get; set; }  // Additional scrip basket margin(Derivative Margin)

        [JsonProperty("addscripbskmrg_f_i")]
        public decimal AddlScripBasketMarginForFxIntraday { get; set; }  // Additional scrip basket margin(FX Intraday)

        [JsonProperty("addscripbskmrg_f_m")]
        public decimal AddlScripBasketMarginForFxMargin { get; set; }  // Additional scrip basket margin(FX Margin)

        [JsonProperty("addscripbskmrg_c_i")]
        public decimal AddlScripBasketMarginForCommodityIntraday { get; set; }  //Additional scrip basket margin(Commodity Intraday)

        [JsonProperty("addscripbskmrg_c_m")]
        public decimal AddlScripBasketMarginForCommodityMargin { get; set; }  //Additional scrip basket margin(Commodity Margin)

        [JsonProperty("brkage_e_i")]
        public decimal BrokerageForEquityIntraday { get; set; }  //Brokerage(Equity Intraday)

        [JsonProperty("brkage_e_m")]
        public decimal BrokerageForEquityMargin { get; set; }  //Brokerage(Equity Margin)

        [JsonProperty("brkage_e_c")]
        public decimal BrokerageForEquityCashNCarry { get; set; }  //Brokerage(Equity CAC)

        [JsonProperty("brkage_e_h")]
        public decimal BrokerageForEquityHighLeverage { get; set; }  //Brokerage(Equity High Leverage)

        [JsonProperty("brkage_e_b")]
        public decimal BrokerageForEquityBracketOrder { get; set; }  //Brokerage(Equity Bracket Order)

        [JsonProperty("brkage_d_i")]
        public decimal BrokerageForDerivativeIntraday { get; set; }  //Brokerage(Derivative Intraday)

        [JsonProperty("brkage_d_m")]
        public decimal BrokerageForDerivativeMargin { get; set; }  //Brokerage(Derivative Margin)

        [JsonProperty("brkage_d_h")]
        public decimal BrokerageForDerivativeHighLeverage { get; set; }  //Brokerage(Derivative High Leverage)

        [JsonProperty("brkage_d_b")]
        public decimal BrokerageForDerivativeBracketOrder { get; set; }  //Brokerage(Derivative Bracket Order)

        [JsonProperty("brkage_f_i")]
        public decimal BrokerageForFxIntraday { get; set; }//Brokerage(FX Intraday)

        [JsonProperty("brkage_f_m")]
        public decimal BrokerageForFxMargin { get; set; }  //Brokerage(FX Margin)

        [JsonProperty("brkage_f_h")]
        public decimal BrokerageForFxBracketHighLeverage { get; set; }  //Brokerage(FX High Leverage)

        [JsonProperty("brkage_f_b")]
        public decimal BrokerageForFxBracketOrder { get; set; }  //Brokerage(FX Bracket Order)

        [JsonProperty("brkage_c_i")]
        public decimal BrokerageForCommodityIntraday { get; set; }  //Brokerage(Commodity Intraday)

        [JsonProperty("brkage_c_m")]
        public decimal BrokerageForCommodityMargin { get; set; }//Brokerage(Commodity Margin)

        [JsonProperty("brkage_c_h")]
        public decimal BrokerageForCommodityHighLeverage { get; set; }//Brokerage(Commodity High Leverage)

        [JsonProperty("brkage_c_b")]
        public decimal BrokerageForCommodityBracketOrder { get; set; }//Brokerage(Commodity Bracket Order)

        [JsonProperty("mr_fx_u")]
        public decimal MRFxUsed { get; set; }//MR fx used

        [JsonProperty("mr_eqt_u")]
        public decimal MREquityUsed { get; set; }//MR Equity used

        [JsonProperty("mr_sell")]
        public decimal MRSellCredit { get; set; }//MR sell credit

        [JsonProperty("mr_t1sell")]
        public decimal MRT1SellCredit { get; set; }//MR t1 sell credit

        [JsonProperty("mr_eqt_a")]
        public decimal MREquityAllocated { get; set; } //MR equity allocated

        [JsonProperty("mr_der_a")]
        public decimal MRDerivativesAllocated { get; set; }       // MR derivatives allocated

        [JsonProperty("mr_fx_a")]
        public decimal MRFxAllocated { get; set; } //MR fx allocated

        [JsonProperty("mr_com_a")]
        public decimal MRCommodityAllocated { get; set; }       //MR commodity allocated

        [JsonProperty("blk_amt")]
        public decimal BlockAmount { get; set; }

        [JsonProperty("aux_daycash")]
        public decimal AuxDayCash { get; set; }

        [JsonProperty("aux_brkcollamt")]
        public decimal AuxBrokerCollateralAmount { get; set; }

        [JsonProperty("aux_unclearedcash")]
        public decimal AuxUnclearedCash { get; set; }

        [JsonProperty("remarks_amt")]
        public decimal RemarksAmount { get; set; }

    }
}
