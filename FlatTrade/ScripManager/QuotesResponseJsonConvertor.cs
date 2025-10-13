using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FlatTrade.ScripManager
{
    public class QuotesResponseJsonConvertor : JsonConverter<QuotesResponse>
    {
        public override QuotesResponse ReadJson(JsonReader reader, Type objectType, QuotesResponse? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject jsonObject = JObject.Load(reader);
            QuotesResponse response = existingValue ?? new QuotesResponse();

            // Populate standard properties using the default serializer
            // This is more efficient than manually mapping each standard property.
            serializer.Populate(jsonObject.CreateReader(), response);

            // Dictionaries to temporarily hold the parsed market depth components
            // Key: Level (1 to 5)
            Dictionary<int, decimal> bp = [];
            Dictionary<int, long> bq = [];
            Dictionary<int, int> bo = [];

            Dictionary<int, decimal> sp = [];
            Dictionary<int, long> sq = [];
            Dictionary<int, int> so = [];

            // Iterate through all properties in the JSON object
            foreach (JProperty property in jsonObject.Properties())
            {
                string propertyName = property.Name;
                JToken? propertyValue = property.Value;

                // Try to parse dynamic fields
                if (propertyName.StartsWith("bp") && int.TryParse(propertyName.AsSpan(2), out int levelBp))
                {
                    bp[levelBp] = propertyValue?.ToObject<decimal>() ?? 0m;
                }
                else if (propertyName.StartsWith("bq") && int.TryParse(propertyName.AsSpan(2), out int levelBq))
                {
                    bq[levelBq] = propertyValue?.ToObject<long>() ?? 0L;
                }
                else if (propertyName.StartsWith("bo") && int.TryParse(propertyName.AsSpan(2), out int levelBo))
                {
                    bo[levelBo] = propertyValue?.ToObject<int>() ?? 0;
                }
                else if (propertyName.StartsWith("sp") && int.TryParse(propertyName.AsSpan(2), out int levelSp))
                {
                    sp[levelSp] = propertyValue?.ToObject<decimal>() ?? 0m;
                }
                else if (propertyName.StartsWith("sq") && int.TryParse(propertyName.AsSpan(2), out int levelSq))
                {
                    sq[levelSq] = propertyValue?.ToObject<long>() ?? 0L;
                }
                else if (propertyName.StartsWith("so") && int.TryParse(propertyName.AsSpan(2), out int levelSo))
                {
                    so[levelSo] = propertyValue?.ToObject<int>() ?? 0;
                }
            }

            // Reconstruct BestBids and BestAsks lists
            for (int i = 1; i <= 5; i++) // Assuming levels 1 to 5
            {
                if (bp.TryGetValue(i, out decimal bpv) && bq.TryGetValue(i, out long bqv) && bo.TryGetValue(i, out int bov))
                {
                    response.BestBids.Add(new MarketDepthLevel
                    {
                        Price = bpv,
                        Quantity = bqv,
                        Orders = bov
                    });
                }
                if (sp.TryGetValue(i, out decimal spv) && sq.TryGetValue(i, out long sqv) && so.TryGetValue(i, out int sov))
                {
                    response.BestAsks.Add(new MarketDepthLevel
                    {
                        Price = spv,
                        Quantity = sqv,
                        Orders = sov
                    });
                }
            }

            return response;
        }

        public override void WriteJson(JsonWriter writer, QuotesResponse? value, JsonSerializer serializer)
        {
            // This method is for serialization (converting C# object to JSON).
            // If you only need deserialization, you can leave this NotImplementedException
            // or provide a full implementation if you ever need to serialize this object back to JSON.
            // For simplicity in this answer, we'll leave it as a basic implementation.
            JObject obj = JObject.FromObject(value!);

            // If you want to serialize the lists back to bp1, sp1 etc., you'd need to
            // iterate value.BestBids and add properties like "bp1", "bq1", "bo1" etc.
            // and then remove the original BestBids/BestAsks properties.
            // For now, it will serialize BestBids/BestAsks as lists.
            obj.WriteTo(writer);
        }
    }
}
