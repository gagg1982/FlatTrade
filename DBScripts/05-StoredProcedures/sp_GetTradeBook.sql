
CREATE OR ALTER PROCEDURE dbo.sp_GetTradeBook
    @StartDate DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT * FROM  dbo.TradeBook
    WHERE   (@StartDate IS NULL OR cast(FillDateTime as date) >= cast(@StartDate as Date))

END;
GO
