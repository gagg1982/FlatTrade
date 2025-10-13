using System.ComponentModel;
using System.Globalization;

namespace FlatTrade.Common.Helpers
{
    public class DateTimeDDMMYYYYHHMMSSConvertor : TypeConverter
    {
        private readonly string[] formats = { "dd-MM-yyyy HH:mm:ss", "yyyy-MM-dd HH:mm:ss", "yyyy-MM-ddTHH:mm:ss" };

        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            if (value is string str)
            {
                if (DateTime.TryParseExact(str, formats, culture, DateTimeStyles.None, out var dt))
                {
                    return dt; // this will map fine to datetime2 in SQL
                }
                throw new FormatException($"Invalid datetime2 format: {str}");
            }
            return base.ConvertFrom(context, culture, value);
        }
    }
}
