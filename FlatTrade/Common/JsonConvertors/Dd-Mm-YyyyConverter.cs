using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace FlatTrade.Common.JsonConvertors
{
    /// <summary>
    /// Custom JsonConverter for converting a JSON array of strings in "ddMMyyyy" format to a List<DateTime>
    /// and List<DateTime> back to a JSON array of "ddMMyyyy" strings.
    /// </summary>
    public class DdMmYyyyListConverter : JsonConverter<List<DateTime>>
    {
        // Define the exact date format expected for the strings within the list.
        private const string DateFormat = "dd-MM-yyyy";

        /// <summary>
        /// Reads the JSON representation of the object.
        /// Converts a JSON array of "ddMMyyyy" strings to a List<DateTime>.
        /// </summary>
        /// <param name="reader">The JsonReader to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="hasExistingValue">A boolean indicating if existingValue has been set.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>A List<DateTime>.</returns>
        public override List<DateTime>? ReadJson(JsonReader reader, Type objectType, List<DateTime>? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return default; // Return null if the JSON array itself is null
            }

            if (reader.TokenType == JsonToken.StartArray)
            {
                JArray array = JArray.Load(reader);
                List<DateTime> dateTimes = new List<DateTime>();

                foreach (JToken item in array)
                {
                    if (item.Type == JTokenType.String)
                    {
                        string? dateString = item.Value<string>();
                        if (string.IsNullOrEmpty(dateString))
                        {
                            // Decide how to handle empty strings: skip, add default, or throw
                            // For this example, we'll throw an error.
                            throw new JsonSerializationException($"Cannot convert empty string in array to DateTime. Expected format '{DateFormat}'.");
                        }
                        try
                        {
                            // Parse each string using the defined format and InvariantCulture.
                            dateTimes.Add(DateTime.ParseExact(dateString, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None));
                        }
                        catch (FormatException ex)
                        {
                            throw new JsonSerializationException(
                                $"Cannot convert string '{dateString}' in array to DateTime. Expected format '{DateFormat}'.", ex);
                        }
                    }
                    else if (item.Type == JTokenType.Null)
                    {
                        // Decide how to handle null items in the array for non-nullable DateTime.
                        // For this example, we'll throw an error for nulls if the list is List<DateTime>
                        // If it were List<DateTime?>, you could add null.
                        throw new JsonSerializationException($"Cannot convert null item in array to non-nullable DateTime.");
                    }
                    else
                    {
                        throw new JsonSerializationException($"Unexpected token type {item.Type} in array when parsing DateTime. Expected String.");
                    }
                }
                return dateTimes;
            }
            else
            {
                throw new JsonSerializationException($"Unexpected token type {reader.TokenType} when parsing List<DateTime>. Expected StartArray.");
            }
        }

        /// <summary>
        /// Writes the JSON representation of the object.
        /// Converts a List<DateTime> to a JSON array of "ddMMyyyy" strings.
        /// </summary>
        /// <param name="writer">The JsonWriter to write to.</param>
        /// <param name="value">The List<DateTime> to write.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, List<DateTime>? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull(); // Write null if the list itself is null
                return;
            }

            writer.WriteStartArray(); // Start JSON array

            foreach (DateTime dateTime in value)
            {
                // Format each DateTime object to the desired string format and write it.
                writer.WriteValue(dateTime.ToString(DateFormat, CultureInfo.InvariantCulture));
            }

            writer.WriteEndArray(); // End JSON array
        }
    }

}
