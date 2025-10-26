IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_UpsertBrokerageAndTaxes]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_UpsertBrokerageAndTaxes]
GO

CREATE OR ALTER PROCEDURE dbo.sp_UpsertBrokerageAndTaxes
    @tvpData dbo.TBrokerageAndTaxes READONLY
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION

        MERGE dbo.BrokerageAndTaxes AS target
    USING @tvpData AS source
    ON target.token = source.token and target.FillId = source.FillId  AND target.Exchange = source.Exchange
    WHEN MATCHED THEN
        UPDATE SET
            TradingSymbol = source.TradingSymbol,
            Exchange = source.Exchange,
            BrokerageAmount = source.BrokerageAmount,
            ClearingMemberAmount = source.ClearingMemberAmount,
            FillDateTime = source.FillDateTime,
            FillPrice = source.FillPrice,
            FillQuantity = source.FillQuantity,
            Gst = source.Gst,
            InvestorProtectionFundTrustAmount = source.InvestorProtectionFundTrustAmount,
            NorenOrderNumber = source.NorenOrderNumber,
            SnoOrderNumber = source.SnoOrderNumber,
            ExchangeOrderNumber = source.ExchangeOrderNumber,
            ProductType = source.ProductType,
            Remarks = source.Remarks,
            SebiCharges = source.SebiCharges,
            ExchangeCharges = source.ExchangeCharges,
            SecurityTransactionTax = source.SecurityTransactionTax,
            StampDuty = source.StampDuty,
            Token = source.Token,
            TotalCharges = source.TotalCharges,
            TransactionType = source.TransactionType,
            Url = source.Url,
            LastModifiedAt =  SYSDATETIME()
    WHEN NOT MATCHED THEN
        INSERT (
            TradingSymbol, Exchange, BrokerageAmount, ClearingMemberAmount, ExchangeOrderNumber,
            FillDateTime, FillId, FillPrice, FillQuantity, Gst,
            InvestorProtectionFundTrustAmount, NorenOrderNumber, SnoOrderNumber, NorenTime, ProductType,
            Remarks, SebiCharges, ExchangeCharges, SecurityTransactionTax, StampDuty,
            Token, TotalCharges, TransactionType, Url, LastModifiedAt
        )
        VALUES (
            source.TradingSymbol, source.Exchange, source.BrokerageAmount, source.ClearingMemberAmount, source.ExchangeOrderNumber,
            source.FillDateTime, source.FillId, source.FillPrice, source.FillQuantity, source.Gst,
            source.InvestorProtectionFundTrustAmount, source.NorenOrderNumber, source.SnoOrderNumber, source.NorenTime, source.ProductType,
            source.Remarks, source.SebiCharges, source.ExchangeCharges, source.SecurityTransactionTax, source.StampDuty,
            source.Token, source.TotalCharges, source.TransactionType, source.Url, SYSDATETIME()
        );

        IF (XACT_STATE()) <> -1
           COMMIT TRANSACTION

    END TRY

    BEGIN CATCH
        IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

        DECLARE @ErrorNumber INT = ERROR_NUMBER()
        BEGIN
           declare @message nvarchar(max) = N'An error occurred while merging [BrokerageAndTaxes]. ' + ERROR_MESSAGE()
           ; THROW 51100, @message, 1
        END
    END CATCH
END
GO;