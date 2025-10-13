namespace FlatTrade.MarketInfoManager
{
    public class MarginEquityResponse
    {
        public string Symbol { get; set; } = string.Empty;
        public string Segment { get; set; } = string.Empty;
        public decimal MISMarginInPercentage { get; set; }
        public decimal CNCMarginMISMarginInPercentage { get; set; }
        public decimal MISLeverageX { get; set; }
        public decimal CNCLeverageX { get; set; }
    }
}
