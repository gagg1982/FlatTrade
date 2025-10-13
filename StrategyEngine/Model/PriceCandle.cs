namespace StrategyEngine.Model
{
    public class PriceCandle : IComparable<PriceCandle>
    {
        public DateTime TimeStamp { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }        
        public long Volume { get; set; }

        public int CompareTo(PriceCandle? other)
        {
            if (other is null) 
                return 1;
            return TimeStamp.CompareTo(other.TimeStamp);
        }        
    }
}
