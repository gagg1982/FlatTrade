IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_UpsertTradeBook]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_UpsertTradeBook]
GO

CREATE PROCEDURE dbo.sp_UpsertTradeBook
    @tvpData dbo.TTradeBook READONLY
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION
        MERGE dbo.TradeBook AS target
        USING @tvpData AS src
            ON target.NorenOrderNumber = src.NorenOrderNumber
                AND target.FillId = src.FillId
                AND target.Exchange = src.Exchange
        WHEN MATCHED THEN
            UPDATE SET
                target.TradingSymbol        = src.TradingSymbol,
                target.Remarks              = src.Remarks,
                target.SnoOrderDt           = src.SnoOrderDt,
                target.UserId               = src.UserId,
                target.AccountId            = src.AccountId,
                target.PriceType            = src.PriceType,
                target.RetentionType        = src.RetentionType,
                target.ProductDisplayName   = src.ProductDisplayName,
                target.ProductType          = src.ProductType,
                target.FillDateTime         = src.FillDateTime,
                target.TransactionType      = src.TransactionType,
                target.Quantity             = src.Quantity,
                target.Token                = src.Token,
                target.TotalFilled          = src.TotalFilled,
                target.FillQuantity         = src.FillQuantity,
                target.Multiplier           = src.Multiplier,
                target.PricePrecision       = src.PricePrecision,
                target.TickSize             = src.TickSize,
                target.LotSize              = src.LotSize,
                target.Price                = src.Price,
                target.PriceFactor          = src.PriceFactor,
                target.FillPrice            = src.FillPrice,
                target.NorenTime            = src.NorenTime,
                target.AveragePrice         = src.AveragePrice,
                target.ExchangeOrderNumber  = src.ExchangeOrderNumber,
                target.ExchangeTime         = src.ExchangeTime
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (
                Exchange,
                TradingSymbol,
                SnoOrderNumber,
                Remarks,
                SnoOrderDt,
                NorenOrderNumber,
                UserId,
                AccountId,
                PriceType,
                RetentionType,
                ProductDisplayName,
                ProductType,
                FillDateTime,
                FillId,
                TransactionType,
                Quantity,
                Token,
                TotalFilled,
                FillQuantity,
                Multiplier,
                PricePrecision,
                TickSize,
                LotSize,
                Price,
                PriceFactor,
                FillPrice,
                NorenTime,
                AveragePrice,
                ExchangeOrderNumber,
                ExchangeTime
            )
            VALUES (
                src.Exchange,
                src.TradingSymbol,
                src.SnoOrderNumber,
                src.Remarks,
                src.SnoOrderDt,
                src.NorenOrderNumber,
                src.UserId,
                src.AccountId,
                src.PriceType,
                src.RetentionType,
                src.ProductDisplayName,
                src.ProductType,
                src.FillDateTime,
                src.FillId,
                src.TransactionType,
                src.Quantity,
                src.Token,
                src.TotalFilled,
                src.FillQuantity,
                src.Multiplier,
                src.PricePrecision,
                src.TickSize,
                src.LotSize,
                src.Price,
                src.PriceFactor,
                src.FillPrice,
                src.NorenTime,
                src.AveragePrice,
                src.ExchangeOrderNumber,
                src.ExchangeTime
            );

        IF (XACT_STATE()) <> -1
           COMMIT TRANSACTION

    END TRY

    BEGIN CATCH
        IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

        DECLARE @ErrorNumber INT = ERROR_NUMBER()
        BEGIN
           declare @message nvarchar(max) = N'An error occurred while merging [TradeBook]. ' + ERROR_MESSAGE()
           ; THROW 51000, @message, 1
        END
    END CATCH
END;
GO