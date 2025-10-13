IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_UpsertMarginEquity]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_UpsertMarginEquity]
GO


CREATE OR ALTER PROCEDURE dbo.sp_UpsertMarginEquity
    @tvpData dbo.TMarginEquity READONLY
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION
     
          MERGE INTO dbo.MarginEquity AS Target
            USING @tvpData AS Source
                ON Target.Symbol = Source.Symbol
               AND Target.Segment = Source.Segment
            WHEN MATCHED THEN
                UPDATE SET 
                    Target.MISMarginInPercentage = Source.MISMarginInPercentage,
                    Target.CNCMarginMISMarginInPercentage = Source.CNCMarginMISMarginInPercentage,
                    Target.MISLeverageX = Source.MISLeverageX,
                    Target.CNCLeverageX = Source.CNCLeverageX,
                    Target.LastUpdatedInDbAt = SYSUTCDATETIME()
            WHEN NOT MATCHED BY TARGET THEN
                INSERT (Symbol, Segment, MISMarginInPercentage, CNCMarginMISMarginInPercentage, MISLeverageX, CNCLeverageX, CreatedInDbAt, LastUpdatedInDbAt)
                VALUES (Source.Symbol, Source.Segment, Source.MISMarginInPercentage, Source.CNCMarginMISMarginInPercentage,
                        Source.MISLeverageX, Source.CNCLeverageX, SYSUTCDATETIME(), SYSUTCDATETIME());   


        IF (XACT_STATE()) <> -1
           COMMIT TRANSACTION

    END TRY

    BEGIN CATCH
        IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

        DECLARE @ErrorNumber INT = ERROR_NUMBER()
        BEGIN
           declare @message nvarchar(max) = N'An error occurred while merging [MarginEquity]. ' + ERROR_MESSAGE()
           ; THROW 51000, @message, 1
        END
    END CATCH
END
GO
