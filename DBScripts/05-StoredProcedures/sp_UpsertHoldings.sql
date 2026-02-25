IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_UpsertHoldings]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_UpsertHoldings]
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_UpsertHoldings]
(
    @tvpData [dbo].[THoldings] READONLY
)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION
     
            MERGE dbo.Holdings AS TARGET
            USING @tvpData AS SRC
                ON ((TARGET.Token1 = SRC.Token1 AND TARGET.Exchange1 = SRC.Exchange1 AND TARGET.TradingSymbol1 = SRC.TradingSymbol1) OR 
                    (TARGET.Token1 = SRC.Token2 AND TARGET.Exchange1 = SRC.Exchange2 AND TARGET.TradingSymbol1 = SRC.TradingSymbol2) OR
                    (TARGET.Token2 = SRC.Token2 AND TARGET.Exchange2 = SRC.Exchange2 AND TARGET.TradingSymbol2 = SRC.TradingSymbol2) OR
                    (TARGET.Token2 = SRC.Token1 AND TARGET.Exchange2 = SRC.Exchange1 AND TARGET.TradingSymbol2 = SRC.TradingSymbol1))
            WHEN MATCHED THEN
                UPDATE SET
                    TARGET.HoldingQuantity                        = SRC.HoldingQuantity,
                    TARGET.NonPoaDisplayQuantity                  = SRC.NonPoaDisplayQuantity,
                    TARGET.NonPoaDisplayT1Quantity                = SRC.NonPoaDisplayT1Quantity,
                    TARGET.BeneficiaryQuantity                    = SRC.BeneficiaryQuantity,
                    TARGET.BrokerEquityPledgedAsCollateralQuantity = SRC.BrokerEquityPledgedAsCollateralQuantity,
                    TARGET.BrokerForexPledgedAsCollateralQuantity  = SRC.BrokerForexPledgedAsCollateralQuantity,
                    TARGET.BrokerAllMarketPledgedAsCollateralQuantity = SRC.BrokerAllMarketPledgedAsCollateralQuantity,
                    TARGET.BrokerDerivativeMarketPledgedAsCollateralQuantity = SRC.BrokerDerivativeMarketPledgedAsCollateralQuantity,
                    TARGET.BuyTodaySellTommorrowQuantity          = SRC.BuyTodaySellTommorrowQuantity,
                    TARGET.HoldingQuantityUsedToday               = SRC.HoldingQuantityUsedToday,
                    TARGET.DpHoldingQuantity                      = SRC.DpHoldingQuantity,
                    TARGET.AvgPriceUploadedAlongWithHoldings      = SRC.AvgPriceUploadedAlongWithHoldings,
                    TARGET.HairCutPercOnPledgedSecurities         = SRC.HairCutPercOnPledgedSecurities,
                    TARGET.TodaySellAmount                        = SRC.TodaySellAmount,
                    TARGET.ProductDisplayName                     = SRC.ProductDisplayName,
                    TARGET.TradeQuantity                          = SRC.TradeQuantity,
                    TARGET.ProductType                            = SRC.ProductType,
                    TARGET.ExchangePendingInstructionDoneQuantity = SRC.ExchangePendingInstructionDoneQuantity,

                    TARGET.Exchange1                              = SRC.Exchange1,
                    TARGET.TradingSymbol1                         = SRC.TradingSymbol1,
                    TARGET.Token1                                 = SRC.Token1,
                    TARGET.Exchange2                              = SRC.Exchange2,
                    TARGET.TradingSymbol2                         = SRC.TradingSymbol2,
                    TARGET.Token2                                 = SRC.Token2,
                    Target.DailyClose                             = SRC.DailyClose,
                    TARGET.ModifiedAt                             = SYSUTCDATETIME()

            WHEN NOT MATCHED BY TARGET THEN
                INSERT (
                    HoldingQuantity,
                    NonPoaDisplayQuantity,
                    NonPoaDisplayT1Quantity,
                    BeneficiaryQuantity,
                    BrokerEquityPledgedAsCollateralQuantity,
                    BrokerForexPledgedAsCollateralQuantity,
                    BrokerAllMarketPledgedAsCollateralQuantity,
                    BrokerDerivativeMarketPledgedAsCollateralQuantity,
                    BuyTodaySellTommorrowQuantity,
                    HoldingQuantityUsedToday,
                    DpHoldingQuantity,
                    AvgPriceUploadedAlongWithHoldings,
                    HairCutPercOnPledgedSecurities,
                    TodaySellAmount,
                    ProductDisplayName, 
                    TradeQuantity,
                    ProductType,
                    ExchangePendingInstructionDoneQuantity,

                    Exchange1, TradingSymbol1, Token1,
                    Exchange2, TradingSymbol2, Token2,
                    DailyClose,
                    ModifiedAt
                )
                VALUES (
                    SRC.HoldingQuantity,
                    SRC.NonPoaDisplayQuantity,
                    SRC.NonPoaDisplayT1Quantity,
                    SRC.BeneficiaryQuantity,
                    SRC.BrokerEquityPledgedAsCollateralQuantity,
                    SRC.BrokerForexPledgedAsCollateralQuantity,
                    SRC.BrokerAllMarketPledgedAsCollateralQuantity,
                    SRC.BrokerDerivativeMarketPledgedAsCollateralQuantity,
                    SRC.BuyTodaySellTommorrowQuantity,
                    SRC.HoldingQuantityUsedToday,
                    SRC.DpHoldingQuantity,
                    SRC.AvgPriceUploadedAlongWithHoldings,
                    SRC.HairCutPercOnPledgedSecurities,
                    SRC.TodaySellAmount,
                    SRC.ProductDisplayName,
                    SRC.TradeQuantity,
                    SRC.ProductType,
                    SRC.ExchangePendingInstructionDoneQuantity,
                    SRC.Exchange1, SRC.TradingSymbol1, SRC.Token1,
                    SRC.Exchange2, SRC.TradingSymbol2, SRC.Token2,
                    SRC.DailyClose,
                    SYSUTCDATETIME()
                );

        IF (XACT_STATE()) <> -1
           COMMIT TRANSACTION

    END TRY

    BEGIN CATCH
        IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

        DECLARE @ErrorNumber INT = ERROR_NUMBER()
        BEGIN
           declare @message nvarchar(max) = N'An error occurred while merging [Holdings]. ' + ERROR_MESSAGE()
           ; THROW 51000, @message, 1
        END
    END CATCH
END
GO