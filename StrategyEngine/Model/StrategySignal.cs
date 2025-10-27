using FlatTrade.Common.Types.Base;
using FlatTrade.TradeManager;

namespace StrategyEngine.Model
{
    public class DecisionMakingInputs
    {
        TradeBookRequest? TradeBookRequest { get; set; }
        PositionBookRequest? PositionBookRequest { get; set; }
        ScripInfo? ScripInfo { get; set; }
        OrderInfo? OrderInfo { get; set; }
    }
    public class ModifyOrder
    {
        public required long Token { get; set; }
        public required string TradingSymbol { get; set; }
        public required Exchange Exchange { get; set; }
        public required RetentionType RetentionType { get; set; }
        public required PriceType PriceType { get; set; }
        public required ProductType ProductType { get; set; }
        public required TransactionType TransactionType { get; set; }

        public required decimal Price { get; set; }
        public required decimal SLPrice { get; set; }
        public required decimal ProfitPrice { get; set; }
        public required long Quantity { get; set; }
    }

    public class CancelOrder
    {
        public required long NorenOrderNumber { get; set; }       
    }

    public class CreateOrder
    {
        public required long Token { get; set; }
        public required string TradingSymbol { get; set; }
        public required Exchange Exchange { get; set; }
        public required RetentionType RetentionType { get; set; }
        public required PriceType PriceType { get; set; }
        public required ProductType ProductType { get; set; }
        public required TransactionType TransactionType { get; set; }
        public required decimal MarketProtectionInPercent { get; set; }

        public required decimal LimitPrice { get; set; }
        public required decimal BoTriggerPrice { get; set; }
        public required decimal DifferentialSLPrice { get; set; }
        public required int DifferentialTrailingTicks { get; set; }
        public required decimal DifferentialProfitPrice { get; set; }
        public required int Quantity { get; set; }
    }

    public class OutputDecision
    {
        public required OrderEventType OrderEventType { get; set; }
        public CreateOrder? CreateOrder { get; set; } = null;
        public ModifyOrder? ModifyOrder { get; set; } = null;
        public CancelOrder? CancelOrder { get; set; } = null;
    }

    public class StrategySignal
    {
        public required OutputDecision OutputDecision { get; set; }
        public required DecisionMakingInputs DecisionMakingInputs { get; set; }

    }
}
