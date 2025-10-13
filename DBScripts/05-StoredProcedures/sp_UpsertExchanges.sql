IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_UpsertExchanges]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_UpsertExchanges]
GO

CREATE PROCEDURE [dbo].[sp_UpsertExchanges]
(
    @tvpData [dbo].[TExchanges] READONLY
)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION
     
            MERGE INTO dbo.Exchange AS Target
            USING @tvpData AS Source
            ON Target.[Code] = Source.[Code]
            WHEN MATCHED THEN
                UPDATE SET Target.[Description] = Source.[Description],
                           Target.[LastUpdatedInDbAt] = GetDate()
            WHEN NOT MATCHED BY SOURCE AND Target.[Active] != 0 THEN
                 UPDATE SET Target.[Active] = 0,
                            Target.[LastUpdatedInDbAt] = GetDate()
            WHEN NOT MATCHED BY TARGET THEN
                INSERT ([Code], [Description], [Active])
                VALUES (Source.[Code], Source.[Description], 1);


        IF (XACT_STATE()) <> -1
           COMMIT TRANSACTION

    END TRY

    BEGIN CATCH
        IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

        DECLARE @ErrorNumber INT = ERROR_NUMBER()
        BEGIN
           declare @message nvarchar(max) = N'An error occurred while merging [Exchanges]. ' + ERROR_MESSAGE()
           ; THROW 51000, @message, 1
        END
    END CATCH
END
GO