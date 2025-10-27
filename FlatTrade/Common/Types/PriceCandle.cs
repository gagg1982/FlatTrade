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
            return StartTimeStamp.CompareTo(other.StartTimeStamp); //descending order
        }

        public static bool IsValid(PriceCandle priceCandle)
        {
            return priceCandle.Open != decimal.MinValue &&
                    priceCandle.Close != decimal.MinValue &&
                    priceCandle.High != decimal.MinValue &&
                    priceCandle.Low != decimal.MaxValue &&
                    priceCandle.StartTimeStamp != DateTime.MinValue;
        }

        public bool UpdateCandle(PriceCandle other)
        {
            if (StartTimeStamp != other.StartTimeStamp || !IsValid(other))
                return false;

            if (PseudoFlag)
            {
                Open = Math.Max(Open, other.Open);
                High = Math.Max(other.High, High);
                Low = Math.Min(other.Low, Low);
                Close = other.Close;
                Volume += other.Volume;
                PseudoFlag = false;
                return true;
            }

            var previousHigh = High;
            var previousLow =  Low;
            var previousClose = Close;
            var prevVolume = Volume;

            High = Math.Max(other.High, High);
            Low = Math.Min(other.Low, Low);
            Close = other.Close;
            Volume += other.Volume;

            if(previousClose != Close || previousHigh != High || previousLow != Low || prevVolume != Volume)
                return true;
            return false;
        }
    }
}
