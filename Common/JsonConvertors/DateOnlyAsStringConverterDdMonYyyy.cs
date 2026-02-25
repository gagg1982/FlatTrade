using Newtonsoft.Json;
using System.Globalization;


namespace Common.JsonConvertors
{
    public class DateOnlyAsStringConverterDdMonYyyy : JsonConverter<DateOnly>
    {
        // Define the specific date format string. ISO 8601 is generally recommended.
        private static readonly string[] DateFormat = {"dd-MMM-yyyy", "dd-MM-yyyy"};
        private const string OutputFormat = "dd-MMM-yyyy";
        /// <summary>
        /// Reads the JSON representation of the object.
        /// Converts a JSON string to a DateOnly.
        /// </summary>
        /// <param name="reader">The JsonReader to read from.</param>
        /// <param name="objectType">Type of the object (e.g., typeof(DateOnly), typeof(DateOnly?)).</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="hasExistingValue">A boolean indicating if existingValue has been set.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The deserialized DateOnly value.</returns>
        public override DateOnly ReadJson(JsonReader reader, Type objectType, DateOnly existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            // Handle null values (for nullable DateOnly? properties)
            if (reader.TokenType == JsonToken.Null)
            {
                // If the target is a nullable DateOnly?, return default (which is null).
                // If it's a non-nullable DateOnly, you might throw an error or return default(DateOnly).
                if (Nullable.GetUnderlyingType(objectType) != null)
                    return default; // Returns default(DateOnly) for DateOnly, null for DateOnly?
                throw new JsonSerializationException("Cannot convert null value to non-nullable DateOnly.");
            }

            string? dateString = null;

            // Expect the JSON token to be a string
            if (reader.TokenType == JsonToken.String)
            {
                dateString = reader.Value?.ToString();
            }
            else
            {
                throw new JsonSerializationException($"Unexpected token type {reader.TokenType} when parsing DateOnly. Expected String.");
            }

            if (string.IsNullOrEmpty(dateString) || string.CompareOrdinal(dateString,"-") == 0)
            {
                // Decide how to handle empty strings: return default(DateOnly), throw, etc.
                return default; // Returns 0001-01-01 for DateOnly
            }

                // Parse the string to DateOnly using ParseExact for strict format matching
            if (DateOnly.TryParseExact(dateString, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
                return result;

            throw new JsonSerializationException(
                    $"Cannot convert string '{dateString}' to DateOnly. Expected format '{DateFormat}'.");

        }

        /// <summary>
        /// Writes the JSON representation of the object.
        /// Converts a DateOnly value to a JSON string.
        /// </summary>
        /// <param name="writer">The JsonWriter to write to.</param>
        /// <param name="value">The DateOnly value to write.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, DateOnly value, JsonSerializer serializer)
        {
            // Convert the DateOnly value to a string using the defined format and InvariantCulture
            writer.WriteValue(value.ToString(OutputFormat, CultureInfo.InvariantCulture));
        }
    }
}
