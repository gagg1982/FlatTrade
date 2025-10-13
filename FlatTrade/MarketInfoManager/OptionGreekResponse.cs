using Newtonsoft.Json;

namespace FlatTrade.MarketInfoManager
{
    public class OptionGreekResponse : BaseErrorMessageResponse
    {
        [JsonProperty("cal_price")]
        public double CallPrice { get; set; }

        [JsonProperty("put_price")]
        public double PutPrice { get; set; }

        [JsonProperty("cal_delta")]
        public double CallDelta { get; set; }

        [JsonProperty("put_delta")]
        public double PutDelta { get; set; }

        [JsonProperty("cal_gamma")]
        public double CallGamma { get; set; }

        [JsonProperty("put_gamma")]
        public double PutGamma { get; set; }

        [JsonProperty("cal_theta")]
        public double CallTheta { get; set; }

        [JsonProperty("put_theta")]
        public double PutTheta { get; set; }

        [JsonProperty("cal_rho")]
        public double CallRho { get; set; }

        [JsonProperty("put_rho")]
        public double PutRho { get; set; }

        [JsonProperty("cal_vego")]
        public double CallVego { get; set; }

        [JsonProperty("put_vego")]
        public double PutVego { get; set; }


    }
}
