using FlatTrade.Common.Types.Base;
using FlatTrade.ScripManager;

namespace StrategyEngine.Model
{
    public class ScripInfo
    {
        public Exchange Exchange { get; set; }

        public string TradingSymbol { get; set; } = string.Empty;

        public string CompanyName { get; set; } = string.Empty;

        public string SymbolName { get; set; } = string.Empty;

        public string Segment { get; set; } = string.Empty;
        
        public string Isin { get; set; } = string.Empty;

        public int PricePrecision { get; set; }

        public decimal LotSize { get; set; }

        public decimal TickSize { get; set; }

        public DateTime LastUpdateTime { get; set; }

        public decimal UpperCircuitLimit { get; set; }

        public decimal LowerCircuitLimit { get; set; }

        public long Token { get; set; }
      
        public decimal IssueCapital { get; set; }

        public bool IsIntraDayAllowed { get; set; }

        public ScripInfo() { }

        protected ScripInfo(ScripInfo other)
        {
            Exchange = other.Exchange;
            TradingSymbol = other.TradingSymbol;
            CompanyName = other.CompanyName;
            SymbolName = other.SymbolName;
            Segment = other.Segment;
            Isin = other.Isin;
            PricePrecision = other.PricePrecision;
            LotSize = other.LotSize;
            TickSize = other.TickSize;
            LastUpdateTime = other.LastUpdateTime;
            UpperCircuitLimit = other.UpperCircuitLimit;    
            LowerCircuitLimit = other.LowerCircuitLimit;
            Token = other.Token;
            IssueCapital = other.IssueCapital;
            IsIntraDayAllowed = other.IsIntraDayAllowed;
        }

        public static IEnumerable<ScripInfo> ConvertFrom(IEnumerable<QuotesResponse> quotesResponse, 
            IEnumerable<LinkedScrip> linkedScrips) =>        
                from a in quotesResponse
                             join b in linkedScrips on new { a.Token, a.Exchange } equals new { b.Token, b.Exchange }
                             select
                             (
                             new ScripInfo
                             {
                                 Exchange = a.Exchange,
                                 TradingSymbol = a.TradingSymbol,
                                 CompanyName = a.CompanyName,

                                 SymbolName = a.SymbolName,
                                 Segment = a.Segment,
                                 Isin = a.Isin,
                                 PricePrecision = a.PricePrecision,
                                 LotSize = a.LotSize,
                                 TickSize = a.TickSize,

                                 LastUpdateTime = a.LastUpdateTime,
                                 LowerCircuitLimit = a.LowerCircuitLimit,
                                 UpperCircuitLimit = a.UpperCircuitLimit,
                                 Token = a.Token,
                                 IssueCapital = a.IssueCapital,
                                 IsIntraDayAllowed = b.IsFutureAllowed && b.IsOptionAllowed
                             });        
    }
}
