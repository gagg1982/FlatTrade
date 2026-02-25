using FlatTrade.Types.Base;

namespace StrategyEngine.Model
{
    public class DecisionMakingInputs
    {
        public required string DecisionMakingRemarks;
    }

    public class ModifyOrder : CreateOrder
    {
        public required long NorenOrderNumber { get; set; } = 0;
        public required decimal RemainingOriginalLimitPrice { get; set; } = 0;
        public required long RemainingOriginalQuantity { get; set; } = 0;
    }

    public class CancelOrder
    {
        public required long NorenOrderNumber { get; set; }       
    }

    public class CreateOrder
    {
        public readonly Guid InternalOrderId = Guid.NewGuid();
        public required long Token { get; set; }
        public required string TradingSymbol { get; set; }
        public required Exchange Exchange { get; set; }
        public RetentionType RetentionType { get; set; } = RetentionType.DAY;
        public PriceType PriceType { get; set; } = PriceType.Limit;
        public ProductType ProductType { get; set; } = ProductType.IntraDay;
        public required TransactionType TransactionType { get; set; }
        public decimal MarketProtectionInPercent { get; set; } = 0.0m;

        public required decimal LimitPrice { get; set; }
        public decimal TriggerPrice { get; set; } = 0.0m;
        public required decimal DifferentialSLPrice { get; set; }
        public decimal DifferentialTrailingTicks { get; set; }
        public required decimal DifferentialProfitPrice { get; set; }
        public required int Quantity { get; set; }
    }

    public abstract record OutputDecision
    {
        public abstract OrderEventType OrderEventType { get; }

        public sealed record Create(CreateOrder Order) : OutputDecision
        {
            public override OrderEventType OrderEventType => OrderEventType.CreateOrder;
        }

        public sealed record Modify(ModifyOrder Order) : OutputDecision
        {
            public override OrderEventType OrderEventType => OrderEventType.ModifyOrder;
        }

        public sealed record Cancel(CancelOrder Order) : OutputDecision
        {
            public override OrderEventType OrderEventType => OrderEventType.CancelOrder;
        }
    }

    public record StrategySignal(Guid StrategySignalId, string StrategyName, OutputDecision OutputDecision, DecisionMakingInputs DecisionMakingInputs);
}
