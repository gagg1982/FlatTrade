using Newtonsoft.Json.Converters;


namespace FlatTrade.Common.JsonConvertors
{
    public class CustomIsoDateTimeConverter : IsoDateTimeConverter
    {
        public CustomIsoDateTimeConverter()
        {
            DateTimeFormat = "dd-MM-yyyyHH:mm:ss";
        }
    }
}
