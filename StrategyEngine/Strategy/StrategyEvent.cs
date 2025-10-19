using FlatTrade.Common.Types.Base;
using StrategyEngine.Model;

namespace StrategyEngine.Strategy
{
    internal class StrategyEvent
    {
        internal required StrategyEngineEventType EventType { get; set; }
        internal required string TradingSymbol { get; set; } = string.Empty;
        internal required Exchange Exchange {  get; set; }
        internal required long Token { get; set; }
    }
}
