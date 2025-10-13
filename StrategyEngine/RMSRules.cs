namespace StrategyEngine
{
    public class RMSRules
    {
        public decimal MaxAmountAllocatedPerDay { get; set; }
        public decimal MaxAmountAllocatedPerTrade { get; set; }
        public int MaxOpenPositionsPerScrip { get; set; }

        public double MaxPercentageLossPerTrade { get; set; }
        public double MaxPercentageLossPerDay { get; set; }

    }
}
