using Common.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.JsonConvertors
{
    public class PriceCandleConverter : JsonConverter<PriceCandle>
    {
        public override PriceCandle ReadJson(
            JsonReader reader,
            Type objectType,
            PriceCandle? existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            var arr = JArray.Load(reader);
            return new PriceCandle
            {
                StartTimeStamp = arr[0].ToObject<DateTimeOffset>().DateTime,
                Open = arr[1].ToObject<decimal>(),
                High = arr[2].ToObject<decimal>(),
                Low = arr[3].ToObject<decimal>(),
                Close = arr[4].ToObject<decimal>(),
                Volume = arr[5].ToObject<long>()
                //OpenInterest = arr[6].ToObject<long>()
            };
        }

        public override void WriteJson(JsonWriter writer, PriceCandle? value, JsonSerializer serializer)
            => throw new NotImplementedException();
    }
}
