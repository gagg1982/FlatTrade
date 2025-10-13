using FlatTrade.Common.Types.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FlatTrade.UserManager
{
    public class UserDetailsResponse : BaseErrorMessageResponse
    {
        [JsonProperty("exarr")]
        public List<Exchange> Exchanges { get; set; } = [];

        [JsonProperty("orarr")]
        public List<PriceType> PriceTypes { get; set; } = [];

        [JsonProperty("prarr")]
        public List<ProductExchangeList> ProductTypes { get; set; } = [];

        [JsonProperty("brkname")]
        public string BrokerName { get; set; } = string.Empty;

        [JsonProperty("brnchid")]
        public string BranchId { get; set; } = string.Empty;

        [JsonProperty("uid")]
        public string UserId { get; set; } = string.Empty;

        [JsonProperty("uname")]
        public string UserName { get; set; } = string.Empty;

        [JsonProperty("email")]
        public string Email { get; set; } = string.Empty;

        [JsonProperty("actid")]
        public string AccountId { get; set; } = string.Empty;

        [JsonProperty("m_num")]
        public string MobileNumber { get; set; } = string.Empty;

        [JsonProperty("uprev")]
        public string UserType { get; set; } = string.Empty;

        [JsonProperty("access_type")]
        public List<AccessType> AccessType { get; set; } = [];

        [JsonProperty("request_time")]
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime RequestDateTime { get; set; }
    }

}
