using FlatTrade.Types.Base;
using Newtonsoft.Json;

namespace FlatTrade.MarketInfoManager
{
    public class BasketCriteriaPair
    {
        [JsonProperty("bskt")]
        public BasketType BasketName { get; set; }

        [JsonProperty("crt")]
        public Criteria Criteria { get; set; }
    }
}
