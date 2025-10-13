IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_GeneratePAndLSummary]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_GeneratePAndLSummary]
GO

CREATE OR ALTER PROCEDURE sp_GeneratePAndLSummary
    @orderDate Date = null -- '09-25-2025'
AS
BEGIN
    SET NOCOUNT ON

    ;WITH BaseData AS
     (
        SELECT
            o.Token,
            o.TradingSymbol,
            CAST(o.NorenTime AS date) AS OrderDate,
            t.TransactionType,
            CAST(t.FillQuantity AS int) AS FillQuantity,
            t.FillPrice AS FillTradePrice,
            CASE WHEN o.ProductType != 'Delivery' THEN 'Intraday' ELSE o.ProductType END AS ProductType
        FROM orderbook o
        LEFT JOIN TradeBook t
            ON o.NorenOrderNumber = t.NorenOrderNumber
        WHERE o.OrderStatus = 'Completed'
    ),
    TradeSummary AS (
    SELECT 
        Token,
        OrderDate AS SummaryDate,
        TradingSymbol,

        --Intraday
        ISNULL(SUM(CASE WHEN TransactionType ='Buy' AND ProductType != 'Delivery' THEN 1 END),0) AS BuyIntradayTradeCount,
        ISNULL(SUM(CASE WHEN TransactionType ='Sell' AND ProductType != 'Delivery' THEN 1 END),0) AS SellIntradayTradeCount,

        ISNULL(SUM(CASE WHEN TransactionType ='Buy'  AND ProductType != 'Delivery' THEN FillQuantity END),0) AS BuyIntradayTradeQuantity,
        ISNULL(SUM(CASE WHEN TransactionType ='Sell' AND ProductType != 'Delivery' THEN FillQuantity END),0) AS SellIntradayTradeQuantity,

        ISNULL(SUM(CASE WHEN TransactionType ='Buy' AND ProductType != 'Delivery' THEN FillQuantity END),0) - 
        ISNULL(SUM(CASE WHEN TransactionType ='Sell'  AND ProductType != 'Delivery' THEN FillQuantity END),0) AS NetIntradayPositionQuantity,

        ISNULL(SUM(CASE WHEN TransactionType ='Buy'  AND ProductType != 'Delivery' THEN FillTradePrice * FillQuantity END),0)/
        ISNULL(SUM(CASE WHEN TransactionType ='Buy'  AND ProductType != 'Delivery' THEN FillQuantity END),1) AS BuyIntradayAverageTradePrice,
        ISNULL(SUM(CASE WHEN TransactionType ='Sell' AND ProductType != 'Delivery' THEN FillTradePrice * FillQuantity END),0)/
        ISNULL(SUM(CASE WHEN TransactionType ='Sell' AND ProductType != 'Delivery' THEN FillQuantity END),1)  AS SellIntradayAverageTradePrice,

        -- Delivery
        ISNULL(SUM(CASE WHEN TransactionType ='Buy' AND ProductType = 'Delivery' THEN 1 ELSE  0  END),0) AS BuyDeliveryTradeCount,
        ISNULL(SUM(CASE WHEN TransactionType ='Sell' AND ProductType = 'Delivery' THEN 1 ELSE  0 END),0) AS SellDeliveryTradeCount,

        ISNULL(SUM(CASE WHEN TransactionType ='Buy'  AND ProductType = 'Delivery' THEN FillQuantity ELSE  0 END),0) AS BuyDeliveryTradeQuantity,
        ISNULL(SUM(CASE WHEN TransactionType ='Sell' AND ProductType = 'Delivery' THEN FillQuantity ELSE  0 END),0) AS SellDeliveryTradeQuantity,

        ISNULL(SUM(CASE WHEN TransactionType ='Buy' AND ProductType = 'Delivery' THEN FillQuantity ELSE  0  END),0) - 
        ISNULL(SUM(CASE WHEN TransactionType ='Sell'  AND ProductType = 'Delivery' THEN FillQuantity ELSE  0 END),0) AS NetDeliveryPositionQuantity,

        ISNULL(SUM(CASE WHEN TransactionType ='Buy'  AND ProductType = 'Delivery' THEN FillTradePrice * FillQuantity  END),0)/
        ISNULL(SUM(CASE WHEN TransactionType ='Buy'  AND ProductType = 'Delivery' THEN FillQuantity  END),1)  AS BuyDeliveryAverageTradePrice,
        ISNULL(SUM(CASE WHEN TransactionType ='Sell' AND ProductType = 'Delivery' THEN FillTradePrice * FillQuantity  END),0)/
        ISNULL(SUM(CASE WHEN TransactionType ='Sell' AND ProductType = 'Delivery' THEN FillQuantity  END),1)  AS SellDeliveryAverageTradePrice
    FROM BaseData
    GROUP BY OrderDate, Token, TradingSymbol
    ),
    TradeQuantity AS  (
    SELECT 
        ISNULL(CASE WHEN NetIntradayPositionQuantity != 0 THEN
                                                    CASE WHEN BuyIntradayTradeQuantity > SellIntradayTradeQuantity THEN
                                                              BuyIntradayTradeQuantity - Abs(NetIntradayPositionQuantity)
                                                        WHEN BuyIntradayTradeQuantity < SellIntradayTradeQuantity THEN
                                                             SellIntradayTradeQuantity - Abs(NetIntradayPositionQuantity)
                                                    END
                                            ELSE
                                                SellIntradayTradeQuantity
                                            END, 0) AS NetIntradayTradeQuantity,

        ISNULL(CASE WHEN NetDeliveryPositionQuantity != 0 THEN
                                                    CASE WHEN BuyDeliveryTradeQuantity > SellDeliveryTradeQuantity THEN
                                                              BuyDeliveryTradeQuantity - Abs(NetDeliveryPositionQuantity)
                                                        WHEN BuyDeliveryTradeQuantity < SellDeliveryTradeQuantity  THEN
                                                             SellDeliveryTradeQuantity - Abs(NetDeliveryPositionQuantity)
                                                    END
                                            ELSE
                                                SellDeliveryTradeQuantity
                                            END, 0) AS NetDeliveryConvertedToIntradayTradeQuantity, *

      FROM TradeSummary
    ),
    PAndL1 AS (
    SELECT  
        NetIntradayTradeQuantity * BuyIntradayAverageTradePrice AS BuyIntradayTradeValue,
        NetIntradayTradeQuantity * SellIntradayAverageTradePrice AS SellIntradayTradeValue,

        NetDeliveryConvertedToIntradayTradeQuantity * BuyDeliveryAverageTradePrice AS BuyDeliveryConvertedToIntradayTradeValue,
        NetDeliveryConvertedToIntradayTradeQuantity * SellDeliveryAverageTradePrice AS SellDeliveryConvertedToIntradayTradeValue,
        
        case when NetIntradayPositionQuantity > 0 THEN  NetIntradayPositionQuantity * BuyIntradayAverageTradePrice
             ELSE NetIntradayPositionQuantity * SellIntradayAverageTradePrice
        END AS NetIntradayPositionTradeValue,

        case when NetDeliveryPositionQuantity > 0 THEN  NetDeliveryPositionQuantity * BuyDeliveryAverageTradePrice
             ELSE NetDeliveryPositionQuantity * SellDeliveryAverageTradePrice
        END AS NetDeliveryPositionTradeValue,
       *
    FROM TradeQuantity
    ),
    PandL2 AS
    (
    SELECT 
            SellIntradayTradeValue - BuyIntradayTradeValue +  
            SellDeliveryConvertedToIntradayTradeValue - BuyDeliveryConvertedToIntradayTradeValue AS NetIntradayPAndL,
            NetIntradayPositionTradeValue +  NetDeliveryPositionTradeValue AS   NetPositionValue,
            *
    FROM PAndL1
    )
    MERGE dbo.PAndLSummary AS T
    USING PAndL2 AS S
    ON T.SummaryDate = S.SummaryDate
       AND T.TradingSymbol = S.TradingSymbol
       AND T.Token =S.Token
    WHEN MATCHED THEN
        UPDATE SET
            T.NetIntradayPAndL = round(S.NetIntradayPAndL,2),
            T.NetPositionValue = round(S.NetPositionValue,2),
            T.BuyIntradayTradeValue = round(S.BuyIntradayTradeValue,2),
            T.SellIntradayTradeValue = round(S.SellIntradayTradeValue,2),
            T.BuyDeliveryConvertedToIntradayTradeValue = round(S.BuyDeliveryConvertedToIntradayTradeValue,2),
            T.SellDeliveryConvertedToIntradayTradeValue = round(S.SellDeliveryConvertedToIntradayTradeValue,2),
            T.NetIntradayPositionTradeValue = round(S.NetIntradayPositionTradeValue,2),
            T.NetDeliveryPositionTradeValue = round(S.NetDeliveryPositionTradeValue,2),
            T.NetIntradayTradeQuantity = S.NetIntradayTradeQuantity,
            T.NetDeliveryConvertedToIntradayTradeQuantity = S.NetDeliveryConvertedToIntradayTradeQuantity,
            T.BuyIntradayTradeCount = S.BuyIntradayTradeCount,
            T.SellIntradayTradeCount = S.SellIntradayTradeCount,
            T.BuyIntradayTradeQuantity = S.BuyIntradayTradeQuantity,
            T.SellIntradayTradeQuantity = S.SellIntradayTradeQuantity,
            T.NetIntradayPositionQuantity = S.NetIntradayPositionQuantity,
            T.BuyIntradayAverageTradePrice = round(S.BuyIntradayAverageTradePrice,2),
            T.SellIntradayAverageTradePrice = round(S.SellIntradayAverageTradePrice,2),
            T.BuyDeliveryTradeCount = S.BuyDeliveryTradeCount,
            T.SellDeliveryTradeCount = S.SellDeliveryTradeCount,
            T.BuyDeliveryTradeQuantity = S.BuyDeliveryTradeQuantity,
            T.SellDeliveryTradeQuantity = S.SellDeliveryTradeQuantity,
            T.NetDeliveryPositionQuantity = S.NetDeliveryPositionQuantity,
            T.BuyDeliveryAverageTradePrice = round(S.BuyDeliveryAverageTradePrice,2),
            T.SellDeliveryAverageTradePrice = round(S.SellDeliveryAverageTradePrice,2)
    WHEN NOT MATCHED BY TARGET THEN
        INSERT (NetIntradayPAndL, NetPositionValue, BuyIntradayTradeValue, SellIntradayTradeValue,
                BuyDeliveryConvertedToIntradayTradeValue, SellDeliveryConvertedToIntradayTradeValue,
                NetIntradayPositionTradeValue, NetDeliveryPositionTradeValue,
                NetIntradayTradeQuantity, NetDeliveryConvertedToIntradayTradeQuantity, Token,
                SummaryDate, TradingSymbol,
                BuyIntradayTradeCount, SellIntradayTradeCount,
                BuyIntradayTradeQuantity, SellIntradayTradeQuantity, NetIntradayPositionQuantity,
                BuyIntradayAverageTradePrice, SellIntradayAverageTradePrice,
                BuyDeliveryTradeCount, SellDeliveryTradeCount,
                BuyDeliveryTradeQuantity, SellDeliveryTradeQuantity,
                NetDeliveryPositionQuantity, BuyDeliveryAverageTradePrice, SellDeliveryAverageTradePrice)
        VALUES (round(S.NetIntradayPAndL,2), round(S.NetPositionValue,2), round(S.BuyIntradayTradeValue,2), round(S.SellIntradayTradeValue,2),
                round(S.BuyDeliveryConvertedToIntradayTradeValue,2), round(S.SellDeliveryConvertedToIntradayTradeValue,2),
                round(S.NetIntradayPositionTradeValue,2), round(S.NetDeliveryPositionTradeValue,2),
                S.NetIntradayTradeQuantity, S.NetDeliveryConvertedToIntradayTradeQuantity, S.Token,
                S.SummaryDate, S.TradingSymbol,
                S.BuyIntradayTradeCount, S.SellIntradayTradeCount,
                S.BuyIntradayTradeQuantity, S.SellIntradayTradeQuantity, S.NetIntradayPositionQuantity,
                round(S.BuyIntradayAverageTradePrice,2), round(S.SellIntradayAverageTradePrice,2),
                S.BuyDeliveryTradeCount, S.SellDeliveryTradeCount,
                S.BuyDeliveryTradeQuantity, S.SellDeliveryTradeQuantity,
                S.NetDeliveryPositionQuantity, round(S.BuyDeliveryAverageTradePrice,2), round(S.SellDeliveryAverageTradePrice,2));

END
GO

