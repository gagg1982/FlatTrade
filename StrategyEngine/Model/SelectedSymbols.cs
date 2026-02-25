using FlatTrade.Types.Base;

namespace StrategyEngine.Model
{
    public class SelectedSymbol
    {
        public string TradingSymbol { get; set; } = string.Empty;
        public Exchange Exchange { get; set; }
        public long Token { get; set; }
    }
}
