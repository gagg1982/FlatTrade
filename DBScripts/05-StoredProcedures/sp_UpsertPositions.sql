IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_UpsertPositions]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_UpsertPositions]
GO

CREATE OR ALTER PROCEDURE dbo.sp_UpsertPositions
    @tvpData dbo.TPositions READONLY
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION
               
        MERGE dbo.Positions AS TARGET
            USING @tvpData AS Source
                ON TARGET.Token = Source.Token AND TARGET.Exchange = Source.Exchange AND Target.ProductDisplayName = Source.ProductDisplayName
                AND cast(TARGET.CreatedAt as date) = cast( Source.CreatedAt as date)
             WHEN MATCHED THEN
                UPDATE SET
                    Target.UserId = Source.UserId,
                    Target.ProductType = Source.ProductType,
                    Target.NetPositionQuantity = Source.NetPositionQuantity,
                    Target.NetAveragePositionPrice = Source.NetAveragePositionPrice,
                    Target.DayBuyQuantity = Source.DayBuyQuantity,
                    Target.DaySellQuantity = Source.DaySellQuantity,
                    Target.DayAveragePrice = Source.DayAveragePrice,
                    Target.DayAverageBuyPrice = Source.DayAverageBuyPrice,
                    Target.DayAverageSellPrice = Source.DayAverageSellPrice,
                    Target.FreezeQuantity = Source.FreezeQuantity,
                    Target.CompanyName = Source.CompanyName,
                    Target.DayBuyAmount = Source.DayBuyAmount,
                    Target.DaySellAmount = Source.DaySellAmount,
                    Target.CarrfyFwdBuyQuantity = Source.CarrfyFwdBuyQuantity,
                    Target.CarryFwdSellQuantity = Source.CarryFwdSellQuantity,
                    Target.CarryFwdOriginalAveragePrice = Source.CarryFwdOriginalAveragePrice,
                    Target.CarryFwdAverageBuyPrice = Source.CarryFwdAverageBuyPrice,
                    Target.CarryFwdAverageSellPrice = Source.CarryFwdAverageSellPrice,
                    Target.CarryFwdBuyAmount = Source.CarryFwdBuyAmount,
                    Target.CarryFwdSellAmount = Source.CarryFwdSellAmount,
                    Target.TotalBuyAmount = Source.TotalBuyAmount,
                    Target.TotalSellAmount = Source.TotalSellAmount,
                    Target.TotalBuyAveragePrice = Source.TotalBuyAveragePrice,
                    Target.TotalSellAveragePrice = Source.TotalSellAveragePrice,
                    Target.LastTradePrice = Source.LastTradePrice,
                    Target.RealizedPNl = Source.RealizedPNl,
                    Target.UnRealizedMTM = Source.UnRealizedMTM,
                    Target.BreakEvenPrice = Source.BreakEvenPrice,
                    Target.AvgPriceUploadedAlongWithHoldings = Source.AvgPriceUploadedAlongWithHoldings,
                    Target.NetAvgPriceUploadedAlongWithHoldings = Source.NetAvgPriceUploadedAlongWithHoldings,
                    Target.OpenBuyQuantity = Source.OpenBuyQuantity,
                    Target.OpenSellQuantity = Source.OpenSellQuantity,
                    Target.OpenBuyAmount = Source.OpenBuyAmount,
                    Target.OpenSellAmount = Source.OpenSellAmount,
                    Target.OpenAverageBuyPrice = Source.OpenAverageBuyPrice,
                    Target.OpenAverageSellPrice = Source.OpenAverageSellPrice,
                    Target.Multiplier = Source.Multiplier,
                    Target.PricePrecision = Source.PricePrecision,
                    Target.TickSize = Source.TickSize,
                    Target.LotSize = Source.LotSize,
                    Target.PriceFactor = Source.PriceFactor,
                    Target.InstrumentName = Source.InstrumentName
            WHEN NOT MATCHED BY TARGET THEN
                INSERT (
                    Token, Exchange, TradingSymbol, AccountId, UserId, ProductDisplayName, ProductType,
                    NetPositionQuantity, NetAveragePositionPrice,
                    DayBuyQuantity, DaySellQuantity, DayAveragePrice, DayAverageBuyPrice, DayAverageSellPrice, FreezeQuantity, CompanyName, DayBuyAmount, DaySellAmount,
                    CarrfyFwdBuyQuantity, CarryFwdSellQuantity, CarryFwdOriginalAveragePrice, CarryFwdAverageBuyPrice, CarryFwdAverageSellPrice, CarryFwdBuyAmount, CarryFwdSellAmount,
                    TotalBuyAmount, TotalSellAmount,
                    TotalBuyAveragePrice, TotalSellAveragePrice, LastTradePrice, RealizedPNl, UnRealizedMTM, BreakEvenPrice,
                    AvgPriceUploadedAlongWithHoldings, NetAvgPriceUploadedAlongWithHoldings,
                    OpenBuyQuantity, OpenSellQuantity, OpenBuyAmount, OpenSellAmount, OpenAverageBuyPrice, OpenAverageSellPrice,
                    Multiplier,
                    PricePrecision, TickSize, LotSize, PriceFactor, InstrumentName, CreatedAt
                )
                VALUES (
                    Source.Token, Source.Exchange, Source.TradingSymbol, Source.AccountId, Source.UserId, Source.ProductDisplayName, Source.ProductType,
                    Source.NetPositionQuantity, Source.NetAveragePositionPrice,
                    Source.DayBuyQuantity, Source.DaySellQuantity, Source.DayAveragePrice, Source.DayAverageBuyPrice, Source.DayAverageSellPrice, Source.FreezeQuantity, Source.CompanyName, Source.DayBuyAmount, Source.DaySellAmount,
                    Source.CarrfyFwdBuyQuantity, Source.CarryFwdSellQuantity, Source.CarryFwdOriginalAveragePrice, Source.CarryFwdAverageBuyPrice, Source.CarryFwdAverageSellPrice, Source.CarryFwdBuyAmount, Source.CarryFwdSellAmount,
                    Source.TotalBuyAmount, Source.TotalSellAmount,
                    Source.TotalBuyAveragePrice, Source.TotalSellAveragePrice, Source.LastTradePrice, Source.RealizedPNl, Source.UnRealizedMTM, Source.BreakEvenPrice,
                    Source.AvgPriceUploadedAlongWithHoldings, Source.NetAvgPriceUploadedAlongWithHoldings,
                    Source.OpenBuyQuantity, Source.OpenSellQuantity, Source.OpenBuyAmount, Source.OpenSellAmount, Source.OpenAverageBuyPrice,
                    Source.OpenAverageSellPrice,
                    Source.Multiplier, 
                    Source.PricePrecision, Source.TickSize, Source.LotSize, Source.PriceFactor, Source.InstrumentName, SysUtcDateTime()
                );

        IF (XACT_STATE()) <> -1
           COMMIT TRANSACTION

    END TRY

    BEGIN CATCH
        IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

        DECLARE @ErrorNumber INT = ERROR_NUMBER()
        BEGIN
           declare @message nvarchar(max) = N'An error occurred while merging [Positions]. ' + ERROR_MESSAGE()
           ; THROW 51000, @message, 1
        END
    END CATCH
END;
GO
