using FlatTrade.Common.Types.Base;
using Newtonsoft.Json;

namespace FlatTrade.UserManager
{
    public class ProductExchangeList
    {
        [JsonProperty("prd")]
        public ProductType ProductType { get; set; }

        [JsonProperty("s_prdt_ali")]
        public ProductName ProductName { get; set; }

        [JsonProperty("exch")]
        public List<Exchange> Exchanges { get; set; } = [];

    }
}
