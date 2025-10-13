using FlatTrade.Common.Types.Base;
using FlatTrade.TradeManager;
using StrategyEngine.Model;

namespace StrategyEngine.Strategy
{
    internal class DecisionMakingInputs
    {
        TradeBookRequest? TradeBookRequest { get; set; }
        PositionBookRequest? PositionBookRequest { get; set; }
        ScripInfo? ScripInfo { get; set; }
        OrderInfo? OrderInfo { get; set; }
    }

    internal class OutputDecision
    {
        public required RetentionType RetentionType { get; set; }
        public required PriceType PriceType { get; set; }
        public required ProductType ProductType { get; set; }
        public required TransactionType TransactionType { get; set; }

        public required decimal Price { get; set; }
        public required decimal SLPrice { get; set; }
        public required decimal ProfitPrice { get; set; }
        public required long Quantity { get; set; }

    }

    internal class StrategySignal
    {
        public required OutputDecision OutputDecision { get; set; }
        public required DecisionMakingInputs DecisionMakingInputs { get; set; }

    }
}
