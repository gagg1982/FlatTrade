using System.ComponentModel;
using System.Globalization;

namespace Common.Helpers
{
    public class DateOnlyConvertor : TypeConverter
    {
        private const string Format = "dd-MM-yyyy";

        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        {
            // Allow conversion from string
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            if (value is string str && !string.IsNullOrWhiteSpace(str))
            {
                // Parse the string strictly in DD-MM-YYYY format
                return DateTime.ParseExact(str, Format, CultureInfo.InvariantCulture);
            }

            return base.ConvertFrom(context, culture, value);
        }
    }
}
