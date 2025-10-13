using Newtonsoft.Json;
using System.Globalization;


namespace FlatTrade.Common.JsonConvertors
{
    /// <summary>
    /// Custom JsonConverter for converting long values to strings during serialization
    /// and back from strings to long during deserialization.
    /// Uses InvariantCulture for consistent string representation.
    /// </summary>
    public class LongAsStringConverter : JsonConverter<long>
    {
        /// <summary>
        /// Reads the JSON representation of the object.
        /// Converts a JSON string to a long.
        /// </summary>
        /// <param name="reader">The JsonReader to read from.</param>
        /// <param name="objectType">Type of the object (e.g., typeof(long), typeof(long?)).</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="hasExistingValue">A boolean indicating if existingValue has been set.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The deserialized long value.</returns>
        public override long ReadJson(JsonReader reader, Type objectType, long existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            // Handle null values (for nullable long? properties)
            if (reader.TokenType == JsonToken.Null)
            {
                // If the target is a nullable long?, return default (which is null).
                // If it's a non-nullable long, you might throw an error or return 0.
                if (Nullable.GetUnderlyingType(objectType) != null)
                    return default; // Returns 0 for long, null for long?
                throw new JsonSerializationException("Cannot convert null value to non-nullable long.");
            }

            string? stringValue = null;

            // Expect the JSON token to be a string
            if (reader.TokenType == JsonToken.String)
            {
                stringValue = reader.Value?.ToString();
            }
            // Optionally, if the JSON might sometimes send a direct number, handle that too
            else if (reader.TokenType == JsonToken.Integer || reader.TokenType == JsonToken.Float) // Float for safety, though long should be integer
            {
                stringValue = Convert.ToString(reader.Value, CultureInfo.InvariantCulture);
            }
            else
            {
                throw new JsonSerializationException($"Unexpected token type {reader.TokenType} when parsing long. Expected String, Integer, or Float.");
            }

            if (string.IsNullOrEmpty(stringValue) || string.CompareOrdinal(stringValue, "NA") == 0)
            {
                // Decide how to handle empty strings: return 0L, throw, etc.
                return 0L; // Default for long
            }

            try
            {
                // Parse the string to long using InvariantCulture for consistent parsing
                return long.Parse(stringValue, CultureInfo.InvariantCulture);
            }
            catch (FormatException ex)
            {
                throw new JsonSerializationException(
                    $"Cannot convert string '{stringValue}' to long. Expected a valid integer string.", ex);
            }
            catch (OverflowException ex)
            {
                throw new JsonSerializationException(
                   $"String '{stringValue}' is too large or too small for type 'long'.", ex);
            }
        }

        /// <summary>
        /// Writes the JSON representation of the object.
        /// Converts a long value to a JSON string.
        /// </summary>
        /// <param name="writer">The JsonWriter to write to.</param>
        /// <param name="value">The long value to write.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, long value, JsonSerializer serializer)
        {
            // Convert the long value to a string using InvariantCulture
            writer.WriteValue(value.ToString(CultureInfo.InvariantCulture));
        }
    }

}
