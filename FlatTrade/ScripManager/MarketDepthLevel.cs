namespace FlatTrade.ScripManager
{
    public class MarketDepthLevel
    {
        public decimal Price { get; set; } = decimal.MinValue;
        public long Quantity { get; set; } = 0;
        public int Orders { get; set; } = 0;
    }

}
