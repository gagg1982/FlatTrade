IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_UpsertHistoricNpsNav]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_UpsertHistoricNpsNav]
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_UpsertHistoricNpsNav]
(
    @tvp_data [dbo].[THistoricNpsNav] READONLY
)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION
            -- Update existing rows
            UPDATE T
            SET 
                T.nav = S.nav,
                T.scheme_name = S.scheme_name,
                T.updated_at = SYSUTCDATETIME()
            FROM dbo.HistoricNpsNav T
            INNER JOIN @tvp_data S
                ON T.scheme_code = S.scheme_code
                AND T.nav_date = S.nav_date;

            -- Insert new rows
            INSERT INTO dbo.HistoricNpsNav
            (
                scheme_code,
                scheme_name,
                nav,
                nav_date
            )
            SELECT 
                S.scheme_code,
                S.scheme_name,
                S.nav,
                S.nav_date
            FROM @tvp_data S
            LEFT JOIN dbo.HistoricNpsNav T
                ON T.scheme_code = S.scheme_code
                AND T.nav_date = S.nav_date
            WHERE T.scheme_code IS NULL;                

        IF (XACT_STATE()) <> -1
           COMMIT TRANSACTION

    END TRY

    BEGIN CATCH
        IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

        DECLARE @ErrorNumber INT = ERROR_NUMBER()
        BEGIN
           declare @message nvarchar(max) = N'An error occurred while merging [HistoricNpsNav]. ' + ERROR_MESSAGE()
           ; THROW 51000, @message, 1
        END
    END CATCH
END
GO