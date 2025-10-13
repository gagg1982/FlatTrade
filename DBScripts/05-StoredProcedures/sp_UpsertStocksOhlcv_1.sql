IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_UpsertStocksOhlcv_1]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_UpsertStocksOhlcv_1]
GO

CREATE PROCEDURE [dbo].[sp_UpsertStocksOhlcv_1]
(
    @tvpData [dbo].[TStocksOhlcv] READONLY
)
AS
BEGIN
    SET NOCOUNT ON;

_START:
    BEGIN TRY
        BEGIN TRANSACTION
     
            MERGE INTO dbo.StocksOhlcv_1 AS Target
            USING (SELECT a.Id, b.* FROM dbo.StockInstruments a join @tvpData b ON a.Token = b.Token ) AS Source
            ON Target.[InstrumentId] = Source.[Id] and Target.StartdateTime =  Source.StartDateTime
            WHEN MATCHED THEN
                UPDATE SET Target.[Open] = Source.[Open],
                           Target.[High] = Source.[High],
                           Target.[Low] = Source.[Low],
                           Target.[Close] = Source.[Close],
                           Target.[Volume] = Source.[Volume]
            WHEN NOT MATCHED THEN
                INSERT ([InstrumentId], [StartDateTime], [Open], [High], [Low], [Close], [Volume])
                VALUES (Source.Id, Source.StartDateTime, Source.[Open], Source.[High], Source.[Low], Source.[Close], Source.Volume);

        IF (XACT_STATE()) <> -1
           COMMIT TRANSACTION

    END TRY

    BEGIN CATCH
        IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

        DECLARE @ErrorNumber INT = ERROR_NUMBER()
        BEGIN
           declare @message nvarchar(max) = N'An error occurred while merging [StocksOhlcv]. ' + ERROR_MESSAGE()
           ; THROW 51002, @message, 1
        END
    END CATCH
END
GO