using Newtonsoft.Json;
using System.Globalization;

namespace Common.JsonConvertors
{
    public class DdMmYyyyHhMmSsConverter : JsonConverter<DateTime>
    {
        // Define the exact format string for parsing and formatting.
        // "dd"  = day (01-31)
        // "MM"  = month (01-12)
        // "yyyy" = year (e.g., 2025)
        // "HH"  = hour in 24-hour format (00-23)
        // "mm"  = minute (00-59)
        // "ss"  = second (00-59)
        private const string DateTimeFormat = "ddMMyyyyHHmmss";

        /// <summary>
        /// Reads the JSON representation of the object.
        /// Converts a string in "ddMMyyyyHHmmss" format to a DateTime object.
        /// </summary>
        /// <param name="reader">The JsonReader to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="hasExistingValue">A boolean indicating if existingValue has been set.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The DateTime value.</returns>
        public override DateTime ReadJson(JsonReader reader, Type objectType, DateTime existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                // Handle null values for nullable DateTime? properties
                if (Nullable.GetUnderlyingType(objectType) != null)
                    return default; // Returns null for Nullable<DateTime>
                throw new JsonSerializationException($"Cannot convert null value to non-nullable DateTime.");
            }

            if (reader.TokenType == JsonToken.String)
            {
                string? dateString = reader.Value?.ToString();

                if (string.IsNullOrEmpty(dateString))
                {
                    // Handle empty string as null or default DateTime
                    if (Nullable.GetUnderlyingType(objectType) != null)
                        return default;
                    throw new JsonSerializationException($"Cannot convert empty string to non-nullable DateTime.");
                }

                try
                {
                    // Use DateTime.ParseExact with InvariantCulture for precise and consistent parsing.
                    // DateTimeStyles.None ensures strict parsing (no leading/trailing whitespace, etc.).
                    return DateTime.ParseExact(dateString, DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None);
                }
                catch (FormatException ex)
                {
                    throw new JsonSerializationException(
                        $"Cannot convert string '{dateString}' to DateTime. Expected format '{DateTimeFormat}'.", ex);
                }
            }
            else
            {
                // If the token is not a string (e.g., number, boolean, object), it's an unexpected format.
                throw new JsonSerializationException($"Unexpected token type {reader.TokenType} when parsing DateTime. Expected String.");
            }
        }

        /// <summary>
        /// Writes the JSON representation of the object.
        /// Converts a DateTime object to a string in "ddMMyyyyHHmmss" format.
        /// </summary>
        /// <param name="writer">The JsonWriter to write to.</param>
        /// <param name="value">The DateTime value to write.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, DateTime value, JsonSerializer serializer)
        {
            // Format the DateTime object back to the desired string format.
            // Using InvariantCulture for consistent formatting.
            writer.WriteValue(value.ToString(DateTimeFormat, CultureInfo.InvariantCulture));
        }
    }

}

