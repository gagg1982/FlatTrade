IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_GenerateDailyPAndLSummary]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_GenerateDailyPAndLSummary]
GO

CREATE OR ALTER PROCEDURE sp_GenerateDailyPAndLSummary
    @orderDate Date = null -- '09-25-2025'
AS
BEGIN
    SET NOCOUNT ON
    
        ;WITH SymbolLevel AS
        (
            SELECT 
                s.SymbolName,
                CAST(t.FillDateTime AS date) AS TradeDate,

                SUM(CASE WHEN t.TransactionType = 'Buy'  THEN 1 ELSE 0 END) AS BuyCount,
                SUM(CASE WHEN t.TransactionType = 'Buy'  THEN t.FillQuantity ELSE 0 END) AS BuyQuantity,
                SUM(CASE WHEN t.TransactionType = 'Buy'  THEN TotalCharges ELSE 0 END) AS BuyCharges,

                SUM(CASE WHEN t.TransactionType = 'Sell' THEN 1 ELSE 0 END) AS SellCount,
                SUM(CASE WHEN t.TransactionType = 'Sell' THEN t.FillQuantity ELSE 0 END) AS SellQuantity,
                SUM(CASE WHEN t.TransactionType = 'Sell'  THEN TotalCharges ELSE 0 END) AS SellCharges,

                -- Buy Average Price
                CASE 
                    WHEN SUM(CASE WHEN t.TransactionType = 'Buy' THEN t.FillQuantity ELSE 0 END) = 0 
                    THEN 0
                    ELSE 
                        SUM(CASE WHEN t.TransactionType = 'Buy' THEN t.FillQuantity * t.FillPrice ELSE 0 END) * 1.0
                        / SUM(CASE WHEN t.TransactionType = 'Buy' THEN t.FillQuantity ELSE 0 END)
                END AS BuyAveragePrice,

                -- Sell Average Price
                CASE 
                    WHEN SUM(CASE WHEN t.TransactionType = 'Sell' THEN t.FillQuantity ELSE 0 END) = 0 
                    THEN 0
                    ELSE 
                        SUM(CASE WHEN t.TransactionType = 'Sell' THEN t.FillQuantity * t.FillPrice ELSE 0 END) * 1.0
                        / SUM(CASE WHEN t.TransactionType = 'Sell' THEN t.FillQuantity ELSE 0 END)
                END AS SellAveragePrice

            FROM TradeBook t
            LEFT JOIN StockInstruments s ON t.TradingSymbol = s.TradingSymbol AND t.Token = s.Token
            LEFT JOIN BrokerageAndTaxes b ON t.TradingSymbol = b.TradingSymbol AND b.FillId = t.FillId AND b.FillDateTime = t.FillDateTime
            WHERE (@orderDate is null  OR  CAST(t.NorenTime AS date) = @orderDate) 
            GROUP BY  
                s.SymbolName,
                CAST(t.FillDateTime AS date)
        ),
        Quantity AS 
        (
        SELECT 
            SymbolName, 
            TradeDate, 
            BuyCount, 
            BuyQuantity, 
            BuyCharges,
            SellCount, 
            SellQuantity, 
            SellCharges,
            BuyCharges + SellCharges As TotalCharges,
            BuyAveragePrice, 
            SellAveragePrice,

            -- Net IntraDay Quantity
            CASE 
                WHEN SellQuantity != 0 AND BuyQuantity != 0 AND BuyQuantity > SellQuantity 
                    THEN SellQuantity
                WHEN SellQuantity != 0 AND BuyQuantity != 0 AND BuyQuantity <= SellQuantity 
                    THEN BuyQuantity
                WHEN BuyQuantity = 0 
                    THEN 0
                WHEN SellQuantity = 0 
                    THEN 0
            END AS NetIntraDayQuantity,

            -- Net Delivery Quantity
            CASE 
                WHEN SellQuantity != 0 AND BuyQuantity != 0 AND BuyQuantity != SellQuantity 
                    THEN BuyQuantity - SellQuantity
                WHEN BuyQuantity = 0 
                    THEN -1 * SellQuantity
                WHEN SellQuantity = 0 
                    THEN BuyQuantity
                ELSE  0
            END AS NetDeliveryQuantity

        FROM SymbolLevel
        ), NetPAndL AS
        (
        SELECT  SymbolName, 
            TradeDate, 

            -- Gross IntraDay PAndL
            Round((NetIntraDayQuantity * SellAveragePrice) - (NetIntraDayQuantity * BuyAveragePrice),2) AS GrossIntraDayPAndL,

            -- Net IntraDay PAndL
            Round((NetIntraDayQuantity * SellAveragePrice) - (NetIntraDayQuantity * BuyAveragePrice) - TotalCharges,2) AS NetIntraDayPAndL,

            -- Net Delivery Value
            CASE WHEN NetDeliveryQuantity >= 0 THEN ROUND((NetDeliveryQuantity * BuyAveragePrice),2) 
            ELSE Round((NetDeliveryQuantity * SellAveragePrice),2) END AS NetDeliveryValue,

            TotalCharges, 
            BuyCount, 
            BuyQuantity, 
            BuyCharges,
            SellCount, 
            SellQuantity, 
            SellCharges,
            BuyAveragePrice, 
            SellAveragePrice,
            NetIntraDayQuantity,
            NetDeliveryQuantity,

            -- Net IntraDay BuyValue
            NetIntraDayQuantity * BuyAveragePrice AS NetIntraDayBuyValue,

            -- Net IntraDay SellValue
            NetIntraDayQuantity * SellAveragePrice AS NetIntraDaySellValue,

            -- Net Delivery BuyValue
            NetDeliveryQuantity * BuyAveragePrice AS NetDeliveryBuyValue,

            -- Net Delivery SellValue
            NetDeliveryQuantity * SellAveragePrice AS NetDeliverySellValue

        FROM Quantity
        )
        MERGE dbo.DailyPAndLSummary AS T
        USING NetPAndL AS S
        ON T.TradeDate = S.TradeDate
           AND T.SymbolName = S.SymbolName
        WHEN MATCHED THEN
            UPDATE SET
                T.BuyCharges = S.BuyCharges,
                T.SellCharges = S.SellCharges,
                T.TotalCharges = S.TotalCharges,
        
                T.GrossIntradayPAndL = S.GrossIntradayPAndL,
                T.NetIntradayPAndL = S.NetIntradayPAndL,
                T.NetDeliveryValue = S.NetDeliveryValue,
                T.BuyCount = S.BuyCount,
                T.BuyQuantity = S.BuyQuantity,
                T.SellCount = S.SellCount,
                T.SellQuantity = S.SellQuantity,
                T.BuyAveragePrice = S.BuyAveragePrice,
                T.SellAveragePrice = S.SellAveragePrice,
                T.NetIntraDayQuantity = S.NetIntraDayQuantity,
                T.NetDeliveryQuantity = S.NetDeliveryQuantity,
                T.NetIntraDayBuyValue = S.NetIntraDayBuyValue,
                T.NetIntraDaySellValue = S.NetIntraDaySellValue,
                T.NetDeliveryBuyValue = S.NetDeliveryBuyValue,
                T.NetDeliverySellValue = S.NetDeliverySellValue
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (SymbolName, TradeDate, NetIntradayPAndL,NetDeliveryValue,BuyCount, BuyQuantity,
                    SellCount,SellQuantity, BuyAveragePrice, SellAveragePrice, BuyCharges, SellCharges,
                    NetIntraDayQuantity,NetDeliveryQuantity,NetIntraDayBuyValue, TotalCharges,
                    NetIntraDaySellValue, NetDeliveryBuyValue, NetDeliverySellValue, GrossIntradayPAndL)
            VALUES (Symbolname, TradeDate, NetIntradayPAndL,NetDeliveryValue,BuyCount, BuyQuantity,
                    SellCount,SellQuantity, BuyAveragePrice, SellAveragePrice,BuyCharges, SellCharges,
                    NetIntraDayQuantity,NetDeliveryQuantity,NetIntraDayBuyValue, TotalCharges,
                    NetIntraDaySellValue, NetDeliveryBuyValue, NetDeliverySellValue, GrossIntradayPAndL);

END
GO

