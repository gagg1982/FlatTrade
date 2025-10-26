using System.Data;
using System.Reflection;

namespace DailyRunner.Helpers
{
    internal static class Utility
    {
        public async static Task WhenAllSafe(params Task?[] tasks)
        {
            await Task.WhenAll(tasks.Where(t => t != null)!);
        }
        
        public static DataTable ToDataTable(IEnumerable<Dictionary<string, object?>> dictionaries)
        {
            ArgumentNullException.ThrowIfNull(dictionaries);

            var list = dictionaries.ToList();
            var dt = new DataTable("DictionaryData");

            if (list.Count == 0)
                return dt;

            // Gather all unique keys across all dictionaries (handle missing keys)
            var allKeys = list.SelectMany(d => d.Keys).Distinct().ToList();

            // Create columns dynamically (use object type for flexibility)
            foreach (var key in allKeys)
                dt.Columns.Add(key, typeof(object));

        
            foreach (var dict in list)
            {
                var row = dt.NewRow();
                foreach (var key in allKeys)
                {
                    if (dict.TryGetValue(key, out var value))
                        row[key] = value ?? DBNull.Value;
                    else
                        row[key] = DBNull.Value;
                }
                dt.Rows.Add(row);
            }

            return dt;
        }
       
        public static DataTable ToDataTable<T>(IEnumerable<T> objects)
        {
            ArgumentNullException.ThrowIfNull(nameof(objects));

            var dt = new DataTable(typeof(T).Name);

            // Only properties declared in T (ignore inherited properties)
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                                      .Where(p => p.CanRead)
                                      .ToArray();

            // Create DataTable columns
            foreach (var prop in properties)
            {
                var propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                // Map enums to string if desired, otherwise keep as int
                if (propType.IsEnum)
                    dt.Columns.Add(prop.Name, typeof(string));
                else
                    dt.Columns.Add(prop.Name, propType);
            }

            // Fill rows
            foreach (var obj in objects)
            {
                var row = dt.NewRow();
                foreach (var prop in properties)
                {
                    var value = prop.GetValue(obj);
                    if (value == null)
                        row[prop.Name] = DBNull.Value;
                    else if (prop.PropertyType.IsEnum)
                        row[prop.Name] = value.ToString();
                    else
                        row[prop.Name] = value;
                }
                dt.Rows.Add(row);
            }
            return dt;
        }

        public static (string header, IEnumerable<string> rows) ToCsv(IEnumerable<Dictionary<string, object?>> dictionaries)
        {
            if (dictionaries == null || !dictionaries.Any())
                return (string.Empty, Enumerable.Empty<string>());

            // Use the keys of the first dictionary as headers
            var headers = dictionaries.First().Keys.ToList();
            var header = string.Join(",", headers);

            // Build each row
            var rows = dictionaries.Select(dict =>
            {
                var values = headers.Select(h =>
                {
                    dict.TryGetValue(h, out var value);

                    if (value == null)
                        return "";

                    var strVal = value.ToString() ?? "";

                    // Escape commas, quotes, or newlines
                    if (strVal.Contains(',') || strVal.Contains('"') || strVal.Contains('\n'))
                        strVal = $"\"{strVal.Replace("\"", "\"\"")}\"";

                    return strVal;
                });

                return string.Join(",", values);
            });

            return (header, rows);
        }

        public static (string header, IEnumerable<string> rows) ToCsv<T>(IEnumerable<T> items)
        {
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            var header = string.Join(",", properties.Select(p => p.Name));

            // Rows: convert each object to comma-separated string of values
            var rows = items.Select(item =>
            {
                var values = properties.Select(p =>
                {
                    var value = p.GetValue(item, null);

                    if (value == null) return "";

                    if (value is string strVal && strVal.Contains(','))
                        return $"\"{strVal}\"";
                    if (p.PropertyType.IsEnum)
                        return value.ToString();
                    return value.ToString();
                });

                return string.Join(",", values);
            }).ToList();

            return (header, rows);
        }
    }
}
