using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Reflection;


namespace FlatTrade.Common.JsonConvertors
{
    public class AllPropertiesToStringConverter<T> : JsonConverter<T>
    {
        /// <summary>
        /// Determines if a given type should be converted to/from string by this converter.
        /// This includes primitive types, string, decimal, DateTime, Guid, and enums.
        /// </summary>
        private static bool IsConvertibleToString(Type type)
        {
            // Get the underlying type if it's a Nullable<T>
            Type actualType = Nullable.GetUnderlyingType(type) ?? type;

            return actualType.IsPrimitive || // Covers int, bool, double, float, long, byte, sbyte, short, ushort, uint, ulong, char
                   actualType == typeof(string) ||
                   actualType == typeof(decimal) ||
                   actualType == typeof(DateTime) ||
                   actualType == typeof(Guid) ||
                   actualType.IsEnum;
        }

        /// <summary>
        /// Writes the JSON representation of the object.
        /// Primitive/value type properties are converted to strings.
        /// Complex types are serialized normally.
        /// </summary>
        public override void WriteJson(JsonWriter writer, T? value, JsonSerializer serializer)
        {
            if (value is null)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteStartObject(); // Start writing the JSON object for the class

            // Iterate through all public instance properties of the object
            foreach (PropertyInfo prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                // Skip properties that cannot be read (e.g., write-only properties)
                if (!prop.CanRead) continue;

                // Determine the JSON property name (from [JsonProperty] attribute or default C# property name)
                string jsonPropertyName = prop.Name;
                var jsonPropertyAttribute = prop.GetCustomAttribute<JsonPropertyAttribute>();
                if (jsonPropertyAttribute != null && !string.IsNullOrEmpty(jsonPropertyAttribute.PropertyName))
                {
                    jsonPropertyName = jsonPropertyAttribute.PropertyName;
                }

                writer.WritePropertyName(jsonPropertyName); // Write the JSON property name

                object? propValue = prop.GetValue(value); // Get the actual value of the C# property

                if (propValue == null)
                {
                    writer.WriteNull(); // If value is null, write JSON null
                }
                else if (IsConvertibleToString(prop.PropertyType))
                {
                    // If it's a type we want to convert to string:
                    string stringRepresentation;

                    // Apply specific formatting for certain types for reliable round-tripping
                    if (prop.PropertyType == typeof(DateTime) || prop.PropertyType == typeof(DateTime?))
                    {
                        // Use ISO 8601 ("o") format for DateTime for precision and culture invariance
                        stringRepresentation = ((DateTime)propValue).ToString("o", CultureInfo.InvariantCulture);
                    }
                    else if (prop.PropertyType == typeof(bool) || prop.PropertyType == typeof(bool?))
                    {
                        // Convert boolean to lowercase string ("true" or "false")
                        stringRepresentation = propValue.ToString()!.ToLowerInvariant();
                    }
                    else if (prop.PropertyType == typeof(decimal) || prop.PropertyType == typeof(decimal?))
                    {
                        // Use InvariantCulture for numeric types to ensure dot as decimal separator
                        stringRepresentation = ((decimal)propValue).ToString(CultureInfo.InvariantCulture);
                    }
                    else if (prop.PropertyType == typeof(double) || prop.PropertyType == typeof(double?))
                    {
                        stringRepresentation = ((double)propValue).ToString(CultureInfo.InvariantCulture);
                    }
                    else if (prop.PropertyType == typeof(float) || prop.PropertyType == typeof(float?))
                    {
                        stringRepresentation = ((float)propValue).ToString(CultureInfo.InvariantCulture);
                    }
                    else if (prop.PropertyType.IsEnum)
                    {
                        // Serialize enums by their name string (e.g., "Active", "Pending")
                        stringRepresentation = propValue.ToString()!;
                    }
                    else
                    {
                        // For other primitive types (int, long, string, Guid, char, etc.), use default ToString()
                        stringRepresentation = propValue.ToString()!;
                    }
                    writer.WriteValue(stringRepresentation); // Write the string value to JSON
                }
                else
                {
                    // For complex types (nested objects, collections like List<T>, arrays),
                    // let the default JsonSerializer handle the recursive serialization.
                    serializer.Serialize(writer, propValue);
                }
            }

            writer.WriteEndObject(); // End writing the JSON object
        }

        /// <summary>
        /// Reads the JSON representation of the object.
        /// Expects primitive/value type properties as strings and converts them back.
        /// Complex types are deserialized normally.
        /// </summary>
        public override T ReadJson(JsonReader reader, Type objectType, T? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return default!; // Handle null JSON object

            // Load the entire JSON object into a JObject for easy property access
            JObject jsonObject = JObject.Load(reader);
            // Create an instance of the target type (T)
            T instance = existingValue ?? (T)Activator.CreateInstance(objectType)!;

            // Iterate through all public instance properties of the C# object
            foreach (PropertyInfo prop in objectType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                // Skip properties that cannot be written to (e.g., read-only properties)
                if (!prop.CanWrite) continue;

                // Determine the JSON property name (from [JsonProperty] attribute or default)
                string jsonPropertyName = prop.Name;
                var jsonPropertyAttribute = prop.GetCustomAttribute<JsonPropertyAttribute>();
                if (jsonPropertyAttribute != null && !string.IsNullOrEmpty(jsonPropertyAttribute.PropertyName))
                {
                    jsonPropertyName = jsonPropertyAttribute.PropertyName;
                }

                JToken? token = jsonObject[jsonPropertyName]; // Get the JSON token for the property

                // Handle missing or null JSON properties
                if (token == null || token.Type == JTokenType.Null)
                {
                    prop.SetValue(instance, null); // Set C# property to its default (null for reference, default for value types)
                    continue;
                }

                if (IsConvertibleToString(prop.PropertyType))
                {
                    // If it's a type we expect as a string in JSON:
                    if (token.Type != JTokenType.String)
                    {
                        throw new JsonSerializationException(
                            $"Expected JSON string for property '{jsonPropertyName}' (C# type '{prop.PropertyType.Name}'), " +
                            $"but found JSON token type '{token.Type}'.");
                    }
                    string stringValue = token.Value<string>()!; // Get the string value from the JSON token

                    try
                    {
                        // Get the actual type, handling Nullable<T>
                        Type actualPropType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                        // Convert the string value back to the original C# property type
                        if (actualPropType == typeof(DateTime))
                        {
                            // Use Parse with RoundtripKind for ISO 8601 strings
                            prop.SetValue(instance, DateTime.Parse(stringValue, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind));
                        }
                        else if (actualPropType == typeof(bool))
                        {
                            prop.SetValue(instance, bool.Parse(stringValue));
                        }
                        else if (actualPropType == typeof(int))
                        {
                            prop.SetValue(instance, int.Parse(stringValue, CultureInfo.InvariantCulture));
                        }
                        else if (actualPropType == typeof(long))
                        {
                            prop.SetValue(instance, long.Parse(stringValue, CultureInfo.InvariantCulture));
                        }
                        else if (actualPropType == typeof(decimal))
                        {
                            prop.SetValue(instance, decimal.Parse(stringValue, CultureInfo.InvariantCulture));
                        }
                        else if (actualPropType == typeof(double))
                        {
                            prop.SetValue(instance, double.Parse(stringValue, CultureInfo.InvariantCulture));
                        }
                        else if (actualPropType == typeof(float))
                        {
                            prop.SetValue(instance, float.Parse(stringValue, CultureInfo.InvariantCulture));
                        }
                        else if (actualPropType == typeof(Guid))
                        {
                            prop.SetValue(instance, Guid.Parse(stringValue));
                        }
                        else if (actualPropType.IsEnum)
                        {
                            // Parse enum from string name (case-insensitive)
                            prop.SetValue(instance, Enum.Parse(actualPropType, stringValue, ignoreCase: true));
                        }
                        else if (actualPropType == typeof(string)) // If the property is already a string
                        {
                            prop.SetValue(instance, stringValue);
                        }
                        // Add more type conversions here if needed (e.g., custom structs)
                    }
                    catch (Exception ex)
                    {
                        throw new JsonSerializationException(
                            $"Error converting string '{stringValue}' to type '{prop.PropertyType.Name}' for property '{jsonPropertyName}'.", ex);
                    }
                }
                else
                {
                    // For complex types (nested objects, collections),
                    // let the default JsonSerializer handle the deserialization.
                    // ToObject<T> on a JToken uses the provided serializer's settings.
                    object? deserializedComplexValue = token.ToObject(prop.PropertyType, serializer);
                    prop.SetValue(instance, deserializedComplexValue);
                }
            }
            return instance;
        }
    }
}
