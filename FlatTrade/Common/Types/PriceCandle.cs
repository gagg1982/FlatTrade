namespace FlatTrade.Common.Types
{
    public class PriceCandle : IComparable<PriceCandle>
    {
        public DateTime StartTimeStamp { get; set; } = DateTime.MinValue;
        public decimal Open { get; set; } = decimal.MinValue;
        public decimal High { get; set; } = decimal.MinValue;
        public decimal Low { get; set; } = decimal.MaxValue;
        public decimal Close { get; set; } = decimal.MinValue;
        public long Volume { get; set; } = 0;

        public bool PseudoFlag = false;
        public int CompareTo(PriceCandle? other)
        {
            if (other is null)
                return 1;
            return other.StartTimeStamp.CompareTo(StartTimeStamp); //descending order
        }

        public PriceCandle UpdateCandle(PriceCandle other)
        {
            if (StartTimeStamp != other.StartTimeStamp)
                return other;

            if (PseudoFlag)
            {
                Open = Math.Max(Open, other.Open);
                High = Math.Max(other.High, High);
                Low = Math.Min(other.Low, Low);
                Close = other.Close == decimal.MinValue ? Close : other.Close;
                Volume += other.Volume;
                PseudoFlag = false;
                return this;
            }

            High = Math.Max(other.High, High);
            Low = Math.Min(other.Low, Low);
            Close = other.Close == decimal.MinValue ? Close : other.Close;
            Volume += other.Volume;

            return this;
        }
    }
}
