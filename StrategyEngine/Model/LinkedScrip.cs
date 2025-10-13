using FlatTrade.Common.Types.Base;

namespace StrategyEngine.Model
{
    public class LinkedScrip
    {
        public string TradingSymbol { get; set; } = string.Empty;
        public Exchange Exchange { get; set; }
        public long Token { get; set; }
        public bool IsFutureAllowed { get; set; }
        public bool IsOptionAllowed { get; set; }
    }
}
