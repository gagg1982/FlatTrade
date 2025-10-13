using FlatTrade.Common.Types.Base;
using Newtonsoft.Json;

namespace FlatTrade.OrderManager
{
    public class PlaceOrderParameterResponse
    {
        [JsonProperty("actid")]
        public string AccountId { get; set; } = string.Empty;

        [JsonProperty("trantype")]
        public TransactionType TransactionType { get; set; }

        [JsonProperty("prctyp")]
        public PriceType PriceType { get; set; } //LMT/MKT

        [JsonProperty("qty")]
        public long Quantity { get; set; }

        [JsonProperty("prc")]
        public decimal Price { get; set; }

        [JsonProperty("C")]
        public ProductType C { get; set; }

        [JsonProperty("s_prdt_ali")]
        public ProductName ProductDisplayName { get; set; }

        [JsonProperty("prd")]
        public ProductType ProductType { get; set; }

        [JsonProperty("ordersource")]
        public AccessType OrderSource { get; set; } //	MOB / WEB / TT	Used to generate exchange info fields.

        [JsonProperty("ipaddr")]
        public string IpAddress { get; set; } = string.Empty;

    }
}
