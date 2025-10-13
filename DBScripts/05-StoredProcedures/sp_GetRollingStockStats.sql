IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_GetRollingStockStats]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_GetRollingStockStats]
GO

CREATE PROCEDURE sp_GetRollingStockStats
    @Top INT,
    @Days INT,
    @TableName NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

        DECLARE @SQL NVARCHAR(MAX);
        DECLARE @SQLLag NVARCHAR(MAX);
        DECLARE @SQLMedianMedian1 NVARCHAR(MAX)
        DECLARE @SQLMedianMedian2 NVARCHAR(MAX);
        DECLARE @Query NVARCHAR(MAX);

        DECLARE @PrevXDaysMaxHigh NVARCHAR(100);
        DECLARE @PrevXDaysMinHigh NVARCHAR(100);
        DECLARE @PrevXDaysMeanHigh NVARCHAR(100);
        DECLARE @PrevXDaysMedianHigh NVARCHAR(100);

        DECLARE @PrevXDaysMaxLow NVARCHAR(100);
        DECLARE @PrevXDaysMinLow NVARCHAR(100);
        DECLARE @PrevXDaysMeanLow NVARCHAR(100);
        DECLARE @PrevXDaysMedianLow NVARCHAR(100);

        DECLARE @PrevXDaysMaxOpen NVARCHAR(100);
        DECLARE @PrevXDaysMinOpen NVARCHAR(100);
        DECLARE @PrevXDaysMeanOpen NVARCHAR(100);
        DECLARE @PrevXDaysMedianOpen NVARCHAR(100);

        DECLARE @PrevXDaysMaxClose NVARCHAR(100);
        DECLARE @PrevXDaysMinClose NVARCHAR(100);
        DECLARE @PrevXDaysMeanClose NVARCHAR(100);
        DECLARE @PrevXDaysMedianClose NVARCHAR(100);

        DECLARE @PrevXDaysMaxVolume NVARCHAR(100);
        DECLARE @PrevXDaysMinVolume NVARCHAR(100);
        DECLARE @PrevXDaysMeanVolume NVARCHAR(100);
        DECLARE @PrevXDaysMedianVolume NVARCHAR(100);

        DECLARE @PrevXDate NVARCHAR(100);
        DECLARE @PrevXDayOpen NVARCHAR(100);
        DECLARE @PrevXDayClose NVARCHAR(100);
        DECLARE @PrevXDayHigh NVARCHAR(100);
        DECLARE @PrevXDayLow NVARCHAR(100);
        DECLARE @PrevXDayVolume NVARCHAR(100);

        DECLARE @PrevXDayOpenGap NVARCHAR(100);
        DECLARE @PrevXDayCloseGap NVARCHAR(100);
        DECLARE @PrevXDayLowGap NVARCHAR(100);
        DECLARE @PrevXDayHighGap NVARCHAR(100);
        DECLARE @PrevXDayVolumeGap NVARCHAR(100);

        DECLARE @PrevXDayOpenCloseGap NVARCHAR(100);
        DECLARE @PrevXDayOpenCloseGapPercent NVARCHAR(100);

        SET @PrevXDate =  QUOTENAME(CONCAT( 'Prev',@Days,'Date'))
        SET @PrevXDayOpen =  QUOTENAME(CONCAT( 'Prev',@Days,'DayOpen'))
        SET @PrevXDayClose =  QUOTENAME(CONCAT( 'Prev',@Days,'DayClose'))
        SET @PrevXDayLow =  QUOTENAME(CONCAT( 'Prev',@Days,'DayLow'))
        SET @PrevXDayHigh =  QUOTENAME(CONCAT( 'Prev',@Days,'DayHigh'))
        SET @PrevXDayVolume =  QUOTENAME(CONCAT( 'Prev',@Days,'DayVolume'))

        SET @PrevXDayOpenGap =  QUOTENAME(CONCAT( 'Prev',@Days,'DayOpenGap'))
        SET @PrevXDayCloseGap =  QUOTENAME(CONCAT( 'Prev',@Days,'DayCloseGap'))
        SET @PrevXDayLowGap =  QUOTENAME(CONCAT( 'Prev',@Days,'DayLowGap'))
        SET @PrevXDayHighGap =  QUOTENAME(CONCAT( 'Prev',@Days,'DayHighGap'))
        SET @PrevXDayVolumeGap =  QUOTENAME(CONCAT( 'Prev',@Days,'DayVolumeGap'))

        SET @PrevXDayOpenCloseGap =  QUOTENAME(CONCAT( 'Prev',@Days,'DayOpenCloseGap'))
        SET @PrevXDayOpenCloseGapPercent =  QUOTENAME(CONCAT( 'Prev',@Days,'DayOpenCloseGapPercent'))

        SET @PrevXDaysMaxVolume =  QUOTENAME(CONCAT( 'Prev',@Days,'DaysMaxVolume'))
        SET @PrevXDaysMinVolume =  QUOTENAME(CONCAT( 'Prev',@Days,'DaysMinVolume'))
        SET @PrevXDaysMeanVolume =  QUOTENAME(CONCAT( 'Prev',@Days,'DaysMeanVolume'))
        SET @PrevXDaysMedianVolume =  QUOTENAME(CONCAT( 'Prev',@Days,'DaysMedianVolume'))

        SET @PrevXDaysMaxOpen =  QUOTENAME(CONCAT( 'Prev',@Days,'DaysMaxOpen'))
        SET @PrevXDaysMinOpen =  QUOTENAME(CONCAT( 'Prev',@Days,'DaysMinOpen'))
        SET @PrevXDaysMeanOpen =  QUOTENAME(CONCAT( 'Prev',@Days,'DaysMeanOpen'))
        SET @PrevXDaysMedianOpen =  QUOTENAME(CONCAT( 'Prev',@Days,'DaysMedianOpen'))


        SET @PrevXDaysMaxClose =  QUOTENAME(CONCAT( 'Prev',@Days,'DaysMaxClose'))
        SET @PrevXDaysMinClose =  QUOTENAME(CONCAT( 'Prev',@Days,'DaysMinClose'))
        SET @PrevXDaysMeanClose =  QUOTENAME(CONCAT( 'Prev',@Days,'DaysMeanClose'))
        SET @PrevXDaysMedianClose =  QUOTENAME(CONCAT( 'Prev',@Days,'DaysMedianClose'))


        SET @PrevXDaysMaxLow =  QUOTENAME(CONCAT( 'Prev',@Days,'DaysMaxLow'))
        SET @PrevXDaysMinLow =  QUOTENAME(CONCAT( 'Prev',@Days,'DaysMinLow'))
        SET @PrevXDaysMeanLow =  QUOTENAME(CONCAT( 'Prev',@Days,'DaysMeanLow'))
        SET @PrevXDaysMedianLow =  QUOTENAME(CONCAT( 'Prev',@Days,'DaysMedianLow'))


        SET @PrevXDaysMaxHigh =  QUOTENAME(CONCAT( 'Prev',@Days,'DaysMaxHigh'))
        SET @PrevXDaysMinHigh =  QUOTENAME(CONCAT( 'Prev',@Days,'DaysMinHigh'))
        SET @PrevXDaysMeanHigh =  QUOTENAME(CONCAT( 'Prev',@Days,'DaysMeanHigh'))
        SET @PrevXDaysMedianHigh =  QUOTENAME(CONCAT( 'Prev',@Days,'DaysMedianHigh'))

        -- Construct the dynamic SQL
        SET @SQL = N'
        ;WITH RankedPrices AS (
            SELECT *,
                ROW_NUMBER() OVER (PARTITION BY InstrumentId ORDER BY StartDateTime DESC) AS Ranking
            FROM stocksohlcv_1440
        ),
        RollingData AS (
            SELECT 
                InstrumentId,
                StartDateTime,

                COUNT(StartDateTime) OVER (PARTITION BY InstrumentId) AS DailyCandleCount,
                -- Calculate X-day rolling statistics
                MAX([High]) OVER (PARTITION BY InstrumentId ORDER BY StartDateTime ROWS  '+cast (@Days-1 as Nvarchar(10))+ N' PRECEDING) AS ' + @PrevXDaysMaxHigh + N',
                MIN([High]) OVER (PARTITION BY InstrumentId ORDER BY StartDateTime ROWS  '+cast (@Days-1 as Nvarchar(10))+ N' PRECEDING) AS ' + @PrevXDaysMinHigh + N',
                AVG([High]) OVER (PARTITION BY InstrumentId ORDER BY StartDateTime ROWS  '+cast (@Days-1 as Nvarchar(10))+ N' PRECEDING) AS ' + @PrevXDaysMeanHigh + N',

                MAX([Low]) OVER (PARTITION BY InstrumentId ORDER BY StartDateTime ROWS  '+cast (@Days-1 as Nvarchar(10))+ N' PRECEDING) AS ' + @PrevXDaysMaxLow + N',
                MIN([Low]) OVER (PARTITION BY InstrumentId ORDER BY StartDateTime ROWS  '+cast (@Days-1 as Nvarchar(10))+ N' PRECEDING) AS ' + @PrevXDaysMinLow + N',
                AVG([Low]) OVER (PARTITION BY InstrumentId ORDER BY StartDateTime ROWS  '+cast (@Days-1 as Nvarchar(10))+ N' PRECEDING) AS ' + @PrevXDaysMeanLow + N',

                MAX([Open]) OVER (PARTITION BY InstrumentId ORDER BY StartDateTime ROWS  '+cast (@Days-1 as Nvarchar(10))+ N' PRECEDING) AS ' + @PrevXDaysMaxOpen + N',
                MIN([Open]) OVER (PARTITION BY InstrumentId ORDER BY StartDateTime ROWS  '+cast (@Days-1 as Nvarchar(10))+ N' PRECEDING) AS ' + @PrevXDaysMinOpen + N',
                AVG([Open]) OVER (PARTITION BY InstrumentId ORDER BY StartDateTime ROWS  '+cast (@Days-1 as Nvarchar(10))+ N' PRECEDING) AS ' + @PrevXDaysMeanOpen + N',

                MAX([Close]) OVER (PARTITION BY InstrumentId ORDER BY StartDateTime ROWS  '+cast (@Days-1 as Nvarchar(10))+ N' PRECEDING) AS ' + @PrevXDaysMaxClose + N',
                MIN([Close]) OVER (PARTITION BY InstrumentId ORDER BY StartDateTime ROWS  '+cast (@Days-1 as Nvarchar(10))+ N' PRECEDING) AS ' + @PrevXDaysMinClose + N',
                AVG([Close]) OVER (PARTITION BY InstrumentId ORDER BY StartDateTime ROWS  '+cast (@Days-1 as Nvarchar(10))+ N' PRECEDING) AS ' + @PrevXDaysMeanClose + N',

                MAX([Volume]) OVER (PARTITION BY InstrumentId ORDER BY StartDateTime ROWS  '+cast (@Days-1 as Nvarchar(10))+ N' PRECEDING) AS ' + @PrevXDaysMaxVolume + N',
                MIN([Volume]) OVER (PARTITION BY InstrumentId ORDER BY StartDateTime ROWS  '+cast (@Days-1 as Nvarchar(10))+ N' PRECEDING) AS ' + @PrevXDaysMinVolume + N',
                AVG([Volume]) OVER (PARTITION BY InstrumentId ORDER BY StartDateTime ROWS  '+cast (@Days-1 as Nvarchar(10))+ N' PRECEDING) AS ' + @PrevXDaysMeanVolume + N'
            FROM stocksohlcv_1440
        ), Final AS (
        SELECT r.Ranking, h.*,'

        SET @SQLLag = N'
            LAG(r.[Open], '+cast (@Days-1 as Nvarchar(10))+ N') OVER (PARTITION BY r.InstrumentId ORDER BY r.StartDateTime ASC) AS ' + @PrevXDayOpen + N',
            LAG(r.[High], '+cast (@Days-1 as Nvarchar(10))+ N') OVER (PARTITION BY r.InstrumentId ORDER BY r.StartDateTime ASC) AS ' + @PrevXDayHigh + N',
            LAG(r.[Low], '+cast (@Days-1 as Nvarchar(10))+ N') OVER (PARTITION BY r.InstrumentId ORDER BY r.StartDateTime ASC) AS ' + @PrevXDayLow + N',
            LAG(r.[Close], '+cast (@Days-1 as Nvarchar(10))+ N') OVER (PARTITION BY r.InstrumentId ORDER BY r.StartDateTime ASC) AS ' + @PrevXDayClose + N',
            LAG(r.[StartDateTime], '+cast (@Days-1 as Nvarchar(10))+ N') OVER (PARTITION BY r.InstrumentId ORDER BY r.StartDateTime ASC) AS ' + @PrevXDate + N',
            LAG(r.Volume, '+cast (@Days-1 as Nvarchar(10))+ N') OVER (PARTITION BY r.InstrumentId ORDER BY r.StartDateTime ASC) AS ' + @PrevXDayVolume + N',

            (r.[Open] - LAG(r.[Open], '+cast (@Days-1 as Nvarchar(10))+ N') OVER (PARTITION BY r.InstrumentId ORDER BY r.StartDateTime ASC)) AS ' + @PrevXDayOpenGap + N',
            (r.[High] - LAG(r.[High], '+cast (@Days-1 as Nvarchar(10))+ N') OVER (PARTITION BY r.InstrumentId ORDER BY r.StartDateTime ASC)) AS ' + @PrevXDayHighGap + N',
            (r.[Low] - LAG(r.[Low], '+cast (@Days-1 as Nvarchar(10))+ N') OVER (PARTITION BY r.InstrumentId ORDER BY r.StartDateTime ASC)) AS ' + @PrevXDayLowGap + N',
            (r.[Close] - LAG(r.[Close], '+cast (@Days-1 as Nvarchar(10))+ N') OVER (PARTITION BY r.InstrumentId ORDER BY r.StartDateTime ASC)) AS ' + @PrevXDayCloseGap + N',
            (r.[Volume] - LAG(r.Volume, '+cast (@Days-1 as Nvarchar(10))+ N') OVER (PARTITION BY r.InstrumentId ORDER BY r.StartDateTime ASC)) AS ' + @PrevXDayVolumeGap + N',

            (r.[Open] - LAG(r.[Close], '+cast (@Days-1 as Nvarchar(10))+ N') OVER (PARTITION BY r.InstrumentId ORDER BY r.StartDateTime ASC)) AS ' + @PrevXDayOpenCloseGap + N',
            ((r.[Open] - LAG(r.[Close], '+cast (@Days-1 as Nvarchar(10))+ N') OVER (PARTITION BY r.InstrumentId ORDER BY r.StartDateTime ASC)) * 100 /LAG(r.[Close], '+cast (@Days-1 as Nvarchar(10))+ N') OVER (PARTITION BY r.InstrumentId ORDER BY r.StartDateTime ASC)) AS ' + @PrevXDayOpenCloseGapPercent + N'

        FROM RollingData h
        JOIN RankedPrices r 
            ON r.InstrumentId = h.InstrumentId 
            AND r.StartDateTime = h.StartDateTime 
        )'

SET @SQLMedianMedian1 = N', RankedValues AS
                    (
                        SELECT O1.InstrumentId, O1.StartDateTime,
                            RV.[Open] AS OpenVal, RV.High AS HighVal, RV.Low AS LowVal, RV.[Close] AS CloseVal, RV.Volume AS VolumeVal,
                            ROW_NUMBER() OVER (PARTITION BY O1.InstrumentId, O1.StartDateTime ORDER BY RV.StartDateTime ASC) AS rn_asc,
                            COUNT(*) OVER (PARTITION BY O1.InstrumentId, O1.StartDateTime) AS cnt
                        FROM StocksOhlcv_1440 O1
                        CROSS APPLY
                        (
                            SELECT TOP (' + cast (@Days as Nvarchar(10))+N') *
                            FROM StocksOhlcv_1440 O2
                            WHERE O2.InstrumentId = O1.InstrumentId AND O2.StartDateTime <= O1.StartDateTime
                            ORDER BY O2.StartDateTime DESC
                        ) RV
                    ),
                    MedianCalc AS
                    (
                        SELECT InstrumentId, StartDateTime,  OpenVal, HighVal, LowVal, CloseVal, VolumeVal, rn_asc, cnt
                        FROM RankedValues
                    )'

SET @SQLMedianMedian2 = N', Median AS (
                    SELECT InstrumentId, StartDateTime,
                    CASE 
                      WHEN cnt % 2 = 1 THEN 
                        MAX(CASE WHEN rn_asc = (cnt + 1) / 2 THEN OpenVal END)
                      ELSE
                        AVG(CASE WHEN rn_asc IN ((cnt / 2), (cnt / 2) + 1) THEN OpenVal END)
                    END AS ' + @PrevXDaysMedianOpen + N',

                    CASE 
                      WHEN cnt % 2 = 1 THEN 
                        MAX(CASE WHEN rn_asc = (cnt + 1) / 2 THEN HighVal END)
                      ELSE
                        AVG(CASE WHEN rn_asc IN ((cnt / 2), (cnt / 2) + 1) THEN HighVal END)
                    END AS ' + @PrevXDaysMedianHigh + N',

                    CASE 
                      WHEN cnt % 2 = 1 THEN 
                        MAX(CASE WHEN rn_asc = (cnt + 1) / 2 THEN LowVal END)
                      ELSE
                        AVG(CASE WHEN rn_asc IN ((cnt / 2), (cnt / 2) + 1) THEN LowVal END)
                    END AS  ' + @PrevXDaysMedianLow + N',

                    CASE 
                      WHEN cnt % 2 = 1 THEN 
                        MAX(CASE WHEN rn_asc = (cnt + 1) / 2 THEN CloseVal END)
                      ELSE
                        AVG(CASE WHEN rn_asc IN ((cnt / 2), (cnt / 2) + 1) THEN CloseVal END)
                    END AS  ' + @PrevXDaysMedianClose + N',

                    CASE 
                      WHEN cnt % 2 = 1 THEN 
                        MAX(CASE WHEN rn_asc = (cnt + 1) / 2 THEN VolumeVal END)
                      ELSE
                        AVG(CASE WHEN rn_asc IN ((cnt / 2), (cnt / 2) + 1) THEN VolumeVal END)
                    END AS  ' + @PrevXDaysMedianVolume + N'

                FROM MedianCalc
                GROUP BY InstrumentId, StartDateTime, cnt
                )'

SET @Query = N'SELECT f.*, ' +  @PrevXDaysMedianOpen +','+  @PrevXDaysMedianHigh +','
                        +  @PrevXDaysMedianLow +','+  @PrevXDaysMedianClose +','+  @PrevXDaysMedianVolume 
                        + ' INTO ' + @Tablename + ' FROM Final f
                      JOIN Median m
                      ON m.InstrumentId = f.InstrumentId and f.StartdateTime = m.StartDateTime
                      WHERE Ranking <= ' + CAST(@Top AS NVARCHAR) + N' 
                      order by f.InstrumentId, f.StartDateTime desc;'

Print @SQL 
Print @SQLLag
PRINT @SQLMedianMedian1
PRINT @SQLMedianMedian2
PRINT @Query

SET @Query = CONCAT('DROP TABLE IF EXISTS ', @TableName, ';', @SQL, @SQLLag, @SQLMedianMedian1, @SQLMedianMedian2, @Query )
PRINT '============= Table Name: ' +  @TableName + ' ================='

-- Execute the dynamic SQL
EXEC  sp_executesql  @Query 

END
GO
