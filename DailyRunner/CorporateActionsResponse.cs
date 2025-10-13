using Newtonsoft.Json;

namespace DailyRunner
{
    internal class CorporateActionsResponse
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; } = string.Empty;

        [JsonProperty("comp")]
        public string CompanyName { get; set; } = string.Empty;

        [JsonProperty("series")]
        public string Series { get; set; } = string.Empty;

        [JsonProperty("subject")]
        public string Purpose { get; set; } = string.Empty;

        [JsonProperty("faceval")]
        public int FaceValue { get; set; }

        [JsonProperty("exDate")]
        public DateOnly ExDate { get; set; }

        [JsonProperty("recDate")]
        public DateOnly RecordDate { get; set; }

        [JsonProperty("bcStartDate")]
        public DateOnly BookClosureStartDate { get; set; }

        [JsonProperty("bcEndDate")]
        public DateOnly BookClosureEndDate { get; set; }

        [JsonProperty("isin")]
        public string Isin { get; set; } = string.Empty;
    }
}
