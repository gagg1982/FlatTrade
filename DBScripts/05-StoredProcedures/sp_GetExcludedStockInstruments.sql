
CREATE OR ALTER PROCEDURE dbo.sp_GetExcludedStockInstruments
    @Token INT = NULL,                  -- Optional: filter by token
    @Exchange NVARCHAR(64) = NULL       -- Optional: filter by exchange
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        Token,
        TradingSymbol,
        Exchange
    FROM
        dbo.ExcludedStockInstruments
    WHERE
        (@Token IS NULL OR Token = @Token)
        AND (@Exchange IS NULL OR Exchange = @Exchange)
    ORDER BY
        TradingSymbol;
END;
GO