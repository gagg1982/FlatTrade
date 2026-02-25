--select * from CorporateActions
CREATE OR ALTER PROCEDURE dbo.sp_UpsertCorporateActions
(
    @tvpData dbo.TCorporateActions READONLY
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRAN;

    MERGE dbo.CorporateActions AS TARGET
    USING @tvpData AS SOURCE
        ON  TARGET.Symbol = SOURCE.Symbol
        AND TARGET.Exchange = SOURCE.Exchange
        AND TARGET.ExDate = SOURCE.ExDate
        AND TARGET.Subject = SOURCE.Subject

    WHEN MATCHED THEN
        UPDATE SET
			TARGET.Series                  = SOURCE.Series,
            TARGET.Indicative              = SOURCE.Indicative,
            TARGET.FaceValue               = SOURCE.FaceValue,
            TARGET.RecordDate              = SOURCE.RecordDate,
			TARGET.Isin			           = SOURCE.Isin,
            TARGET.BookClosureStartDate    = SOURCE.BookClosureStartDate,
            TARGET.BookClosureEndDate      = SOURCE.BookClosureEndDate,
            TARGET.NoDeliveryStartDate          = SOURCE.NoDeliveryStartDate,
			TARGET.NoDeliveryEndDate          = SOURCE.NoDeliveryEndDate,
            TARGET.AnnouncementDate        = SOURCE.AnnouncementDate,
            TARGET.CompanyName             = SOURCE.CompanyName,
            TARGET.UpdatedAt               = GETDATE()

    WHEN NOT MATCHED BY TARGET THEN
        INSERT
        (
            Symbol,
			Exchange,
            Series,
            Indicative,
            FaceValue,
            Subject,
            ExDate,
            RecordDate,
            BookClosureStartDate,
            BookClosureEndDate,
            NoDeliveryStartDate,
			NoDeliveryEndDate,
            AnnouncementDate,
            CompanyName,
            Isin
        )
        VALUES
        (
            SOURCE.Symbol,
			SOURCE.Exchange,
            SOURCE.Series,
            SOURCE.Indicative,
            SOURCE.FaceValue,
            SOURCE.Subject,
            SOURCE.ExDate,
            SOURCE.RecordDate,
            SOURCE.BookClosureStartDate,
            SOURCE.BookClosureEndDate,
            SOURCE.NoDeliveryStartDate,
			SOURCE.NoDeliveryEndDate,
            SOURCE.AnnouncementDate,
            SOURCE.CompanyName,
            SOURCE.Isin
        );

    COMMIT TRAN;
END;
GO
