IF OBJECT_ID('[dbo].[sp_UpsertStocksOhlcv_1]', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[sp_UpsertStocksOhlcv_1];
GO

CREATE PROCEDURE [dbo].[sp_UpsertStocksOhlcv_1]
(
    @tvpData dbo.TStocksOhlcv READONLY
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Step 1: Materialize TVP + Join StockInstruments into a temp table
        CREATE TABLE #Source
        (
            InstrumentId INT NOT NULL,
            StartDateTime DATETIME2 NOT NULL,
            [Open] DECIMAL(18,4) NULL,
            [High] DECIMAL(18,4) NULL,
            [Low] DECIMAL(18,4) NULL,
            [Close] DECIMAL(18,4) NULL,
            [Volume] BIGINT NULL
        );

        INSERT INTO #Source (InstrumentId, StartDateTime, [Open], [High], [Low], [Close], [Volume])
        SELECT a.Id, b.StartDateTime, b.[Open], b.[High], b.[Low], b.[Close], b.[Volume]
        FROM dbo.StockInstruments a
        JOIN @tvpData b ON a.Token = b.Token;

        -- Step 2: Add index for faster join and existence check
        CREATE CLUSTERED INDEX IX_Source_Id_Date ON #Source (InstrumentId, StartDateTime);

        -- Step 3: Update existing rows
        UPDATE T
        SET T.[Open] = S.[Open],
            T.[High] = S.[High],
            T.[Low]  = S.[Low],
            T.[Close] = S.[Close],
            T.[Volume] = S.[Volume]
        FROM dbo.StocksOhlcv_1 T
        INNER JOIN #Source S
            ON T.InstrumentId = S.InstrumentId
           AND T.StartDateTime = S.StartDateTime;

        -- Step 4: Insert new rows
        INSERT INTO dbo.StocksOhlcv_1
            (InstrumentId, StartDateTime, [Open], [High], [Low], [Close], [Volume])
        SELECT S.InstrumentId, S.StartDateTime, S.[Open], S.[High], S.[Low], S.[Close], S.[Volume]
        FROM #Source S
        WHERE NOT EXISTS (
            SELECT 1
            FROM dbo.StocksOhlcv_1 T
            WHERE T.InstrumentId = S.InstrumentId
              AND T.StartDateTime = S.StartDateTime
        );

        COMMIT TRANSACTION;
    END TRY

    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        DECLARE @Error NVARCHAR(MAX) =
            N'Error in [sp_UpsertStocksOhlcv_1]: ' + ERROR_MESSAGE();
        THROW 51002, @Error, 1;
    END CATCH
END
GO
