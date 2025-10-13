using System.Data;

namespace DailyRunner
{
    internal class DbChannelObject
    {
        public string TvpName { get; set; } = string.Empty;
        public string StoredProcedureName { get; set; } = string.Empty;
        public required DataTable Records { get; set; }
    }
}
