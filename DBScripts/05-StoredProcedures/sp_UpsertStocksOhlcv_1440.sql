IF OBJECT_ID('[dbo].[sp_UpsertStocksOhlcv_1440]', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[sp_UpsertStocksOhlcv_1440];
GO

CREATE PROCEDURE [dbo].[sp_UpsertStocksOhlcv_1440]
(
    @tvpData dbo.TStocksOhlcv READONLY
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -------------------------------------------------------------------------
        -- Step 1: Materialize and pre-join to StockInstruments in a temp table
        -------------------------------------------------------------------------
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
        SELECT si.Id, t.StartDateTime, t.[Open], t.[High], t.[Low], t.[Close], t.[Volume]
        FROM dbo.StockInstruments si
        JOIN @tvpData t ON si.Token = t.Token;

        -------------------------------------------------------------------------
        -- Step 2: Index for efficient lookups
        -------------------------------------------------------------------------
        CREATE CLUSTERED INDEX IX_Source_Id_Date ON #Source (InstrumentId, StartDateTime);

        -------------------------------------------------------------------------
        -- Step 3: Update existing rows (only those that changed)
        -------------------------------------------------------------------------
        UPDATE T
        SET 
            T.[Open]  = S.[Open],
            T.[High]  = S.[High],
            T.[Low]   = S.[Low],
            T.[Close] = S.[Close],
            T.[Volume] = S.[Volume]
        FROM dbo.StocksOhlcv_1440 AS T
        INNER JOIN #Source AS S
            ON T.InstrumentId = S.InstrumentId
           AND T.StartDateTime = S.StartDateTime;

        -------------------------------------------------------------------------
        -- Step 4: Insert new rows (not already present)
        -------------------------------------------------------------------------
        INSERT INTO dbo.StocksOhlcv_1440
            (InstrumentId, StartDateTime, [Open], [High], [Low], [Close], [Volume])
        SELECT 
            S.InstrumentId, S.StartDateTime, 
            S.[Open], S.[High], S.[Low], S.[Close], S.[Volume]
        FROM #Source AS S
        WHERE NOT EXISTS (
            SELECT 1
            FROM dbo.StocksOhlcv_1440 AS T WITH (NOLOCK)
            WHERE T.InstrumentId = S.InstrumentId
              AND T.StartDateTime = S.StartDateTime
        );

        COMMIT TRANSACTION;
    END TRY

    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        DECLARE @Error NVARCHAR(MAX) =
            N'Error in [sp_UpsertStocksOhlcv_1440]: ' + ERROR_MESSAGE();
        THROW 51001, @Error, 1;
    END CATCH
END
GO
