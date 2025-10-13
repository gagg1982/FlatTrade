using Newtonsoft.Json;
using System.Globalization;

namespace FlatTrade.Common.JsonConvertors
{

    /// <summary>
    /// Custom JsonConverter for converting decimal values to strings during serialization
    /// and back from strings to decimal during deserialization.
    /// Uses InvariantCulture for consistent string representation (dot as decimal separator).
    /// </summary>
    public class DecimalAsStringConverter : JsonConverter<decimal>
    {
        /// <summary>
        /// Reads the JSON representation of the object.
        /// Converts a JSON string to a decimal.
        /// </summary>
        /// <param name="reader">The JsonReader to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="hasExistingValue">A boolean indicating if existingValue has been set.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The decimal value.</returns>
        public override decimal ReadJson(JsonReader reader, Type objectType, decimal existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            bool isNullable = Nullable.GetUnderlyingType(objectType) != null;

            // Handle null values from JSON
            if (reader.TokenType == JsonToken.Null)
            {
                if (isNullable)
                    return default; // For decimal?, default is null
                throw new JsonSerializationException("Cannot convert null value to non-nullable decimal.");
            }

            string? stringValue;

            // Expect the JSON token to be a string
            if (reader.TokenType == JsonToken.String)
            {
                stringValue = reader.Value?.ToString();
            }
            // Optionally, if the JSON might sometimes send a direct number, handle that too
            else if (reader.TokenType == JsonToken.Float || reader.TokenType == JsonToken.Integer)
            {
                // Convert to string for parsing consistency, or directly convert if preferred.
                stringValue = Convert.ToString(reader.Value, CultureInfo.InvariantCulture);
            }
            else
            {
                throw new JsonSerializationException($"Unexpected token type {reader.TokenType} when parsing decimal. Expected String, Float, or Integer.");
            }

            // Handle "NaN" string
            if (stringValue != null && stringValue.Equals("NaN", StringComparison.OrdinalIgnoreCase))
            {
                if (isNullable)
                {
                    // For nullable decimal?, return null
                    return default; // default for decimal? is null
                }
                else
                {
                    // For non-nullable decimal, return 0m or throw an error based on your logic
                    Console.WriteLine("[Warning]: Deserializing 'NaN' to 0m for non-nullable decimal property.");
                    return 0m;
                }
            }

            // Handle empty string
            if (string.IsNullOrEmpty(stringValue))
            {
                // Decide how to handle empty strings: return 0m, throw, etc.
                return 0m;
            }

            try
            {
                // Parse the string to decimal using InvariantCulture for consistent parsing
                return decimal.Parse(stringValue, CultureInfo.InvariantCulture);
            }
            catch (FormatException ex)
            {
                throw new JsonSerializationException(
                    $"Cannot convert string '{stringValue}' to decimal. Expected a valid numeric string or 'NaN'.", ex);
            }
            catch (OverflowException ex)
            {
                throw new JsonSerializationException(
                   $"String '{stringValue}' is too large or too small for type 'decimal'.", ex);
            }
        }

        /// <summary>
        /// Writes the JSON representation of the object.
        /// Converts a decimal value to a JSON string.
        /// </summary>
        /// <param name="writer">The JsonWriter to write to.</param>
        /// <param name="value">The decimal value to write.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, decimal value, JsonSerializer serializer)
        {
            // Convert the decimal value to a string using InvariantCulture
            // This ensures consistent formatting (e.g., dot as decimal separator)
            writer.WriteValue(value.ToString(CultureInfo.InvariantCulture));
        }
    }

}
