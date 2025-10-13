
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_GetFilteredStocks]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_GetFilteredStocks]
GO

CREATE PROCEDURE  [dbo].[sp_GetFilteredStocks]
    @Top INT,
    @MinimumDaysCount INT = 140,
    @Exchange NVARCHAR(8)= N'NSE',
    @StartPriceRange Decimal(15,4) = 100,
    @EndPriceRange Decimal(15,4) = 400

AS
BEGIN
    SET NOCOUNT ON;
    SET TRANSACTION ISOLATION LEVEL SNAPSHOT;

    Declare @TableName1 NVARCHAR(100) ;
    Declare @TableName2 NVARCHAR(100) ;
    Declare @TableName3 NVARCHAR(100) ;
    Declare @TableName4 NVARCHAR(100) ;
    Declare @TableName5 NVARCHAR(100) ;


    DECLARE @Days INT= 2
    SET @TableName1 =  QUOTENAME(CONCAT('Latest', @Top,'RecordsForRolling',@Days,'DaysAnalysis'));
    exec  sp_GetRollingStockStats @Top, @Days, @TableName1;

    SET @Days = 5
    SET @TableName2 = QUOTENAME(CONCAT('Latest', @Top,'RecordsForRolling',@Days,'DaysAnalysis'));
    exec  sp_GetRollingStockStats @Top, @Days, @TableName2;

    SET @Days = 10
    SET @TableName3 = QUOTENAME(CONCAT('Latest', @Top,'RecordsForRolling',@Days,'DaysAnalysis'));
    exec  sp_GetRollingStockStats @Top, @Days, @TableName3;

    SET @Days = 18
    SET @TableName4 = QUOTENAME(CONCAT('Latest', @Top,'RecordsForRolling',@Days,'DaysAnalysis'));
    exec  sp_GetRollingStockStats @Top, @Days, @TableName4;

    SET @Days = 30
    SET @TableName5 = QUOTENAME(CONCAT('Latest', @Top,'RecordsForRolling',@Days,'DaysAnalysis'));
    exec  sp_GetRollingStockStats @Top, @Days, @TableName5;

    DECLARE @SQL NVARCHAR(MAX) = N' SELECT  f.Token, f.TradingSymbol, f.CompanyName, f.ExchangeCode, f.TickSize, f.PricePrecision, f.Active, f.LastTradePrice,
            f.AverageTradePrice, CONCAT(CAST(f.LastTradeDate AS DATE) ,'' '', f.LastTradeTime) as LastTradeDateTime,
            f.UpperCircuit, f.LowerCircuit, f.Wk52High, f.Wk52Low, a.*, b.*, c.*, d.*, e.*, g.*, q.CandleCount AS OneMinCandleCount, q.StdDevOnClose FROM ' + @TableName1 + N' a
    JOIN ' + @TableName2 + N' b ON b.StartDateTime = a.StartDateTime and b.InstrumentId = a.InstrumentId
    JOIN ' + @TableName3 + N' c ON c.StartDateTime = a.StartDateTime and c.InstrumentId = a.InstrumentId
    JOIN ' + @TableName4 + N' d ON d.StartDateTime = a.StartDateTime and d.InstrumentId = a.InstrumentId
    JOIN ' + @TableName5 + N' e ON e.StartDateTime = a.StartDateTime and e.InstrumentId = a.InstrumentId
    JOIN StocksOhlcv_1440 g ON g.StartDateTime = a.StartDateTime and g.InstrumentId = a.InstrumentId
    JOIN (SELECT InstrumentId, CAST(StartDateTime as Date) As TradeDate,  StDev([Close]) AS StdDevOnClose, count(*) As CandleCount FROM stocksOhlcv_1 GROUP BY InstrumentId, CAST(StartDateTime as Date) ) q ON q.InstrumentId= a.InstrumentId AND q.TradeDate = a.StartDateTime
    JOIN StockInstruments f ON f.Id = a.InstrumentId AND f.Active = 1 
    ORDER BY a.InstrumentId asc, a.ranking asc, a.StartDateTime desc;'

    PRINT @SQL
    EXEC sp_executesql @SQL

END
GO

