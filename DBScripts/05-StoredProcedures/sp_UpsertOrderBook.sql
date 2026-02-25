IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_UpsertOrderBook]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_UpsertOrderBook]
GO

CREATE PROCEDURE dbo.sp_UpsertOrderBook
    @tvpData dbo.TOrderBook READONLY
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION
        MERGE INTO dbo.OrderBook AS target
        USING @tvpData AS source
        ON target.NorenOrderNumber = source.NorenOrderNumber 
            and target.Exchange= source.Exchange 
            and target.EpochOrderEntryDateTime=source.EpochOrderEntryDateTime
        WHEN MATCHED THEN
        UPDATE SET
            target.UserId                  = source.UserId,
            target.AccountId               = source.AccountId,
            target.SymbolName              = source.SymbolName,
            target.KidId                   = source.KidId,
            target.TradingSymbol           = source.TradingSymbol,
            target.RejectionBy             = source.RejectionBy,
            target.SourceUid               = source.SourceUid,
            target.CompanyName             = source.CompanyName,
            target.Quantity                = source.Quantity,
            target.TotalFilled             = source.TotalFilled,
            target.MarginPriceFromModify   = source.MarginPriceFromModify,
            target.RTriggerPrice           = source.RTriggerPrice,
            target.RQuantity               = source.RQuantity,
            target.ROrderRemaningQuantity  = source.ROrderRemaningQuantity,
            target.OrderStatus             = source.OrderStatus,
            target.InternalOrderStatus     = source.InternalOrderStatus,
            target.IpAddress               = source.IpAddress,
            target.TriggerPrice            = source.TriggerPrice,
            target.Remarks                 = source.Remarks,
            target.TransactionType         = source.TransactionType,
            target.PriceType               = source.PriceType,
            target.RetentionType           = source.RetentionType,
            target.RejectionReason         = source.RejectionReason,
            target.Token                   = source.Token,
            target.Multiplier              = source.Multiplier,
            target.PriceFactor             = source.PriceFactor,
            target.InstrumentName          = source.InstrumentName,
            target.OrderSource             = source.OrderSource,
            target.PricePrecision          = source.PricePrecision,
            target.TickSize                = source.TickSize,
            target.LotSize                 = source.LotSize,
            target.Price                   = source.Price,
            target.AveragePrice            = source.AveragePrice,
            target.RPrice                  = source.RPrice,
            target.BookProfitPrice         = source.BookProfitPrice,
            target.BookLossPrice           = source.BookLossPrice,
            target.RBookLossPrice          = source.RBookLossPrice,
            target.TrailingPrice           = source.TrailingPrice,
            target.DisclosedQuantity       = source.DisclosedQuantity,
            target.CancelledQuantity       = source.CancelledQuantity,
            target.SnoFillId               = source.SnoFillId,
            target.SnoOrderNumber          = source.SnoOrderNumber,
            target.SnoOrderDt              = source.SnoOrderDt,
            target.BranchId                = source.BranchId,
            target.C                       = source.C,
            target.ProductDisplayName      = source.ProductDisplayName,
            target.ProductType             = source.ProductType,
            target.NorenTime               = source.NorenTime,
            target.ExchangeTime            = source.ExchangeTime,
            target.AlgoId                  = source.AlgoId,
            target.ExchangeOrderNumber     = source.ExchangeOrderNumber,
            target.Amo                     = source.Amo,
            target.MarketProtectionPercentage = source.MarketProtectionPercentage
        WHEN NOT MATCHED THEN
        INSERT (
            UserId, AccountId, SymbolName, KidId, NorenOrderNumber, Exchange, TradingSymbol,
            RejectionBy, SourceUid, CompanyName, Quantity, TotalFilled,
            MarginPriceFromModify, RTriggerPrice, RQuantity, ROrderRemaningQuantity,
            OrderStatus, InternalOrderStatus, IpAddress, EpochOrderEntryDateTime,
            TriggerPrice, Remarks, TransactionType, PriceType, RetentionType,
            RejectionReason, Token, Multiplier, PriceFactor, InstrumentName,
            OrderSource, PricePrecision, TickSize, LotSize, Price, AveragePrice,
            RPrice, BookProfitPrice, BookLossPrice, RBookLossPrice, TrailingPrice,
            DisclosedQuantity, CancelledQuantity, SnoFillId, SnoOrderNumber,
            SnoOrderDt, BranchId, C, ProductDisplayName, ProductType,
            NorenTime, ExchangeTime, AlgoId, ExchangeOrderNumber, amo, MarketProtectionPercentage
        )
        VALUES (
            source.UserId, source.AccountId, source.SymbolName, source.KidId, source.NorenOrderNumber, source.Exchange, source.TradingSymbol,
            source.RejectionBy, source.SourceUid, source.CompanyName, source.Quantity, source.TotalFilled,
            source.MarginPriceFromModify, source.RTriggerPrice, source.RQuantity, source.ROrderRemaningQuantity,
            source.OrderStatus, source.InternalOrderStatus, source.IpAddress, source.EpochOrderEntryDateTime,
            source.TriggerPrice, source.Remarks, source.TransactionType, source.PriceType, source.RetentionType,
            source.RejectionReason, source.Token, source.Multiplier, source.PriceFactor, source.InstrumentName,
            source.OrderSource, source.PricePrecision, source.TickSize, source.LotSize, source.Price, source.AveragePrice,
            source.RPrice, source.BookProfitPrice, source.BookLossPrice, source.RBookLossPrice, source.TrailingPrice,
            source.DisclosedQuantity, source.CancelledQuantity, source.SnoFillId, source.SnoOrderNumber,
            source.SnoOrderDt, source.BranchId, source.C, source.ProductDisplayName, source.ProductType,
            source.NorenTime, source.ExchangeTime, source.AlgoId, source.ExchangeOrderNumber, source.amo, source.MarketProtectionPercentage
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
