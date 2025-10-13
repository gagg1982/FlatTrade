using FlatTrade.Common.JsonConvertors;
using FlatTrade.Common.Types.Base;
using Newtonsoft.Json;

namespace FlatTrade.MarketInfoManager
{
    public class PositionRequest
    {
        [JsonProperty("exch")]
        public required Exchange Exchange { get; set; }

        [JsonProperty("instname")]
        public required InstrumentName InstrumentName { get; set; }

        [JsonProperty("symname")]
        public required string SymbolName { get; set; }

        [JsonProperty("expd")]
        [JsonConverter(typeof(DateOnlyAsStringConverter))]
        public required DateOnly ExpiryDate { get; set; }

        [JsonProperty("optt")]
        public required OptionType OptionType { get; set; }

        [JsonProperty("prd")]
        public required ProductType ProductType { get; set; }

        [JsonProperty("strprc")]
        [JsonConverter(typeof(DecimalAsStringConverter))]
        public required decimal StrikePrice { get; set; }

        [JsonProperty("buyqty")]
        [JsonConverter(typeof(LongAsStringConverter))]
        public required long BuyQuantity { get; set; }

        [JsonProperty("sellqty")]
        [JsonConverter(typeof(LongAsStringConverter))]
        public required long SellQuantity { get; set; }

        [JsonProperty("netqty")]
        [JsonConverter(typeof(LongAsStringConverter))]
        public required long NetQuantity { get; set; }
    }
}
