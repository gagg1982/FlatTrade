namespace DailyRunner
{
    public class CsvChannelObject
    {
        public string FileName { get; set; } = string.Empty;
        public string Header { get; set; } = string.Empty;
        public IEnumerable<string> Records { get; set; } = [];
    }
}
