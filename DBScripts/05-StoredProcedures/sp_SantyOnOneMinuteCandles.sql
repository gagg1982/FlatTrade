CREATE OR ALTER PROCEDURE dbo.sp_SantyOnOneMinuteCandles
    @StartDateForOHLCVSanity DATE = '2025-01-01',
    @TradingSymbol NVARCHAR(64) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    --SET TRANSACTION ISOLATION LEVEL SNAPSHOT;
    -----------------------------------------------------------------------
    -- Variables
    -----------------------------------------------------------------------
    DECLARE @EndDate DATE = CAST(GETDATE() AS DATE);

    -----------------------------------------------------------------------
    -- 1. Build TradeDates (fast)
    -----------------------------------------------------------------------
    DROP TABLE IF EXISTS #TradeDates;
    ;WITH
    -- small tally: generate required day offsets
    N AS (
        SELECT TOP (DATEDIFF(DAY, @StartDateForOHLCVSanity, @EndDate) + 1)
            ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) - 1 AS n
        FROM sys.all_objects a1
        CROSS JOIN sys.all_objects a2
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

    -----------------------------------------------------------------------
    -- 2. Pick instruments to evaluate early (apply Active, TradingSymbol and Exclusions)
    -----------------------------------------------------------------------
    DROP TABLE IF EXISTS #Instruments;
    SELECT
        i.Id,
        i.TradingSymbol,
        i.ExchangeCode,
        i.Token,
        i.ListingDate
    INTO #Instruments
    FROM dbo.StockInstruments i
    WHERE i.Active = 1
      AND (@TradingSymbol IS NULL OR @TradingSymbol = i.TradingSymbol)
      AND NOT EXISTS (
            SELECT 1
            FROM ExcludedStockInstruments e
            WHERE e.Token = i.Token
              AND e.TradingSymbol = i.TradingSymbol
              AND e.Exchange = i.ExchangeCode
      );

    CREATE INDEX IX_#Instruments_Id ON #Instruments(Id);
    CREATE INDEX IX_#Instruments_TradingSymbolExchange ON #Instruments(TradingSymbol, ExchangeCode);

    -----------------------------------------------------------------------
    -- 3. Pre-aggregate OHLCV only for those instruments and date range
    -----------------------------------------------------------------------
    DROP TABLE IF EXISTS #InstrumentDailyCandles;
    SELECT
        s.InstrumentId,
        s.TradeDate,
        COUNT_BIG(*) AS ActualTradeCandlesPerDay
    INTO #InstrumentDailyCandles
    FROM dbo.StocksOhlcv_1 s WITH (NOLOCK)
    WHERE s.TradeDate >= @StartDateForOHLCVSanity
      AND s.TradeDate <= @EndDate
      AND s.InstrumentId IN (SELECT Id FROM #Instruments)  -- << important filter
    GROUP BY s.InstrumentId, s.TradeDate;

    CREATE INDEX IX_#InstrumentDailyCandles_IdTradeDate ON #InstrumentDailyCandles(InstrumentId, TradeDate);

    -----------------------------------------------------------------------
    -- 4. Combine with instruments and count missing days
    -----------------------------------------------------------------------
    SELECT DISTINCT
        i.TradingSymbol,
        i.ExchangeCode,
        i.Token,
        i.ListingDate,
        CASE WHEN i.ListingDate >= @StartDateForOHLCVSanity THEN i.ListingDate ELSE @StartDateForOHLCVSanity END AS SanityTradeStartDate,
        MAX(c.TradeDate) AS TradeEndDate,
        COUNT(t.TradingDate) AS RequiredTradeCandles,
        SUM(ISNULL(c.ActualTradeCandlesPerDay,0)) AS TotalTradeCandles,
        COUNT(c.TradeDate) AS ActualTradeCandles,
        COUNT(t.TradingDate) - COUNT(c.TradeDate) AS MissingTradeCandles
    FROM #Instruments i
    INNER JOIN #TradeDates t
        ON t.Exchange = i.ExchangeCode
       AND t.TradingDate >= CASE WHEN i.ListingDate >= @StartDateForOHLCVSanity THEN i.ListingDate ELSE @StartDateForOHLCVSanity END
       AND t.TradingDate <= @EndDate
    LEFT JOIN #InstrumentDailyCandles c
        ON c.InstrumentId = i.Id
       AND c.TradeDate = t.TradingDate
    GROUP BY
        i.TradingSymbol, i.ExchangeCode, i.Token, i.ListingDate
    HAVING COUNT(t.TradingDate) - COUNT(c.TradeDate) > 0
    ORDER BY TradeEndDate DESC, MissingTradeCandles DESC
    OPTION (RECOMPILE); -- optional, helps parameter sniffing issues

END;
GO
