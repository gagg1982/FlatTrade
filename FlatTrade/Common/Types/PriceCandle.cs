namespace FlatTrade.Common.Types
{
    public class PriceCandleDescComparer : IComparer<PriceCandle>
    {
        public int Compare(PriceCandle? x, PriceCandle? y)
        {
            if (x == null || y == null) return 0;
            return y.StartTimeStamp.CompareTo(x.StartTimeStamp);
        }
    }

    public class PriceCandle : IComparable<PriceCandle>
    {
        public DateTime StartTimeStamp { get; set; } = DateTime.MinValue;
        public decimal Open { get; set; } = decimal.MinValue;
        public decimal High { get; set; } = decimal.MinValue;
        public decimal Low { get; set; } = decimal.MaxValue;
        public decimal Close { get; set; } = decimal.MinValue;
        public long Volume { get; set; } = 0;

        public long AccumulatedVolume { get;set; } = 0;

        public bool PseudoFlag = false;
        public int CompareTo(PriceCandle? other)
        {
            if (other is null)
                return 1;
            return other.StartTimeStamp.CompareTo(StartTimeStamp); //descending order
        }

        public PriceCandle Clone()
        {
            return new PriceCandle
            {
                StartTimeStamp = this.StartTimeStamp,
                Open = this.Open,
                High = this.High,
                Low = this.Low,
                Close = this.Close,
                Volume = this.Volume,
                AccumulatedVolume = this.AccumulatedVolume
            };

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

            var previousOpen = Open;
            var previousHigh = High;
            var previousLow =  Low;
            var previousClose = Close;
            var prevVolume = Volume;

            Open = other.Open;
            High = other.High;
            Low = other.Low;
            Close = other.Close;
            Volume = other.Volume;

            if(previousOpen != Open || previousClose != Close || previousHigh != High || previousLow != Low || prevVolume != Volume)
                return true;
            return false;
        }

        public bool ApplyWithQuotes(PriceCandle other)
        {
            if (StartTimeStamp != other.StartTimeStamp || !IsValid(other))
                return false;

            if (PseudoFlag)
            {
                Open = other.Open;
                High = other.High;
                Low = other.Low;
                Close = other.Close;
                AccumulatedVolume = other.AccumulatedVolume;
                Volume = other.Volume;
                PseudoFlag = false;
                return true;
            }

            //var previousOpen = Open;
            var previousHigh = High;
            var previousLow = Low;
            var previousClose = Close;
            var prevVolume = Volume;

            High = Math.Max(other.High, High);
            Low = Math.Min(other.Low, Low);
            Close = other.Close;
            Volume = other.Volume;

            if (previousClose != Close || previousHigh != High || previousLow != Low || Volume != prevVolume)
                return true;
            return false;
        }

        public bool OverWrite(PriceCandle other)
        {
            if (StartTimeStamp != other.StartTimeStamp || !IsValid(other))
                return false;

            var previousOpen = Open;
            var previousHigh = High;
            var previousLow = Low;
            var previousClose = Close;
            var prevVolume = Volume;

            
            Open = other.Open;
            High = other.High;
            Low = other.Low;
            Close = other.Close;
            Volume = other.Volume;
            PseudoFlag = false;
            
            
            if (previousOpen != Open || previousClose != Close || previousHigh != High || previousLow != Low || prevVolume != Volume)
                return true;
            return false;
        }
    }
}
