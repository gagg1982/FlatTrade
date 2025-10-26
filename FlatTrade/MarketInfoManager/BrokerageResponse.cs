using Newtonsoft.Json;

namespace FlatTrade.MarketInfoManager
{
    public class BrokerageResponse : BaseErrorMessageResponse
    {
        [JsonProperty("remarks")]
        public string Remarks { get; set; } = string.Empty;

        [JsonProperty("url")]
        public string Url { get; set; } = string.Empty;

        [JsonProperty("brkage_amt")]
        public decimal BrokerageAmount { get; set; }

        [JsonProperty("stt_amt")]
        public decimal SecurityTransactionTax { get; set; }

        [JsonProperty("sebi_chrg")]
        public decimal SebiCharges { get; set; }

        [JsonProperty("exch_chrg")]
        public decimal ExchangeCharges { get; set; }

        [JsonProperty("stamp_duty")]
        public decimal StampDuty { get; set; }

        [JsonProperty("gst")]
        public decimal Gst { get; set; }

        [JsonProperty("ipft_amt")]
        public decimal InvestorProtectionFundTrustAmount { get; set; }

        [JsonProperty("cm_amt")]
        public decimal ClearingMemberAmount { get; set; }

        [JsonProperty("tot_chrg")]
        public decimal TotalCharges { get; set; }

    }
}
