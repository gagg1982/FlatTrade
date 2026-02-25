using System.Data;
using System.Reflection;

namespace Common.Helpers
{
    public static class DataReaderHelper
    {
        public static bool HasColumn(this IDataRecord reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        [AttributeUsage(AttributeTargets.Property)]
        public class ColumnNameAttribute : Attribute
        {
            public string Name { get; }
            public ColumnNameAttribute(string name) => Name = name;
        }


        [AttributeUsage(AttributeTargets.Property)]
        public class TransformAttribute : Attribute
        {
            public Type TransformerType { get; }

            public TransformAttribute(Type transformerType)
            {
                TransformerType = transformerType;
            }
        }

        public interface IValueTransformer
        {
            object? Transform(object? input);
        }

        public class BigIntToStringTransformer : IValueTransformer
        {
            public object? Transform(object? input)
            {
                if (input == null || input is DBNull)
                    return string.Empty;

                // Convert numeric value to string
                return input switch
                {
                    long l => l.ToString(),
                    int i => i.ToString(),
                    decimal d => d.ToString(),
                    _ => input.ToString() ?? string.Empty
                };
            }
        }

        public class EnumTransformer<TEnum> : IValueTransformer where TEnum : struct, Enum
        {
            public object? Transform(object? input)
            {
                if (input is string s && !string.IsNullOrWhiteSpace(s))
                {
                    // Try parse the string into the enum type
                    if (Enum.TryParse<TEnum>(s, true, out var result))
                        return result;
                }

                // Return default value if parsing fails
                return default(TEnum);
            }
        }

        public static T MapReaderTo<T>(IDataReader reader) where T : new()
        {
            var obj = new T();
            var props = typeof(T).GetProperties();

            foreach (var prop in props)
            {
                var columnAttr = prop.GetCustomAttribute<ColumnNameAttribute>();
                var transformAttr = prop.GetCustomAttribute<TransformAttribute>();

                var columnName = columnAttr?.Name ?? prop.Name;

                if (!reader.HasColumn(columnName) || reader[columnName] is DBNull)
                    continue;

                object? value = reader[columnName];

                // Apply transformation if present
                if (transformAttr?.TransformerType is not null)
                {
                    if (Activator.CreateInstance(transformAttr.TransformerType) is IValueTransformer transformer)
                        value = transformer.Transform(value);
                }

                prop.SetValue(obj, value);
            }

            return obj;
        }

    }
}
