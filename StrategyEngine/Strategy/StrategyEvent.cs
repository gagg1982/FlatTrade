using FlatTrade.Common.Types.Base;
using StrategyEngine.Model;

namespace StrategyEngine.Strategy
{
    internal class StrategyEvent
    {
        internal StrategyEngineEventType EventType { get; set; }
        internal string? TradingSymbol { get; set; } = string.Empty;
        internal Exchange? Exchange {  get; set; }
        internal long? Token { get; set; }
    }
}
