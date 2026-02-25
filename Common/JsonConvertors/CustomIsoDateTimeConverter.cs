using Newtonsoft.Json.Converters;


namespace Common.JsonConvertors
{
    public class CustomIsoDateTimeConverter : IsoDateTimeConverter
    {
        public CustomIsoDateTimeConverter()
        {
            DateTimeFormat = "dd-MM-yyyyHH:mm:ss";
        }
    }
}
