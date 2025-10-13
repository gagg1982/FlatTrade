IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_UpsertStockInstruments]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_UpsertStockInstruments]
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_UpsertStockInstruments]
(
    @tvpData [dbo].[TStockInstruments] READONLY
)
AS
BEGIN
    SET NOCOUNT ON;

_START:
    BEGIN TRY
        BEGIN TRANSACTION
     
            MERGE INTO dbo.StockInstruments AS Target
            USING @tvpData AS Source
            ON Target.Token = Source.Token AND Target.ExchangeCode = Source.ExchangeCode
            WHEN MATCHED THEN
                UPDATE SET Target.SymbolName = Source.SymbolName,
                           Target.TradingSymbol = Source.TradingSymbol,
                           Target.CompanyName = Source.CompanyName,
                           Target.InstrumentName = Source.InstrumentName,
                           Target.Isin = Source.Isin,
                           Target.TickSize = Source.TickSize,
                           Target.PricePrecision = Source.PricePrecision,
                           Target.LotSize = Source.LotSize,
                           Target.UpperCircuit = Source.UpperCircuit,
                           Target.LowerCircuit = Source.LowerCircuit,
                           Target.LastTradeDateTime = Source.LastTradeDateTime,
                           Target.LastUpdateTime = Source.LastUpdateTime,
                           Target.LastTradePrice = Source.LastTradePrice,
                           Target.AverageTradePrice = Source.AverageTradePrice,
                           Target.Wk52High = Source.Wk52High,
                           Target.Wk52Low = Source.Wk52Low,
                           Target.IssueCapital = Source.IssueCapital,
                           Target.IssueDate = Source.IssueDate,
                           Target.ListingDate = Source.ListingDate,
                           Target.FreezeQuantity = Source.FreezeQuantity,
                           Target.IsFutureAllowed = source.IsFutureAllowed,
                           Target.IsOptionAllowed = source.IsOptionAllowed,
                           Target.[Active] = 1,
                           Target.[LastUpdatedInDbAt] = GetDate()
            
            WHEN NOT MATCHED BY SOURCE AND Target.[Active] != 0 THEN
                 UPDATE SET Target.[Active] = 0,
                            Target.[LastUpdatedInDbAt] = GetDate()
            WHEN NOT MATCHED BY TARGET THEN
                INSERT (Token, SymbolName, TradingSymbol, CompanyName, ExchangeCode, Segment, InstrumentName, Isin, TickSize, PricePrecision, LotSize, UpperCircuit, LowerCircuit, LastTradeDateTime, LastUpdateTime, LastTradePrice, AverageTradePrice, IssueCapital, Wk52High, Wk52Low, IssueDate, ListingDate, FreezeQuantity, IsFutureAllowed, IsOptionAllowed, [Active], [LastUpdatedInDbAt], [CreatedInDbAt])
                VALUES (Source.Token, Source.SymbolName, Source.TradingSymbol, Source.CompanyName, Source.ExchangeCode, Source.Segment, Source.InstrumentName, Source.Isin, Source.TickSize, Source.PricePrecision, Source.LotSize, Source.UpperCircuit, Source.LowerCircuit,
                        Source.LastTradeDateTime,
                        Source.LastUpdateTime,
                        Source.LastTradePrice,
                        Source.AverageTradePrice,
                        Source.IssueCapital,
                        Source.Wk52High, Source.Wk52Low,
                        Source.IssueDate,
                        Source.ListingDate,
                        Source.FreezeQuantity,
                        source.IsFutureAllowed,
                        source.IsOptionAllowed,
                        1,
                        GetDate(), -- LastUpdatedInDbAt
                        GetDate()  -- CreatedInDbAt
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
END
GO
