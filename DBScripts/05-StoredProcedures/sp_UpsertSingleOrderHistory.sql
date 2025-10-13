IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_UpsertSingleOrderHistory]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_UpsertSingleOrderHistory]
GO

CREATE PROCEDURE dbo.sp_UpsertSingleOrderHistory
    @tvpData dbo.TSingleOrderHistory READONLY
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
    BEGIN TRANSACTION
    MERGE dbo.SingleOrderHistory AS target
    USING @tvpData AS src
        ON target.NorenOrderNumber = src.NorenOrderNumber
       AND target.Exchange   = src.Exchange
       AND target.ReportType = src.ReportType
       AND target.KidId = src.KidId
       AND target.InternalOrderStatus = src.InternalOrderStatus
    WHEN MATCHED THEN
        UPDATE SET
            target.Exchange                 = src.Exchange,
            target.TradingSymbol            = src.TradingSymbol,
            target.SnoOrderDt               = src.SnoOrderDt,
            target.ExchangeTime             = src.ExchangeTime,
            target.OptionalIntropExchange   = src.OptionalIntropExchange,
            target.NorenTime                = src.NorenTime,
            target.ProductDisplayName       = src.ProductDisplayName,
            target.FillQuantity             = src.FillQuantity,
            target.FillPrice                = src.FillPrice,
            target.FillId                   = src.FillId,
            target.Quantity                 = src.Quantity,
            target.Price                    = src.Price,
            target.ProductType              = src.ProductType,
            target.OrderSource              = src.OrderSource,
            target.RejectionBy              = src.RejectionBy,
            target.Pan                      = src.Pan,
            target.SourceUid                = src.SourceUid,
            target.OrderStatus              = src.OrderStatus,
            target.ReportType               = src.ReportType,
            target.TransactionType          = src.TransactionType,
            target.PriceType                = src.PriceType,
            target.TotalFilled              = src.TotalFilled,
            target.AvgPriceOfTradedQuantity = src.AvgPriceOfTradedQuantity,
            target.RejectionReason          = src.RejectionReason,
            target.ExchangeOrderNumber      = src.ExchangeOrderNumber,
            target.CancelledQuantity        = src.CancelledQuantity,
            target.Remarks                  = src.Remarks,
            target.DisclosedQuantity        = src.DisclosedQuantity,
            target.TriggerPrice             = src.TriggerPrice,
            target.RetentionType            = src.RetentionType,
            target.UserId                   = src.UserId,
            target.AccountId                = src.AccountId,
            target.BookProfitPrice          = src.BookProfitPrice,
            target.BookLossProfit           = src.BookLossProfit,
            target.TrailingPrice            = src.TrailingPrice,
            target.Amo                      = src.Amo,
            target.PricePrecision           = src.PricePrecision,
            target.TickSize                 = src.TickSize,
            target.LotSize                  = src.LotSize,
            target.Token                    = src.Token,
            target.OrderDateTime            = src.OrderDateTime,
            target.EpochOrderEntryDateTime  = src.EpochOrderEntryDateTime,
            target.Extm                     = src.Extm
    WHEN NOT MATCHED BY TARGET THEN
        INSERT (
            Exchange,
            TradingSymbol,
            NorenOrderNumber,
            InternalOrderStatus,
            SnoOrderDt,
            SnoOrderNumber,
            ExchangeTime,
            OptionalIntropExchange,
            NorenTime,
            ProductDisplayName,
            FillQuantity,
            FillPrice,
            FillId,
            Quantity,
            Price,
            ProductType,
            KidId,
            OrderSource,
            RejectionBy,
            Pan,
            SourceUid,
            OrderStatus,
            ReportType,
            TransactionType,
            PriceType,
            TotalFilled,
            AvgPriceOfTradedQuantity,
            RejectionReason,
            ExchangeOrderNumber,
            CancelledQuantity,
            Remarks,
            DisclosedQuantity,
            TriggerPrice,
            RetentionType,
            UserId,
            AccountId,
            BookProfitPrice,
            BookLossProfit,
            TrailingPrice,
            Amo,
            PricePrecision,
            TickSize,
            LotSize,
            Token,
            OrderDateTime,
            EpochOrderEntryDateTime,
            Extm
        )
        VALUES (
            src.Exchange,
            src.TradingSymbol,
            src.NorenOrderNumber,
            src.InternalOrderStatus,
            src.SnoOrderDt,
            src.SnoOrderNumber,
            src.ExchangeTime,
            src.OptionalIntropExchange,
            src.NorenTime,
            src.ProductDisplayName,
            src.FillQuantity,
            src.FillPrice,
            src.FillId,
            src.Quantity,
            src.Price,
            src.ProductType,
            src.KidId,
            src.OrderSource,
            src.RejectionBy,
            src.Pan,
            src.SourceUid,
            src.OrderStatus,
            src.ReportType,
            src.TransactionType,
            src.PriceType,
            src.TotalFilled,
            src.AvgPriceOfTradedQuantity,
            src.RejectionReason,
            src.ExchangeOrderNumber,
            src.CancelledQuantity,
            src.Remarks,
            src.DisclosedQuantity,
            src.TriggerPrice,
            src.RetentionType,
            src.UserId,
            src.AccountId,
            src.BookProfitPrice,
            src.BookLossProfit,
            src.TrailingPrice,
            src.Amo,
            src.PricePrecision,
            src.TickSize,
            src.LotSize,
            src.Token,
            src.OrderDateTime,
            src.EpochOrderEntryDateTime,
            src.Extm
        );
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
END;
GO