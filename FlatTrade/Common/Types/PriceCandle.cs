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

        public bool ApplyQuotes(DateTime startDateTime, decimal price, long volume, bool pseudoPrice)
        {
            if (startDateTime != StartTimeStamp)
                return false;

            //If input price is pseudo, no need to update, exisiting candle has already previous price through shifting
            //OR
            //1st price received for this time has the price from previous candle.
            if(pseudoPrice)
            {
                var prevVolume = Volume;
                Volume = volume - AccumulatedVolume;
                if (prevVolume != Volume)
                    return true;
                return false;
            }

            // The below will hit only when actual price of the interval comes.
            if (PseudoFlag)
            {             
                Open = price;
                High = price;
                Low = price;
                Close = price;
                Volume = volume - AccumulatedVolume;

                PseudoFlag = false;                
                return true;
            }

            //var previousOpen = Open;
            var previousHigh = High;
            var previousLow = Low;
            var previousClose = Close;
            var previousVolume = Volume;

            High = Math.Max(price, High);
            Low = Math.Min(price, Low);
            Close = price;
            Volume = volume - AccumulatedVolume;

            if (previousClose != Close || previousHigh != High || previousLow != Low || previousVolume != Volume)
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
            
            
            if (previousOpen != Open || previousClose != Close || previousHigh != High || previousLow != Low)
                return true;
            return false;
        }
    }
}
