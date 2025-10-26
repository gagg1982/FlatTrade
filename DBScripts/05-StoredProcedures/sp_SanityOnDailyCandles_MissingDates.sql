IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SanityOnDailyCandles_MissingDates]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_SanityOnDailyCandles_MissingDates]
GO


CREATE OR ALTER PROCEDURE dbo.sp_SanityOnDailyCandles_MissingDates
    @StartDateForOHLCVSanity DATE = '2025-01-01',
    @TradingSymbol NVARCHAR(64) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @EndDate DATE = CAST(GETDATE() AS DATE);
    -----------------------------------------------------------------------
    -- 1. Build TradeDates (valid exchange-specific calendar)
    -----------------------------------------------------------------------
    DROP TABLE IF EXISTS #TradeDates;
    ;WITH N AS (
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
    -- 2. Filter instruments (active, non-excluded)
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
      AND (@TradingSymbol IS NULL OR i.TradingSymbol = @TradingSymbol)
      AND NOT EXISTS (
            SELECT 1
            FROM ExcludedStockInstruments e
            WHERE e.Token = i.Token
              AND e.TradingSymbol = i.TradingSymbol
              AND e.Exchange = i.ExchangeCode
      );

    CREATE INDEX IX_#Instruments_Id ON #Instruments(Id);
    -----------------------------------------------------------------------
    -- 3. Get actual traded dates for those instruments
    -----------------------------------------------------------------------
    DROP TABLE IF EXISTS #ActualDates;
    SELECT DISTINCT
        s.InstrumentId,
        s.StartDateTime as TradeDate
    INTO #ActualDates
    FROM dbo.StocksOhlcv_1440 s WITH (NOLOCK)
    WHERE s.StartDateTime >= @StartDateForOHLCVSanity
      AND s.StartDateTime <= @EndDate
      AND s.InstrumentId IN (SELECT Id FROM #Instruments);

    CREATE INDEX IX_#ActualDates_IdDate ON #ActualDates(InstrumentId, TradeDate);
    -----------------------------------------------------------------------
    -- 4. Return missing dates
    -----------------------------------------------------------------------
    SELECT
        i.TradingSymbol,
        i.ExchangeCode,
        i.Token,
        t.TradingDate AS MissingTradeDate
    FROM #Instruments i
    INNER JOIN #TradeDates t
        ON t.Exchange = i.ExchangeCode
       AND t.TradingDate >= CASE WHEN i.ListingDate >= @StartDateForOHLCVSanity
                                 THEN i.ListingDate ELSE @StartDateForOHLCVSanity END
       AND t.TradingDate <= @EndDate
    LEFT JOIN #ActualDates a
        ON a.InstrumentId = i.Id
       AND a.TradeDate = t.TradingDate
    WHERE a.TradeDate IS NULL  -- Missing only
    ORDER BY i.ExchangeCode, i.TradingSymbol, t.TradingDate
    OPTION (RECOMPILE);
END;
GO
