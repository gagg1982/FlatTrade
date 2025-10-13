IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SantyOnDailyCandles]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_SantyOnDailyCandles]
GO


CREATE OR ALTER PROCEDURE dbo.sp_SantyOnDailyCandles
    @StartDateForOHLCVSanity Date = '01-01-2025'
AS
BEGIN

    DROP TABLE IF EXISTS #TradeDates
    --Got All working Date Starting from 01-01-2025 till current date
    ;WITH DateRange AS
    (
        -- Find min and max range from your TradeCounts table
            SELECT DATEADD(day, 1000, @StartDateForOHLCVSanity) as EndDate, @StartDateForOHLCVSanity AS StartDate
    ),
    AllDates AS
    (
        -- Generate continuous date series
        SELECT StartDate AS TheDate
        FROM DateRange
        UNION ALL
        SELECT DATEADD(DAY, 1, TheDate)
        FROM AllDates a
        JOIN DateRange r ON a.TheDate < r.EndDate
    ), 
    WeekDates As
    (
        SELECT * FROM
        (
        SELECT a.TheDate 
        FROM AllDates a
        WHERE  DATENAME(WEEKDAY, a.TheDate) NOT IN ('Saturday','Sunday')  -- Not weekend
        ) a,
        (
        SELECT DISTINCT Exchange FROM HolidayCalendar
        )b
    )
    SELECT w.Exchange, w.TheDate As TradingDates INTO  #TradeDates --, h.HolidayDate
    FROM WeekDates w
    LEFT JOIN HolidayCalendar h ON h.HolidayDate = w.TheDate AND w.Exchange = h.Exchange
    WHERE h.Holidaydate is NULL
    order by Exchange, thedate asc
    OPTION (MAXRECURSION 0);


    SELECT distinct TradingSymbol, ExchangeCode, Token, ListingDate, SanityTradeStartDate, TradeEndDate, RequiredTradeCandles - ActualTradeCandles As MissingTradeCandles
    FROM
    (
        Select i.Token, I.ListingDate, i.TradingSymbol, i.ExchangeCode, s.*,
            CASE WHEN ListingDate > @StartDateForOHLCVSanity THEN ListingDate ELSE @StartDateForOHLCVSanity END as SanityTradeStartDate,
            ( SELECT COUNT(*) 
            FROM #TradeDates t
            WHERE t.Exchange = i.ExchangeCode 
                                AND t.TradingDates >= CASE WHEN ListingDate > @StartDateForOHLCVSanity THEN ListingDate ELSE @StartDateForOHLCVSanity END
                                AND t.TradingDates <= cast(GetDate() as DATE)) RequiredTradeCandles

        from StockInstruments i
        LEFt JOIN 
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
        WHERE i.Active=1
    ) a WHERE  RequiredTradeCandles - ActualTradeCandles > 0
    order by TradeEndDate desc
    
END

GO

