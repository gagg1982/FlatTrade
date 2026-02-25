IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_UpsertLatestNpsNav]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_UpsertLatestNpsNav]
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_UpsertLatestNpsNav]
(
    @tvp_data [dbo].[TLatestNpsNav] READONLY
)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @LaterDate DATE = cast(dateadd(day, -30, getUtcDate()) as date)
    BEGIN TRY
        BEGIN TRANSACTION
            MERGE dbo.LatestNpsNav AS TARGET
    USING @tvp_data AS SOURCE
        ON TARGET.SchemeCode = SOURCE.SchemeCode 
        AND  TARGET.PFMCode = SOURCE.PFMCode  
        AND TARGET.NavDate = SOURCE.NavDate 

    WHEN MATCHED THEN
        UPDATE SET
            TARGET.SchemeName  = SOURCE.SchemeName,
            TARGET.PFMName     = SOURCE.PFMName,
            TARGET.NAV         = SOURCE.NAV,
            TARGET.Return_1D   = SOURCE.Return_1D,
            TARGET.Return_7D   = SOURCE.Return_7D,
            TARGET.Return_1M   = SOURCE.Return_1M,
            TARGET.Return_3M   = SOURCE.Return_3M,
            TARGET.Return_6M   = SOURCE.Return_6M,
            TARGET.Return_1Y   = SOURCE.Return_1Y,
            TARGET.Return_3Y   = SOURCE.Return_3Y,
            TARGET.Return_5Y   = SOURCE.Return_5Y,
            TARGET.LastUpdated = SYSUTCDATETIME()

    WHEN NOT MATCHED THEN
        INSERT (
            SchemeCode,
            SchemeName,
            PFMCode,
            PFMName,
            NavDate,
            NAV,
            Return_1D,
            Return_7D,
            Return_1M,
            Return_3M,
            Return_6M,
            Return_1Y,
            Return_3Y,
            Return_5Y
        )
        VALUES (
            SOURCE.SchemeCode,
            SOURCE.SchemeName,
            SOURCE.PFMCode,
            SOURCE.PFMName,
            SOURCE.NavDate,
            SOURCE.NAV,
            SOURCE.Return_1D,
            SOURCE.Return_7D,
            SOURCE.Return_1M,
            SOURCE.Return_3M,
            SOURCE.Return_6M,
            SOURCE.Return_1Y,
            SOURCE.Return_3Y,
            SOURCE.Return_5Y
        );

        INSERT INTO HistoricNpsNav 
        (Scheme_Code, Scheme_name, nav, nav_date, Created_At, Updated_at )
        SELECT l.SchemeCode , l.SchemeName , l.nav, l.NavDate, SYSUTCDATETIME() , SYSUTCDATETIME() 
        FROM
        (
            SELECT SchemeCode , SchemeName , nav, NavDate
            FROM LatestNpsNav 
            WHERE NavDate > @LaterDate
        ) l
        LEFT JOIN HistoricNpsNav h
        ON l.SchemeCode = h.Scheme_code and h.nav_date = l.navdate
        WHERE h.nav_date IS NULL

        IF (XACT_STATE()) <> -1
           COMMIT TRANSACTION

    END TRY

    BEGIN CATCH
        IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

        DECLARE @ErrorNumber INT = ERROR_NUMBER()
        BEGIN
           declare @message nvarchar(max) = N'An error occurred while merging [LatestNpsNav]. ' + ERROR_MESSAGE()
           ; THROW 51000, @message, 1
        END
    END CATCH
END
GO