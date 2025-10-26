IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SantyOnDailyCandles]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_SantyOnDailyCandles]
GO


CREATE OR ALTER PROCEDURE dbo.sp_SantyOnDailyCandles
    @StartDateForOHLCVSanity Date = '01-01-2025',
    @TradingSymbol NVARCHAR(64) = null
AS
BEGIN
    SET NOCOUNT ON;

    DROP TABLE IF EXISTS #TradeDates;
    DECLARE @EndDate DATE = CAST(GETDATE() AS DATE);

    ;WITH N AS (
            SELECT TOP (DATEDIFF(DAY, @StartDateForOHLCVSanity, @EndDate) + 1)
            ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) - 1 AS n
            FROM sys.objects o1
            CROSS JOIN sys.objects o2
    )
    SELECT DISTINCT
           hc.Exchange,
           DATEADD(DAY, n, @StartDateForOHLCVSanity) AS TradingDate
    INTO #TradeDates
    FROM N
    CROSS JOIN (SELECT DISTINCT Exchange FROM HolidayCalendar) hc
    WHERE DATENAME(WEEKDAY, DATEADD(DAY, n, @StartDateForOHLCVSanity)) NOT IN ('Saturday','Sunday')
      AND NOT EXISTS (
            SELECT 1
            FROM HolidayCalendar h
            WHERE h.Exchange = hc.Exchange
              AND h.HolidayDate = DATEADD(DAY, n, @StartDateForOHLCVSanity)
      );

    CREATE INDEX IX_#TradeDates_ExchangeDate ON #TradeDates(Exchange, TradingDate);


    SELECT distinct TradingSymbol, ExchangeCode, Token, ListingDate, TradeStartDate, TradeEndDate, SanityTradeStartDate, RequiredTradeCandles, ActualTradeCandles, RequiredTradeCandles - ActualTradeCandles As MissingTradeCandles
    FROM
    (
        Select i.Token, I.ListingDate, i.TradingSymbol, i.ExchangeCode, s.*,
            CASE WHEN ListingDate >= @StartDateForOHLCVSanity THEN ListingDate ELSE @StartDateForOHLCVSanity END as SanityTradeStartDate,
            ( SELECT COUNT(*) 
                FROM #TradeDates t
                WHERE t.Exchange = i.ExchangeCode 
                                AND t.TradingDate >= CASE WHEN ListingDate >= @StartDateForOHLCVSanity THEN ListingDate ELSE @StartDateForOHLCVSanity END
                                AND t.TradingDate <= cast(GetDate() as DATE)
            ) RequiredTradeCandles

        from StockInstruments i
        LEFT JOIN 
        (
            SELECT a.*, s.StartDate as TradeStartDate, s.EndDate as TradeEndDate, s.ActualTradeCandles FROM 
            (
                SELECT InstrumentId,
                Min(cast(StartDateTime as date)) As StartDate , 
                Max(cast(StartDateTime as date)) as EndDate,
                Count(*) As ActualTradeCandles
                FROM   StocksOhlcv_1440 s
                GROUP BY InstrumentId
            )s
            JOIN StocksOhlcv_1440 a On a.InstrumentId =s.InstrumentId
        ) s On s.InstrumentId = i.id
        WHERE i.Active=1 AND (@TradingSymbol is null OR @TradingSymbol = i.TradingSymbol) AND i.Token NOT IN
            (select Token from ExcludedStockInstruments a where i.TradingSymbol = a.TradingSymbol and i.ExchangeCode = a.Exchange and i.Token = a.token)
    ) a WHERE  RequiredTradeCandles - ActualTradeCandles > 0
    order by TradeEndDate desc, MissingTradeCandles desc
    
END

GO
