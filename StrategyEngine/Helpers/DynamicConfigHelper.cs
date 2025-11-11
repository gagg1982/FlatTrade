using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Dynamic;

public static class DynamicConfigHelper
{
    /// <summary>
    /// Reads a JSON array section (like "RmsRules") and returns a list of dictionaries.
    /// </summary>
    public static List<Dictionary<string, object?>> GetDynamicList(IConfiguration config, string sectionName)
    {
        var section = config.GetSection(sectionName);
        var list = new List<Dictionary<string, object?>>();

        foreach (var child in section.GetChildren())
        {
            var dict = new Dictionary<string, object?>();
            foreach (var grandChild in child.GetChildren())
                dict[grandChild.Key] = grandChild.Value;
            list.Add(dict);
        }

        return list;
    }

    /// <summary>
    /// Finds a single rule by name and returns it as a dictionary.
    /// </summary>
    public static Dictionary<string, object?>? GetRule(IConfiguration config, string sectionName, string ruleName)
    {
        var rules = GetDynamicList(config, sectionName);

        return rules.FirstOrDefault(r =>
            r.TryGetValue("Name", out var nameObj) &&
            string.Equals(nameObj?.ToString(), ruleName, StringComparison.OrdinalIgnoreCase));
    }

    public static bool GetBool(this IDictionary<string, object?> dict, string key, bool defaultValue = false)
    {
        if (dict.TryGetValue(key, out var value) && value != null)
        {
            if (bool.TryParse(value.ToString(), out var result))
                return result;

            // Sometimes "1"/"0" or numeric flags
            if (int.TryParse(value.ToString(), out var i))
                return i != 0;
        }
        return defaultValue;
    }

    public static int GetInt(this IDictionary<string, object?> dict, string key, int defaultValue = 0)
    {
        if (dict.TryGetValue(key, out var value) && value != null && int.TryParse(value.ToString(), out var result))
            return result;
        return defaultValue;
    }

    public static double GetDouble(this IDictionary<string, object?> dict, string key, double defaultValue = 0)
    {
        if (dict.TryGetValue(key, out var value) && value != null && double.TryParse(value.ToString(), out var result))
            return result;
        return defaultValue;
    }

    public static decimal GetDecimal(this IDictionary<string, object?> dict, string key, decimal defaultValue = 0)
    {
        if (dict.TryGetValue(key, out var value) && value != null && decimal.TryParse(value.ToString(), out var result))
            return result;
        return defaultValue;
    }
}
