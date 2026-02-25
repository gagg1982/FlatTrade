DROP TABLE IF EXISTS [dbo].[LatestNpsNav];
PRINT 'Table LatestNpsNav dropped if it existed.';
DROP TABLE IF EXISTS [dbo].[HistoricNpsNav];
PRINT 'Table HistoricNpsNav dropped if it existed.';
DROP TABLE IF EXISTS [dbo].[CorporateActions];
PRINT 'Table CorporateActions dropped if it existed.';
DROP TABLE IF EXISTS [dbo].[BrokerageAndTaxes];
PRINT 'Table BrokerageAndTaxes dropped if it existed.';
DROP TABLE IF EXISTS [dbo].[ExcludedStockInstruments];
PRINT 'Table ExcludedStockInstruments dropped if it existed.';
DROP TABLE IF EXISTS [dbo].[Positions];
PRINT 'Table Positions dropped if it existed.';
DROP TABLE IF EXISTS [dbo].[Holdings];
PRINT 'Table Holdings dropped if it existed.';
DROP TABLE IF EXISTS [dbo].[MarginEquity];
PRINT 'Table MarginEquity dropped if it existed.';
DROP TABLE IF EXISTS [dbo].[HolidayCalendar];
PRINT 'Table HolidayCalendar dropped if it existed.';
DROP TABLE IF EXISTS [dbo].[TradeBook];
PRINT 'Table TradeBook dropped if it existed.';
DROP TABLE IF EXISTS [dbo].[OrderBook];
PRINT 'Table OrderBook dropped if it existed.';
DROP TABLE IF EXISTS [dbo].[SingleOrderHistory];
PRINT 'Table SingleOrderHistory dropped if it existed.';
DROP TABLE IF EXISTS [dbo].[StocksOhlcv_1];
PRINT 'Table StocksOhlcv_1 dropped if it existed.';
DROP TABLE IF EXISTS [dbo].[StocksOhlcv_1440];
PRINT 'Table StocksOhlcv_1440 dropped if it existed.';
DROP TABLE IF EXISTS [dbo].[StockInstruments];
PRINT 'Table StockInstruments dropped if it existed.';
DROP TABLE IF EXISTS [dbo].[Exchange];
PRINT 'Table Exchange dropped if it existed.';

DROP TYPE IF EXISTS [dbo].[TLatestNpsNav];
PRINT 'Type TLatestNpsNav dropped if it existed.';
DROP TYPE IF EXISTS [dbo].[THistoricNpsNav];
PRINT 'Type THistoricNpsNav dropped if it existed.';
DROP TYPE IF EXISTS [dbo].[TCorporateActions];
PRINT 'Type TCorporateActions dropped if it existed.';
DROP TYPE IF EXISTS [dbo].[TBrokerageAndTaxes];
PRINT 'Type TBrokerageAndTaxes dropped if it existed.';
DROP TYPE IF EXISTS [dbo].[TPositions];
PRINT 'Type TPositions dropped if it existed.';
DROP TYPE IF EXISTS [dbo].[THoldings];
PRINT 'Type THoldings dropped if it existed.';
DROP TYPE IF EXISTS [dbo].[TMarginEquity];
PRINT 'Type TMarginEquity dropped if it existed.';

DROP TYPE IF EXISTS [dbo].[TTradeBook];
PRINT 'Type TTradeBook dropped if it existed.';
DROP TYPE IF EXISTS [dbo].[TOrderBook];
PRINT 'Type TOrderBook dropped if it existed.';
DROP TYPE IF EXISTS [dbo].[TSingleOrderHistory];
PRINT 'Type TSingleOrderHistory dropped if it existed.';
DROP TYPE IF EXISTS [dbo].[TExchanges];
PRINT 'Type TExchanges dropped if it existed.';
DROP TYPE IF EXISTS [dbo].[TStockInstruments];
PRINT 'Type TStockInstruments dropped if it existed.';
DROP TYPE IF EXISTS [dbo].[TStocksOhlcv];
PRINT 'Type TStocksOhlcv dropped if it existed.';

PRINT '==================All specified tables and types dropped successfully.======================';

--============================================================

--============================================================

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='BrokerageAndTaxes' AND xtype='U')
    BEGIN      
        
       CREATE TABLE dbo.BrokerageAndTaxes
        (
            TradingSymbol NVARCHAR(64) NOT NULL,
            Exchange NVARCHAR(16) NOT NULL,
            BrokerageAmount DECIMAL(18, 2) NULL,
            ClearingMemberAmount DECIMAL(18, 2) NULL,
            ExchangeOrderNumber NVARCHAR(64) NULL,
            FillDateTime DATETIME2 NOT NULL,
            FillId NVARCHAR(64) NOT NULL,
            FillPrice DECIMAL(18, 4) NOT NULL,
            FillQuantity DECIMAL(18, 4) NOT NULL,
            Gst DECIMAL(18, 2) NULL,
            InvestorProtectionFundTrustAmount DECIMAL(18, 2) NULL,
            NorenOrderNumber BIGINT NULL,
            SnoOrderNumber BIGINT NULL,
            NorenTime DATETIME2 NULL,
            ProductType NVARCHAR(32) NULL,
            Remarks NVARCHAR(256) NULL,
            SebiCharges DECIMAL(18, 2) NULL,
            ExchangeCharges DECIMAL(18, 2) NULL,
            SecurityTransactionTax DECIMAL(18, 2) NULL,
            StampDuty DECIMAL(18, 2) NULL,
            Token BIGINT NOT NULL,
            TotalCharges DECIMAL(18, 2) NULL,
            TransactionType NVARCHAR(32) NULL,
            Url NVARCHAR(256) NULL,
            LastModifiedAt dateTime2 Not NULL,
            CONSTRAINT PK_BrokerageAndTaxes PRIMARY KEY (Token, FillId, Exchange)
        );

        PRINT 'Table BrokerageAndTaxes created.';
    END
    ELSE
    BEGIN
        PRINT 'Table BrokerageAndTaxes already exists.';
    END

GO;
--============================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='LatestNpsNav' AND xtype='U')
    BEGIN      
        
       CREATE TABLE dbo.LatestNpsNav
        (
            PFMCode         VARCHAR(20)     NOT NULL,
            PFMName         VARCHAR(200)    NOT NULL,
            SchemeCode      VARCHAR(20)     NOT NULL,
            SchemeName      VARCHAR(200)    NOT NULL,
            NavDate         DATE            NOT NULL,
            NAV             DECIMAL(18,6)   NOT NULL,
            Return_1D       DECIMAL(10,4)   NULL,
            Return_7D       DECIMAL(10,4)   NULL,
            Return_1M       DECIMAL(10,4)   NULL,
            Return_3M       DECIMAL(10,4)   NULL,
            Return_6M       DECIMAL(10,4)   NULL,
            Return_1Y       DECIMAL(10,4)   NULL,
            Return_3Y       DECIMAL(10,4)   NULL,
            Return_5Y       DECIMAL(10,4)   NULL,

            CreatedAt       DATETIME       NOT NULL DEFAULT SYSUTCDATETIME(),
            LastUpdated     DATETIME       NULL

            CONSTRAINT PK_LatestNpsNav 
                PRIMARY KEY CLUSTERED (PFMCode, SchemeCode, NavDate)
        );

        CREATE NONCLUSTERED INDEX IX_LatestNpsNav_PFMCode
        ON dbo.LatestNpsNav (PFMCode);

        CREATE NONCLUSTERED INDEX IX_LatestNpsNav_Return1D
        ON dbo.LatestNpsNav (Return_1D);

        CREATE NONCLUSTERED INDEX IX_LatestNpsNav_Return7D
        ON dbo.LatestNpsNav (Return_7D);

        CREATE NONCLUSTERED INDEX IX_LatestNpsNav_Return1M
        ON dbo.LatestNpsNav (Return_1M);

        CREATE NONCLUSTERED INDEX IX_LatestNpsNav_Return3M
        ON dbo.LatestNpsNav (Return_3M);

        CREATE NONCLUSTERED INDEX IX_LatestNpsNav_Return6M
        ON dbo.LatestNpsNav (Return_6M);

        CREATE NONCLUSTERED INDEX IX_LatestNpsNav_Return1Y
        ON dbo.LatestNpsNav (Return_1Y);

        CREATE NONCLUSTERED INDEX IX_LatestNpsNav_Return3Y
        ON dbo.LatestNpsNav (Return_3Y);

        CREATE NONCLUSTERED INDEX IX_LatestNpsNav_Return5Y
        ON dbo.LatestNpsNav (Return_5Y);

        CREATE NONCLUSTERED INDEX IX_NpsLatestNav_SchemeName
        ON dbo.LatestNpsNav (SchemeName);
        
        PRINT 'Table LatestNpsNav created.';
    END
    ELSE
    BEGIN
        PRINT 'Table LatestNpsNav already exists.';
    END

GO;

--============================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='HistoricNpsNav' AND xtype='U')
    BEGIN      
        
       CREATE TABLE dbo.HistoricNpsNav
        (
            scheme_code   VARCHAR(20)  NOT NULL,
            scheme_name   VARCHAR(200) NOT NULL,
            nav           DECIMAL(18,6) NOT NULL,
            nav_date      DATE         NOT NULL,
            created_at    DATETIME2(3) NOT NULL DEFAULT SYSUTCDATETIME(),
            updated_at    DATETIME2(3) NULL,

            CONSTRAINT PK_HistoricNpsNav 
                PRIMARY KEY CLUSTERED (scheme_code, nav_date)
        );


        CREATE NONCLUSTERED INDEX IX_HistoricNpsNav_Scheme_Historic
        ON dbo.HistoricNpsNav (scheme_code, nav_date DESC)
        INCLUDE (nav, scheme_name);

        CREATE NONCLUSTERED INDEX IX_HistoricNpsNav_NavDate
        ON dbo.HistoricNpsNav (nav_date)
        INCLUDE (scheme_code, nav, scheme_name);
        
        PRINT 'Table HistoricNpsNav created.';
    END
    ELSE
    BEGIN
        PRINT 'Table HistoricNpsNav already exists.';
    END

GO;

--============================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='CorporateActions' AND xtype='U')
    BEGIN      
        
       CREATE TABLE dbo.CorporateActions
        (
            Id                  INT IDENTITY(1,1) PRIMARY KEY,
            Symbol              NVARCHAR(20)      NOT NULL,
	        Exchange            NVARCHAR(10)      NOT NULL,
	        Series              NVARCHAR(20)      NOT NULL,
	        Indicative   		NVARCHAR(10)      NULL,
	        FaceValue			DECIMAL(5,2)	  NOT NULL,
            Subject             NVARCHAR(200)     NULL,
            ExDate              DATE              NULL,
            RecordDate          DATE              NULL,
	        BookClosureStartDate          DATE              NULL,
	        BookClosureEndDate            DATE              NULL,
	        NoDeliveryStartDate      DATE              NULL,
	        NoDeliveryEndDate        DATE              NULL,
	        CompanyName         NVARCHAR(200)     NOT NULL,
            Isin                NVARCHAR(32)      NOT NULL,
	        AnnouncementDate        DATE          NOT NULL,
            CreatedAt           DATETIME          DEFAULT GETDATE(),
            UpdatedAt           DATETIME          DEFAULT GETDATE()
        );

        CREATE UNIQUE INDEX IX_CorporateActions_Symbol_Exchange_Subject_ExDate
        ON dbo.CorporateActions(Symbol, Exchange, Subject, ExDate);
        
        PRINT 'Table CorporateActions created.';
    END
    ELSE
    BEGIN
        PRINT 'Table CorporateActions already exists.';
    END

GO;
--============================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ExcludedStockInstruments' AND xtype='U')
    BEGIN      
        
        CREATE TABLE dbo.ExcludedStockInstruments
        (
            Token INT NOT NULL,
            TradingSymbol NVARCHAR(64) NOT NULL,
            Exchange NVARCHAR(64) NOT NULL
        );
        CREATE UNIQUE INDEX IX_ExcludedStockInstruments_TradingSymbolExchange 
        ON dbo.ExcludedStockInstruments(TradingSymbol, Exchange);

        PRINT 'Table ExcludedStockInstruments created.';
    END
    ELSE
    BEGIN
        PRINT 'Table ExcludedStockInstruments already exists.';
    END

GO;
--============================================================

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Postions' AND xtype='U')
    BEGIN      
        
        CREATE TABLE dbo.Positions
        (
            Token                       INT,
            Exchange                    VARCHAR(10),
            SymbolName                  VARCHAR(50),
            TradingSymbol               VARCHAR(50),
            AccountId                   VARCHAR(20),
            UserId                      VARCHAR(20),
            ProductDisplayName          VARCHAR(50),
            ProductType                 VARCHAR(20),

            NetPositionQuantity         INT,
            NetAveragePositionPrice     DECIMAL(18,4),

            DayBuyQuantity              INT,
            DaySellQuantity             INT,
            DayAveragePrice             DECIMAL(18,4),
            DayAverageBuyPrice          DECIMAL(18,4),
            DayAverageSellPrice         DECIMAL(18,4),
            FreezeQuantity              INT,
            CompanyName                 VARCHAR(64),
            DayBuyAmount                DECIMAL(18,4),
            DaySellAmount               DECIMAL(18,4),

            CarrfyFwdBuyQuantity        INT,
            CarryFwdSellQuantity        INT,
            CarryFwdOriginalAveragePrice DECIMAL(18,4),
            CarryFwdAverageBuyPrice     DECIMAL(18,4),
            CarryFwdAverageSellPrice    DECIMAL(18,4),
            CarryFwdBuyAmount           DECIMAL(18,4),
            CarryFwdSellAmount          DECIMAL(18,4),

            TotalBuyAmount              DECIMAL(18,4),
            TotalSellAmount             DECIMAL(18,4),

            TotalBuyAveragePrice        DECIMAL(18,4),
            TotalSellAveragePrice       DECIMAL(18,4),
            LastTradePrice              DECIMAL(18,4),
            RealizedPNl                 DECIMAL(18,4),
            UnRealizedMTM               DECIMAL(18,4),
            BreakEvenPrice              DECIMAL(18,4),

            AvgPriceUploadedAlongWithHoldings      DECIMAL(18,4),
            NetAvgPriceUploadedAlongWithHoldings   DECIMAL(18,4),

            OpenBuyQuantity             INT,
            OpenSellQuantity            INT,
            OpenBuyAmount               DECIMAL(18,4),
            OpenSellAmount              DECIMAL(18,4),
            OpenAverageBuyPrice         DECIMAL(18,4),
            OpenAverageSellPrice        DECIMAL(18,4),

            Multiplier                  DECIMAL(18,4),
            PricePrecision              INT,
            TickSize                    DECIMAL(18,4),
            LotSize                     DECIMAL(18,4),
            PriceFactor                 VARCHAR(50),
            InstrumentName              VARCHAR(30),
            CreatedAt                   DATETIME2
        );

        PRINT 'Table Postions created.';
    END
    ELSE
    BEGIN
        PRINT 'Table Postions already exists.';
    END

GO;
--============================================================

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Holdings' AND xtype='U')
    BEGIN              
    CREATE TABLE dbo.Holdings
        (
            HoldingQuantity                        INT          NOT NULL,
            NonPoaDisplayQuantity                  INT          NULL,
            NonPoaDisplayT1Quantity                INT          NULL,
            BeneficiaryQuantity                    INT          NULL,
            BrokerEquityPledgedAsCollateralQuantity INT          NULL,
            BrokerForexPledgedAsCollateralQuantity INT          NULL,
            BrokerAllMarketPledgedAsCollateralQuantity INT       NULL,
            BrokerDerivativeMarketPledgedAsCollateralQuantity INT NULL,
            BuyTodaySellTommorrowQuantity          INT          NULL,
            HoldingQuantityUsedToday               INT          NULL,
            DpHoldingQuantity                      INT          NULL,
            AvgPriceUploadedAlongWithHoldings      DECIMAL(18,4)   NULL,
            HairCutPercOnPledgedSecurities         DECIMAL(18,4)   NULL,
            TodaySellAmount                        DECIMAL(18,4)   NULL,
            ProductDisplayName                     VARCHAR(20)     NULL,
            ProductType                            VARCHAR(10) NULL,
            TradeQuantity                          INT NULL,
            ExchangePendingInstructionDoneQuantity INT NULL,

            -- Flattened ExchangeSymbolResponse columns (example: up to 2)
            Exchange1                              VARCHAR(10)     NULL,
            TradingSymbol1                         VARCHAR(50)     NULL,
            Token1                                 INT          NULL,

            Exchange2                              VARCHAR(10)     NULL,
            TradingSymbol2                         VARCHAR(50)     NULL,
            Token2                                 INT          NULL,

            DailyClose                             Decimal(18,4) NULL,
            -- Optional: metadata columns
            ModifiedAt                              DATETIME2(3)    DEFAULT SYSUTCDATETIME()
        );
        PRINT 'Table Holdings created.';
    END
    ELSE
    BEGIN
        PRINT 'Table Holdings already exists.';
    END

GO;
--============================================================

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='MarginEquity' AND xtype='U')
    BEGIN      
        CREATE TABLE MarginEquity
        (
            Symbol NVARCHAR(50) NOT NULL,
            Segment NVARCHAR(20) NOT NULL,
            MISMarginInPercentage DECIMAL(10, 2) NOT NULL,
            CNCMarginMISMarginInPercentage DECIMAL(10, 2) NOT NULL,
            MISLeverageX DECIMAL(10, 2) NOT NULL,
            CNCLeverageX DECIMAL(10, 2) NOT NULL,
            CreatedInDbAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
            LastUpdatedInDbAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
        );
        PRINT 'Table MarginEquity created.';
    END
    ELSE
    BEGIN
        PRINT 'Table MarginEquity already exists.';
    END

GO;
--============================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='HolidayCalendar' AND xtype='U')
    BEGIN      
        CREATE TABLE HolidayCalendar
        (
            Id INT PRIMARY KEY,
            Exchange NVARCHAR(8) NOT NULL,
            HolidayDate DATE NOT NULL,
            [DayName] VARCHAR(20) NOT NULL,
            Description VARCHAR(200) NOT NULL
        );

        CREATE INDEX IX_HolidayCalendar_ExchangeDate
            ON dbo.HolidayCalendar (Exchange, HolidayDate);
        PRINT 'Table HolidayCalendar created.';
    END
    ELSE
    BEGIN
        PRINT 'Table HolidayCalendar already exists.';
    END

GO;
--============================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='SingleOrderHistory' AND xtype='U')
    BEGIN        	
        CREATE TABLE dbo.SingleOrderHistory
        (
            Exchange                 NVARCHAR(50)     NOT NULL,
            TradingSymbol            NVARCHAR(100)    NOT NULL,
            NorenOrderNumber         BIGINT           NOT NULL,
            InternalOrderStatus      NVARCHAR(50)     NOT NULL,
            SnoOrderDt               BIGINT           NOT NULL,
            SnoOrderNumber           BIGINT           NOT NULL,
            ExchangeTime             DATETIME2        NOT NULL,
            OptionalIntropExchange   NVARCHAR(50)     NOT NULL,
            NorenTime                DATETIME2        NOT NULL,
            ProductDisplayName       NVARCHAR(100)    NOT NULL,
            FillQuantity             INT           NOT NULL,
            FillPrice                DECIMAL(18,6)    NOT NULL,
            FillId                   BIGINT           NOT NULL,
            Quantity                 INT           NOT NULL,
            Price                    DECIMAL(18,6)    NOT NULL,
            ProductType              NVARCHAR(50)     NOT NULL,
            KidId                    INT              NOT NULL,
            OrderSource              NVARCHAR(50)     NOT NULL,
            RejectionBy              NVARCHAR(100)    NOT NULL,
            Pan                      NVARCHAR(20)     NOT NULL,
            SourceUid                NVARCHAR(100)    NOT NULL,
            OrderStatus              NVARCHAR(50)     NOT NULL,
            ReportType               NVARCHAR(50)     NOT NULL,
            TransactionType          NVARCHAR(50)     NOT NULL,
            PriceType                NVARCHAR(50)     NOT NULL,
            TotalFilled              INT           NOT NULL,
            AvgPriceOfTradedQuantity DECIMAL(18,6)    NOT NULL,
            RejectionReason          NVARCHAR(500)    NOT NULL,
            ExchangeOrderNumber      NVARCHAR(100)    NOT NULL,
            CancelledQuantity        INT           NOT NULL,
            Remarks                  NVARCHAR(500)    NOT NULL,
            DisclosedQuantity        INT           NOT NULL,
            TriggerPrice             DECIMAL(18,6)    NOT NULL,
            RetentionType            NVARCHAR(50)     NOT NULL,
            UserId                   NVARCHAR(100)    NOT NULL,
            AccountId                NVARCHAR(100)    NOT NULL,
            BookProfitPrice          DECIMAL(18,6)    NOT NULL,
            BookLossProfit           DECIMAL(18,6)    NOT NULL,
            TrailingPrice            DECIMAL(18,6)    NOT NULL,
            Amo                      NVARCHAR(10)     NOT NULL,
            PricePrecision           INT              NOT NULL,
            TickSize                 DECIMAL(18,6)    NOT NULL,
            LotSize                  DECIMAL(18,6)    NOT NULL,
            Token                    INT           NOT NULL,
            OrderDateTime            DATETIME2        NOT NULL,
            EpochOrderEntryDateTime  BIGINT           NOT NULL,
            Extm                     NVARCHAR(50)     NOT NULL
        );

        PRINT 'Table SingleOrderHistory created.';
    END
    ELSE
    BEGIN
        PRINT 'Table SingleOrderHistory already exists.';
    END

GO
--============================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TradeBook' AND xtype='U')
    BEGIN
      --drop table TradeBook
        CREATE TABLE dbo.TradeBook
        (
            Exchange             NVARCHAR(64)   NOT NULL,
            TradingSymbol        NVARCHAR(100)   NOT NULL,
            SnoOrderNumber       BIGINT          NOT NULL,
            Remarks              NVARCHAR(255)   NULL,
            SnoOrderDt           BIGINT          NOT NULL,
            NorenOrderNumber     BIGINT          NOT NULL,
            UserId               NVARCHAR(50)    NOT NULL,
            AccountId            NVARCHAR(50)    NOT NULL,
            PriceType            NVARCHAR(50)    NOT NULL,
            RetentionType        NVARCHAR(50)             NOT NULL,
            ProductDisplayName   NVARCHAR(50)             NOT NULL,
            ProductType          NVARCHAR(50)             NOT NULL,
            FillDateTime         DATETIME2       NOT NULL,
            FillId               BIGINT    NOT NULL,
            TransactionType      NVARCHAR(50)             NOT NULL,
            Quantity             INT          NOT NULL,
            Token                INT          NOT NULL,
            TotalFilled          INT          NOT NULL,
            FillQuantity         INT          NOT NULL,
            Multiplier           DECIMAL(18,6)   NOT NULL,
            PricePrecision       INT             NOT NULL,
            TickSize             DECIMAL(18,6)   NOT NULL,
            LotSize              DECIMAL(18,6)   NOT NULL,
            Price                DECIMAL(18,6)   NOT NULL,
            PriceFactor          NVARCHAR(50)    NULL,
            FillPrice            DECIMAL(18,6)   NOT NULL,
            NorenTime            DATETIME2       NOT NULL,
            AveragePrice         DECIMAL(18,6)   NOT NULL,
            ExchangeOrderNumber  NVARCHAR(50)    NOT NULL,
            ExchangeTime         DATETIME2       NOT NULL
        );

        PRINT 'Table TradeBook created.';
    END
    ELSE
    BEGIN
        PRINT 'Table TradeBook already exists.';
    END

GO
--============================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='OrderBook' AND xtype='U')
    BEGIN        	
        CREATE TABLE OrderBook (
            UserId NVARCHAR(64) NOT NULL,
            AccountId NVARCHAR(64) NOT NULL,
            SymbolName NVARCHAR(64) NOT NULL,
            KidId INT NOT NULL,
            NorenOrderNumber BIGINT NOT NULL,
            Exchange NVARCHAR(50) NOT NULL,
            TradingSymbol NVARCHAR(100) NOT NULL,
            RejectionBy NVARCHAR(100) NOT NULL,
            SourceUid NVARCHAR(100) NOT NULL,
            CompanyName NVARCHAR(200) NOT NULL,
            Quantity DECIMAL(18,6) NOT NULL,
            TotalFilled INT NOT NULL,
            MarginPriceFromModify DECIMAL(18,6) NOT NULL,
            RTriggerPrice DECIMAL(18,6) NOT NULL,
            RQuantity DECIMAL(18,6) NOT NULL,
            ROrderRemaningQuantity INT NOT NULL,
            OrderStatus NVARCHAR(50) NOT NULL,
            InternalOrderStatus NVARCHAR(50) NOT NULL,
            IpAddress NVARCHAR(50) NOT NULL,
            EpochOrderEntryDateTime BIGINT NOT NULL,
            TriggerPrice DECIMAL(18,6) NOT NULL,
            Remarks NVARCHAR(500) NOT NULL,
            TransactionType NVARCHAR(50) NOT NULL,
            PriceType NVARCHAR(50) NOT NULL,
            RetentionType NVARCHAR(50) NOT NULL,
            RejectionReason NVARCHAR(500) NOT NULL,
            Token INT NOT NULL,
            Multiplier DECIMAL(18,6) NOT NULL,
            PriceFactor NVARCHAR(50) NOT NULL,
            InstrumentName NVARCHAR(50) NOT NULL,
            OrderSource NVARCHAR(50) NOT NULL,
            PricePrecision INT NOT NULL,
            TickSize DECIMAL(18,6) NOT NULL,
            LotSize DECIMAL(18,6) NOT NULL,
            Price DECIMAL(18,6) NOT NULL,
            AveragePrice DECIMAL(18,6) NOT NULL,
            RPrice DECIMAL(18,6) NOT NULL,
            BookProfitPrice DECIMAL(18,6) NOT NULL,
            BookLossPrice DECIMAL(18,6) NOT NULL,
            RBookLossPrice DECIMAL(18,6) NOT NULL,
            TrailingPrice DECIMAL(18,6) NOT NULL,
            DisclosedQuantity INT NOT NULL,
            CancelledQuantity INT NOT NULL,
            SnoFillId BIGINT NOT NULL,
            SnoOrderNumber BIGINT NOT NULL,
            SnoOrderDt BIGINT NOT NULL,
            BranchId NVARCHAR(50) NOT NULL,
            C NVARCHAR(50) NOT NULL,
            ProductDisplayName NVARCHAR(100) NOT NULL,
            ProductType NVARCHAR(50) NOT NULL,
            NorenTime DATETIME2 NOT NULL,
            ExchangeTime DATETIME2 NOT NULL,
            AlgoId INT NOT NULL,
            ExchangeOrderNumber NVARCHAR(100) NOT NULL,
            Amo NVARCHAR(10) NOT NULL,
            MarketProtectionPercentage DECIMAL(8,4) NOT NULL default(0)
        );
        PRINT 'Table OrderBook created.';
    END
    ELSE
    BEGIN
        PRINT 'Table OrderBook already exists.';
    END

GO
--============================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='DailyPAndLSummary' AND xtype='U')
    BEGIN
        
        CREATE TABLE dbo.DailyPAndLSummary
        (
            -- Primary identifiers
            SymbolName              VARCHAR(64)      NOT NULL,
            TradeDate               DATE              NOT NULL,

            -- P&L fields
            GrossIntraDayPAndL        DECIMAL(18,2)     NOT NULL,
            NetIntraDayPAndL        DECIMAL(18,2)     NOT NULL,
            NetDeliveryValue        DECIMAL(18,2)     NOT NULL,

            -- Counts
            BuyCount                INT               NOT NULL,
            BuyQuantity             INT               NOT NULL,
            BuyCharges              Decimal(10,2)     NOT NULL,
            SellCount               INT               NOT NULL,
            SellQuantity            INT               NOT NULL,
            SellCharges              Decimal(10,2)     NOT NULL,

            -- Average prices
            BuyAveragePrice         DECIMAL(18,4)     NOT NULL,
            SellAveragePrice        DECIMAL(18,4)     NOT NULL,
            TotalCharges            Decimal(10,2)     NOT NULL,

            -- Net quantities
            NetIntraDayQuantity     INT               NOT NULL,
            NetDeliveryQuantity     INT               NOT NULL,

            -- Value calculations
            NetIntraDayBuyValue     DECIMAL(18,2)     NOT NULL,
            NetIntraDaySellValue    DECIMAL(18,2)     NOT NULL,

            NetDeliveryBuyValue     DECIMAL(18,2)     NOT NULL,
            NetDeliverySellValue    DECIMAL(18,2)     NOT NULL,

            -- Recommended Composite Primary Key
            CONSTRAINT PK_DailyPAndLSummary 
                PRIMARY KEY (SymbolName, TradeDate)
        );
        PRINT 'Table DailyPAndLSummary created.';
    END
    ELSE
    BEGIN
        PRINT 'Table DailyPAndLSummary already exists.';
    END

GO
--============================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Exchange' AND xtype='U')
    BEGIN
        CREATE TABLE Exchange (
            [Id] INT IDENTITY(1,1) PRIMARY KEY,
            [Code] NVARCHAR(8) NOT NULL,
            [Description] NVARCHAR(128) NOT NULL,
            
            Active BIT NOT NULL DEFAULT 1, -- Added ACTIVE column to indicate if the instrument is active
            LastUpdatedInDbAt DATETIME2 NOT NULL DEFAULT GETDATE(),
            CreatedInDbAt DATETIME2 NOT NULL DEFAULT GETDATE()
            
            CONSTRAINT UQ_Exchange_Code UNIQUE ([Code]) -- Ensure unique entries per exchange
        );
        PRINT 'Table Exchange created.';
    END
    ELSE
        PRINT 'Table Exchange already exists.';

GO

--============================================================
IF TYPE_ID(N'[dbo].[TLatestNpsNav]') IS NULL
    BEGIN
        CREATE TYPE dbo.TLatestNpsNav AS TABLE
        (
            PFMCode         VARCHAR(20),
            PFMName         VARCHAR(200),
            SchemeCode      VARCHAR(20),
            SchemeName      VARCHAR(200),
            NAV             DECIMAL(18,6),
            Return_1D       DECIMAL(10,4),
            Return_7D       DECIMAL(10,4),
            Return_1M       DECIMAL(10,4),
            Return_3M       DECIMAL(10,4),
            Return_6M       DECIMAL(10,4),
            Return_1Y       DECIMAL(10,4),
            Return_3Y       DECIMAL(10,4),
            Return_5Y       DECIMAL(10,4),
            NavDate         Date
        );

        PRINT 'Type TLatestNpsNav created.';
    END
    ELSE
        PRINT 'Type TLatestNpsNav already exists.';

GO
--============================================================
IF TYPE_ID(N'[dbo].[THistoricNpsNav]') IS NULL
    BEGIN
        CREATE TYPE dbo.THistoricNpsNav AS TABLE
        (
            scheme_code VARCHAR(20),
            scheme_name VARCHAR(200),
            nav_date    DATE,
            nav         DECIMAL(18,6),

            PRIMARY KEY (scheme_code, nav_date)
        );

        PRINT 'Type THistoricNpsNav created.';
    END
    ELSE
        PRINT 'Type THistoricNpsNav already exists.';

GO
--============================================================
IF TYPE_ID(N'[dbo].[TCorporateActions]') IS NULL
    BEGIN
        CREATE TYPE dbo.TCorporateActions AS TABLE
        (
            Symbol              NVARCHAR(20)      NOT NULL,
	        Exchange            NVARCHAR(10)      NOT NULL,
	        Series              NVARCHAR(20)      NOT NULL,
	        Indicative   		NVARCHAR(10)      NULL,
	        FaceValue			DECIMAL(5,2)	  NOT NULL,
            Subject             NVARCHAR(200)     NULL,
            ExDate              DATE              NULL,
            RecordDate          DATE              NULL,
	        BookClosureStartDate          DATE              NULL,
	        BookClosureEndDate            DATE              NULL,
	        NoDeliveryStartDate      DATE              NULL,
	        NoDeliveryEndDate        DATE              NULL,
	        CompanyName         NVARCHAR(200)     NOT NULL,
            Isin                NVARCHAR(32)      NOT NULL,
	        AnnouncementDate        DATE          NOT NULL
        );

        PRINT 'Type TCorporateActions created.';
    END
    ELSE
        PRINT 'Type TCorporateActions already exists.';

GO
--============================================================
IF TYPE_ID(N'[dbo].[TBrokerageAndTaxes]') IS NULL
    BEGIN
        CREATE TYPE dbo.TBrokerageAndTaxes AS TABLE
        (
            TradingSymbol NVARCHAR(64),
            Exchange NVARCHAR(16),
            BrokerageAmount DECIMAL(18, 2),
            ClearingMemberAmount DECIMAL(18, 2),
            ExchangeOrderNumber NVARCHAR(64),
            FillDateTime DATETIME2,
            FillId NVARCHAR(64),
            FillPrice DECIMAL(18, 4),
            FillQuantity DECIMAL(18, 4),
            Gst DECIMAL(18, 2),
            InvestorProtectionFundTrustAmount DECIMAL(18, 2),
            NorenOrderNumber BIGINT,
            SnoOrderNumber BIGINT,
            NorenTime DATETIME2,
            ProductType NVARCHAR(32),
            Remarks NVARCHAR(256),
            SebiCharges DECIMAL(18, 2),
            ExchangeCharges DECIMAL(18, 2),
            SecurityTransactionTax DECIMAL(18, 2),
            StampDuty DECIMAL(18, 2),
            Token BIGINT,
            TotalCharges DECIMAL(18, 2),
            TransactionType NVARCHAR(32),
            Url NVARCHAR(256)
        );
        PRINT 'Type TBrokerageAndTaxes created.';
    END
    ELSE
        PRINT 'Type TBrokerageAndTaxes already exists.';

GO
--============================================================
IF TYPE_ID(N'[dbo].[TExchanges]') IS NULL
    BEGIN
        CREATE TYPE [dbo].[TExchanges] AS TABLE
        (
            Code NVARCHAR(8) NOT NULL,
            [Description] NVARCHAR(128) NULL   
        )
        PRINT 'Type TExchanges created.';
    END
    ELSE
        PRINT 'Type TExchanges already exists.';

GO
--============================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='StockInstruments' AND xtype='U')
    BEGIN
        CREATE TABLE StockInstruments (
            [Id] INT IDENTITY(1,1) PRIMARY KEY,
            Token INT NOT NULL,
            SymbolName NVARCHAR(64) NOT NULL,
            TradingSymbol NVARCHAR(64) NOT NULL,
            CompanyName  NVARCHAR(128) NOT NULL,
            ExchangeCode NVARCHAR(8) NOT NULL,         --Foreign Key to Exchange table
            Segment NVARCHAR(8) NOT NULL,
            InstrumentName NVARCHAR(8) NOT NULL,
            Isin NVARCHAR(16) NOT NULL,
            TickSize DECIMAL(8,4) NOT NULL,
            PricePrecision SMALLINT NOT NULL,
            LotSize DECIMAL(4,2) NOT NULL,
            UpperCircuit DECIMAL(15,4) NOT NULL,
            LowerCircuit DECIMAL(15,4) NOT NULL,
            LastTradeDateTime DATETIME2,
            LastUpdateTime DATETIME2,
            LastTradePrice DECIMAL(15,4) NOT NULL,
            AverageTradePrice DECIMAL(15,4) NOT NULL,
            IssueCapital DECIMAL(20,2) NOT NULL,
            Wk52High DECIMAL(15,4),
            Wk52Low DECIMAL(15,4),
            IssueDate DATE,
            ListingDate DATE,
            FreezeQuantity INT NOT NULL Default 0,
            IsFutureAllowed BIT NOT NULL Default 0,
            IsOptionAllowed BIT NOT NULL Default 0,
            Active BIT NOT NULL DEFAULT 1, -- Added ACTIVE column to indicate if the instrument is active
            LastUpdatedInDbAt DATETIME2 NOT NULL DEFAULT GETDATE(),
            CreatedInDbAt DATETIME2 NOT NULL DEFAULT GETDATE()
            
            CONSTRAINT UQ_StockInstruments_Token UNIQUE ([Token]), 
            CONSTRAINT FK_StockInstruments_Exchange FOREIGN KEY (ExchangeCode) REFERENCES Exchange (Code)
            ON DELETE CASCADE
            ON UPDATE CASCADE
        );

        CREATE INDEX IX_StockInstruments_Active
            ON dbo.StockInstruments (Active, TradingSymbol, ExchangeCode);

        PRINT 'Table StockInstruments created.';
    END
    ELSE
    BEGIN
        PRINT 'Table StockInstruments already exists.';
    END

GO
--============================================================
IF TYPE_ID(N'[dbo].[TStockInstruments]') IS NULL
    BEGIN
        CREATE TYPE [dbo].TStockInstruments AS TABLE
        (
            Token INT NOT NULL,
            SymbolName NVARCHAR(64) NOT NULL,
            TradingSymbol NVARCHAR(64) NOT NULL,
            CompanyName  NVARCHAR(128) NOT NULL,
            ExchangeCode NVARCHAR(8) NOT NULL,         --Foreign Key to Exchange table
            Segment NVARCHAR(8) NOT NULL,
            InstrumentName NVARCHAR(8) NOT NULL,
            Isin NVARCHAR(16) NOT NULL,
            TickSize DECIMAL(8,4) NOT NULL,
            PricePrecision SMALLINT NOT NULL,
            LotSize DECIMAL(4,2) NOT NULL,
            UpperCircuit DECIMAL(15,4) NOT NULL,
            LowerCircuit DECIMAL(15,4) NOT NULL,
            LastTradeDateTime DATETIME2,
            LastUpdateTime DATETIME2,
            LastTradePrice DECIMAL(15,4) NOT NULL,
            AverageTradePrice DECIMAL(15,4) NOT NULL,
            Wk52High DECIMAL(15,4) NOT NULL,
            Wk52Low DECIMAL(15,4) NOT NULL,
            IssueCapital DECIMAL(20,2) NOT NULL,
            IssueDate DATE,
            ListingDate DATE,
            IsFutureAllowed BIT NOT NULL DEFAULT 0,
            IsOptionAllowed BIT NOT NULL DEFAULT 0
        )
        PRINT 'Type TStockInstruments created.';
    END
    ELSE
        PRINT 'Type TStockInstruments already exists.';

GO
--============================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='StocksOhlcv_1' AND xtype='U')
    BEGIN
        CREATE TABLE StocksOhlcv_1 (
            InstrumentId INT NOT NULL,
            StartdateTime DATETIME2 NOT NULL,
            [Open] DECIMAL(15,4) NOT NULL,
            [High] DECIMAL(15,4) NOT NULL,

            [Low] DECIMAL(15,4) NOT NULL,
            [Close] DECIMAL(15,4) NOT NULL,
            [Volume] DECIMAL(10) NOT NULL,
            [TradeDate] AS CAST(StartDateTime AS date) PERSISTED,

            CONSTRAINT FK_StockInstruments_1_InstrumentId FOREIGN KEY (InstrumentId) REFERENCES StockInstruments (Id)
            ON DELETE CASCADE
            ON UPDATE CASCADE
        );
        CREATE CLUSTERED INDEX IX_StocksOhlcv_1_Id_DateTime ON StocksOhlcv_1 (InstrumentId, StartDateTime);
        UPDATE STATISTICS dbo.StocksOhlcv_1 WITH FULLSCAN;
        
        CREATE INDEX IX_StocksOhlcv_InstrumentDate
        ON dbo.StocksOhlcv_1 (InstrumentId, StartDateTime)
        INCLUDE (Volume);  -- add other fields if frequently used

        CREATE INDEX IX_StocksOhlcv_Instrument_TradeDate
        ON dbo.StocksOhlcv_1 (InstrumentId, TradeDate)
        INCLUDE (StartDateTime);

        PRINT 'Table StocksOhlcv_1 created.';
    END
    ELSE
    BEGIN
        PRINT 'Table StocksOhlcv_1 already exists.';
    END

GO
--============================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='StocksOhlcv_1440' AND xtype='U')
    BEGIN
        CREATE TABLE StocksOhlcv_1440 (
            InstrumentId INT NOT NULL,
            StartDateTime DATETIME2 NOT NULL,
            [Open] DECIMAL(15,4) NOT NULL,
            [High] DECIMAL(15,4) NOT NULL,
            [Low] DECIMAL(15,4) NOT NULL,
            [Close] DECIMAL(15,4) NOT NULL,
            [Volume] DECIMAL(10) NOT NULL,
            
            CONSTRAINT FK_StockInstruments_1440_InstrumentId FOREIGN KEY (InstrumentId) REFERENCES StockInstruments (Id)
            ON DELETE CASCADE
            ON UPDATE CASCADE
        );
        CREATE CLUSTERED INDEX IX_StocksOhlcv_1440_Id_DateTime ON StocksOhlcv_1440 (InstrumentId, StartDateTime);
        PRINT 'Table StocksOhlcv_1440 created.';
    END
    ELSE
    BEGIN
        PRINT 'Table StocksOhlcv_1440 already exists.';
    END

GO
--============================================================
IF TYPE_ID(N'[dbo].[TStocksOhlcv]') IS NULL
    BEGIN
        CREATE TYPE [dbo].TStocksOhlcv AS TABLE
        (
            Token INT NOT NULL,
            StartDateTime DATETIME2 NOT NULL,
            [Open] DECIMAL(15,4) NOT NULL,
            [High] DECIMAL(15,4) NOT NULL,
            [Low] DECIMAL(15,4) NOT NULL,
            [Close] DECIMAL(15,4) NOT NULL,
            [Volume] DECIMAL(10) NOT NULL
        )
        PRINT 'Type TStocksOhlcv created.';
    END
    ELSE
        PRINT 'Type TStocksOhlcv already exists.';

GO
--============================================================
IF TYPE_ID(N'[dbo].[TPositions]') IS NULL
    BEGIN
        CREATE TYPE dbo.TPositions AS TABLE
        (
            Token                       INT,
            Exchange                    VARCHAR(10),
            SymbolName                  VARCHAR(50),
            TradingSymbol               VARCHAR(50),
            AccountId                   VARCHAR(20),
            UserId                      VARCHAR(20),
			ProductDisplayName          VARCHAR(50),
            ProductType                 VARCHAR(20),

            NetPositionQuantity         INT,
            NetAveragePositionPrice     DECIMAL(18,4),

            DayBuyQuantity              INT,
            DaySellQuantity             INT,
			DayAveragePrice             DECIMAL(18,4),
            DayAverageBuyPrice          DECIMAL(18,4),
            DayAverageSellPrice         DECIMAL(18,4),
			FreezeQuantity				INT,
			CompanyName					VARCHAR(64),
            DayBuyAmount                DECIMAL(18,4),
            DaySellAmount               DECIMAL(18,4),

            CarrfyFwdBuyQuantity        INT,
            CarryFwdSellQuantity        INT,
            CarryFwdOriginalAveragePrice DECIMAL(18,4),
            CarryFwdAverageBuyPrice     DECIMAL(18,4),
            CarryFwdAverageSellPrice    DECIMAL(18,4),
            CarryFwdBuyAmount           DECIMAL(18,4),
            CarryFwdSellAmount          DECIMAL(18,4),

			TotalBuyAmount          	DECIMAL(18,4),
			TotalSellAmount          	DECIMAL(18,4),
			
			TotalBuyAveragePrice        DECIMAL(18,4),
            TotalSellAveragePrice       DECIMAL(18,4),
            LastTradePrice              DECIMAL(18,4),
            RealizedPNl                 DECIMAL(18,4),
            UnRealizedMTM               DECIMAL(18,4),
            BreakEvenPrice              DECIMAL(18,4),
			
			AvgPriceUploadedAlongWithHoldings 		DECIMAL(18,4),
			NetAvgPriceUploadedAlongWithHoldings 	DECIMAL(18,4),
			
            OpenBuyQuantity             INT,
            OpenSellQuantity            INT,
            OpenBuyAmount               DECIMAL(18,4),
            OpenSellAmount              DECIMAL(18,4),
            OpenAverageBuyPrice         DECIMAL(18,4),
            OpenAverageSellPrice        DECIMAL(18,4),

            Multiplier                  DECIMAL(18,4),
            PricePrecision              INT,
            TickSize                    DECIMAL(18,4),
            LotSize                     DECIMAL(18,4),
            PriceFactor                 VARCHAR(50),
            InstrumentName              VARCHAR(30),
            CreatedAt                   DateTime2
        );

        PRINT 'Type TPositions created.';
    END
    ELSE
        PRINT 'Type TPositions already exists.';

GO
--============================================================
IF TYPE_ID(N'[dbo].[THoldings]') IS NULL
    BEGIN
        CREATE TYPE dbo.THoldings AS TABLE
        (
            HoldingQuantity                        INT,
            NonPoaDisplayQuantity                  INT,
            NonPoaDisplayT1Quantity                INT,
            BeneficiaryQuantity                    INT,
            BrokerEquityPledgedAsCollateralQuantity INT,
            BrokerForexPledgedAsCollateralQuantity INT,
            BrokerAllMarketPledgedAsCollateralQuantity INT,
            BrokerDerivativeMarketPledgedAsCollateralQuantity INT,
            BuyTodaySellTommorrowQuantity          INT,
            HoldingQuantityUsedToday               INT,
            DpHoldingQuantity                      INT,
            AvgPriceUploadedAlongWithHoldings      DECIMAL(18,4),
            HairCutPercOnPledgedSecurities         DECIMAL(18,4),
            TodaySellAmount                        DECIMAL(18,4),
            ProductDisplayName                     VARCHAR(20),
            ProductType                            VARCHAR(10),
            TradeQuantity                          INT,
            ExchangePendingInstructionDoneQuantity INT,

            Exchange1                              VARCHAR(10),
            TradingSymbol1                         VARCHAR(50),
            Token1                                 INT,

            Exchange2                              VARCHAR(10),
            TradingSymbol2                         VARCHAR(50),
            Token2                                 INT,
            
            DailyClose                             DECIMAL(18,4)
        );
        PRINT 'Type THoldings created.';
    END
    ELSE
        PRINT 'Type THoldings already exists.';

GO
--============================================================
IF TYPE_ID(N'[dbo].[TMarginEquity]') IS NULL
    BEGIN
       CREATE TYPE dbo.TMarginEquity AS TABLE
        (
            Symbol NVARCHAR(50) NOT NULL,
            Segment NVARCHAR(20) NOT NULL,
            MISMarginInPercentage DECIMAL(10,2) NOT NULL,
            CNCMarginMISMarginInPercentage DECIMAL(10,2) NOT NULL,
            MISLeverageX DECIMAL(10,2) NOT NULL,
            CNCLeverageX DECIMAL(10,2) NOT NULL
        );
        PRINT 'Type TMarginEquity created.';
    END
    ELSE
        PRINT 'Type TMarginEquity already exists.';

GO
--===============================tstock finished=============================
IF TYPE_ID(N'[dbo].[TTradeBook]') IS NULL
    BEGIN
        CREATE TYPE dbo.TTradeBook AS TABLE
        (
            Exchange             NVARCHAR(64)   NOT NULL,
            TradingSymbol        NVARCHAR(100)   NOT NULL,
            SnoOrderNumber       BIGINT          NOT NULL,
            Remarks              NVARCHAR(255)   NULL,
            SnoOrderDt           BIGINT          NOT NULL,
            NorenOrderNumber     BIGINT          NOT NULL,
            UserId               NVARCHAR(50)    NOT NULL,
            AccountId            NVARCHAR(50)    NOT NULL,
            PriceType            NVARCHAR(50)    NOT NULL,
            RetentionType        NVARCHAR(50)             NOT NULL,
            ProductDisplayName   NVARCHAR(50)             NOT NULL,
            ProductType          NVARCHAR(50)             NOT NULL,
            FillDateTime         DATETIME2       NOT NULL,
            FillId               BIGINT    NOT NULL,
            TransactionType      NVARCHAR(50)             NOT NULL,
            Quantity             INT          NOT NULL,
            Token                INT          NOT NULL,
            TotalFilled          INT          NOT NULL,
            FillQuantity         INT          NOT NULL,
            Multiplier           DECIMAL(18,6)   NOT NULL,
            PricePrecision       INT             NOT NULL,
            TickSize             DECIMAL(18,6)   NOT NULL,
            LotSize              DECIMAL(18,6)   NOT NULL,
            Price                DECIMAL(18,6)   NOT NULL,
            PriceFactor          NVARCHAR(50)    NULL,
            FillPrice            DECIMAL(18,6)   NOT NULL,
            NorenTime            DATETIME2       NOT NULL,
            AveragePrice         DECIMAL(18,6)   NOT NULL,
            ExchangeOrderNumber  NVARCHAR(50)    NOT NULL,
            ExchangeTime         DATETIME2       NOT NULL
        );

        PRINT 'Type TTradeBook created.';
    END
    ELSE
        PRINT 'Type TTradeBook already exists.';

GO
--============================================================
IF TYPE_ID(N'[dbo].[TSingleOrderHistory]') IS NULL
    BEGIN
        CREATE TYPE dbo.TSingleOrderHistory AS TABLE
        (
            Exchange                 NVARCHAR(50)     NOT NULL,
            TradingSymbol            NVARCHAR(100)    NOT NULL,
            NorenOrderNumber         BIGINT           NOT NULL,
            InternalOrderStatus      NVARCHAR(50)     NOT NULL,
            SnoOrderDt               BIGINT           NOT NULL,
            SnoOrderNumber           BIGINT           NOT NULL,
            ExchangeTime             DATETIME2        NOT NULL,
            OptionalIntropExchange   NVARCHAR(50)     NOT NULL,
            NorenTime                DATETIME2        NOT NULL,
            ProductDisplayName       NVARCHAR(100)    NOT NULL,
            FillQuantity             INT           NOT NULL,
            FillPrice                DECIMAL(18,6)    NOT NULL,
            FillId                   BIGINT           NOT NULL,
            Quantity                 INT           NOT NULL,
            Price                    DECIMAL(18,6)    NOT NULL,
            ProductType              NVARCHAR(50)     NOT NULL,
            KidId                    INT              NOT NULL,
            OrderSource              NVARCHAR(50)     NOT NULL,
            RejectionBy              NVARCHAR(100)    NOT NULL,
            Pan                      NVARCHAR(20)     NOT NULL,
            SourceUid                NVARCHAR(100)    NOT NULL,
            OrderStatus              NVARCHAR(50)     NOT NULL,
            ReportType               NVARCHAR(50)     NOT NULL,
            TransactionType          NVARCHAR(50)     NOT NULL,
            PriceType                NVARCHAR(50)     NOT NULL,
            TotalFilled              INT           NOT NULL,
            AvgPriceOfTradedQuantity DECIMAL(18,6)    NOT NULL,
            RejectionReason          NVARCHAR(500)    NOT NULL,
            ExchangeOrderNumber      NVARCHAR(100)    NOT NULL,
            CancelledQuantity        INT           NOT NULL,
            Remarks                  NVARCHAR(500)    NOT NULL,
            DisclosedQuantity        INT           NOT NULL,
            TriggerPrice             DECIMAL(18,6)    NOT NULL,
            RetentionType            NVARCHAR(50)     NOT NULL,
            UserId                   NVARCHAR(100)    NOT NULL,
            AccountId                NVARCHAR(100)    NOT NULL,
            BookProfitPrice          DECIMAL(18,6)    NOT NULL,
            BookLossProfit           DECIMAL(18,6)    NOT NULL,
            TrailingPrice            DECIMAL(18,6)    NOT NULL,
            Amo                      NVARCHAR(10)     NOT NULL,
            PricePrecision           INT              NOT NULL,
            TickSize                 DECIMAL(18,6)    NOT NULL,
            LotSize                  DECIMAL(18,6)    NOT NULL,
            Token                    INT           NOT NULL,
            OrderDateTime            DATETIME2        NOT NULL,
            EpochOrderEntryDateTime  BIGINT           NOT NULL,
            Extm                     NVARCHAR(50)     NOT NULL
        );

        PRINT 'Type TSingleOrderHistory created.';
    END
    ELSE
        PRINT 'Type TSingleOrderHistory already exists.';

GO
--============================================================
IF TYPE_ID(N'[dbo].[TOrderBook]') IS NULL
    BEGIN
        CREATE TYPE dbo.TOrderBook AS TABLE
        (
            UserId NVARCHAR(64) NOT NULL,
            AccountId NVARCHAR(64) NOT NULL,
            SymbolName NVARCHAR(64) NOT NULL,
            KidId INT NOT NULL,
            NorenOrderNumber BIGINT NOT NULL,
            Exchange NVARCHAR(50) NOT NULL,
            TradingSymbol NVARCHAR(100) NOT NULL,
            RejectionBy NVARCHAR(100) NOT NULL,
            SourceUid NVARCHAR(100) NOT NULL,
            CompanyName NVARCHAR(200) NOT NULL,
            Quantity DECIMAL(18,6) NOT NULL,
            TotalFilled INT NOT NULL,
            MarginPriceFromModify DECIMAL(18,6) NOT NULL,
            RTriggerPrice DECIMAL(18,6) NOT NULL,
            RQuantity DECIMAL(18,6) NOT NULL,
            ROrderRemaningQuantity INT NOT NULL,
            OrderStatus NVARCHAR(50) NOT NULL,
            InternalOrderStatus NVARCHAR(50) NOT NULL,
            IpAddress NVARCHAR(50) NOT NULL,
            EpochOrderEntryDateTime BIGINT NOT NULL,
            TriggerPrice DECIMAL(18,6) NOT NULL,
            Remarks NVARCHAR(500) NOT NULL,
            TransactionType NVARCHAR(50) NOT NULL,
            PriceType NVARCHAR(50) NOT NULL,
            RetentionType NVARCHAR(50) NOT NULL,
            RejectionReason NVARCHAR(500) NOT NULL,
            Token INT NOT NULL,
            Multiplier DECIMAL(18,6) NOT NULL,
            PriceFactor NVARCHAR(50) NOT NULL,
            InstrumentName NVARCHAR(50) NOT NULL,
            OrderSource NVARCHAR(50) NOT NULL,
            PricePrecision INT NOT NULL,
            TickSize DECIMAL(18,6) NOT NULL,
            LotSize DECIMAL(18,6) NOT NULL,
            Price DECIMAL(18,6) NOT NULL,
            AveragePrice DECIMAL(18,6) NOT NULL,
            RPrice DECIMAL(18,6) NOT NULL,
            BookProfitPrice DECIMAL(18,6) NOT NULL,
            BookLossPrice DECIMAL(18,6) NOT NULL,
            RBookLossPrice DECIMAL(18,6) NOT NULL,
            TrailingPrice DECIMAL(18,6) NOT NULL,
            DisclosedQuantity INT NOT NULL,
            CancelledQuantity INT NOT NULL,
            SnoFillId BIGINT NOT NULL,
            SnoOrderNumber BIGINT NOT NULL,
            SnoOrderDt BIGINT NOT NULL,
            BranchId NVARCHAR(50) NOT NULL,
            C NVARCHAR(50) NOT NULL,
            ProductDisplayName NVARCHAR(100) NOT NULL,
            ProductType NVARCHAR(50) NOT NULL,
            NorenTime DATETIME2 NOT NULL,
            ExchangeTime DATETIME2 NOT NULL,
            AlgoId INT NOT NULL,
            ExchangeOrderNumber NVARCHAR(100) NOT NULL,
            Amo NVARCHAR(10) NOT NULL,
            MarketProtectionPercentage DECIMAL(8,4) NOT NULL
        );
        PRINT 'Type TOrderBook created.';
    END
    ELSE
        PRINT 'Type TOrderBook already exists.';

GO
--============================================================

--============================================================
PRINT '==================All specified tables and types created successfully.======================';

--============================================================

-- Drop the view if it already exists to allow recreation
IF OBJECT_ID('vw_StocksOhlcv_1440') IS NOT NULL
    DROP VIEW vw_StocksOhlcv_1440;
GO
-- Create the View
CREATE VIEW vw_StocksOhlcv_1440
AS
    WITH RankedPrices AS 
    (
        -- Step 1: Rank the prices for the target symbol by date in descending order
        SELECT *,  ROW_NUMBER() OVER (PARTITION BY InstrumentId ORDER BY StartDateTime DESC) AS Ranking
        FROM stocksohlcv_1440
    )
    SELECT *,   LAG([Open]) OVER (PARTITION BY InstrumentId ORDER BY StartDateTime ASC) AS PreviousDayOpen,
                LAG([Close]) OVER (PARTITION BY InstrumentId ORDER BY StartDateTime ASC) AS PreviousDayClose,
                LAG([StartDateTime]) OVER (PARTITION BY InstrumentId ORDER BY StartDateTime ASC) AS PreviousDate,
                LAG(Volume) OVER (PARTITION BY InstrumentId ORDER BY StartDateTime ASC) AS PreviousDayVolume
    FROM RankedPrices
    
GO

--============================================================

-- Drop the view if it already exists to allow recreation
IF OBJECT_ID('vw_LatestTwoDaysPriceAnalysisOverStocksOhlcv_1440') IS NOT NULL
    DROP VIEW vw_LatestTwoDaysPriceAnalysisOverStocksOhlcv_1440;
GO
-- Create the View
CREATE VIEW vw_LatestTwoDaysPriceAnalysisOverStocksOhlcv_1440
AS  
    WITH LatestTwoDays AS
    (
        -- Step 2: Select only the latest two days (rn = 1 is latest, rn = 2 is the day before)
        SELECT *
        FROM vw_StocksOhlcv_1440
        WHERE Ranking = 1
    )
-- Step 3: Calculate the difference and percentage change for the latest day
SELECT 
    p.*,
    StartDateTime AS LatestDate,
    L.Volume As LatestDayVolume,
    [Close] AS LatestDayClose,
    [open] As LatestDayOpen,

    PreviousDate,
    PreviousDayVolume,
    PreviousDayClose,
    PreviousDayOpen,

    ([Close] - PreviousDayClose) AS Last2DaysClosePriceDifference,
    -- Calculate percentage change, handling division by zero for PreviousDayClose
    CASE
        WHEN PreviousDayClose IS NULL OR PreviousDayClose = 0 THEN NULL -- Or 0, depending on business rule
        ELSE (([Close] - PreviousDayClose) / PreviousDayClose) * 100
    END AS Last2DaysClosepricePercentageChange
FROM
    LatestTwoDays L
    join 
    (
        select InstrumentId, AVG(s.VOLUME) as AvgVolume,
        stdev(s.volume) as StDevOfVolume, 
        Max(s.StartDatetIme) as MaxDate, Min(s.StartDateTime) as MinDate, 
        DateDiff(day, Min(s.StartDatetIme) , Max(s.StartDateTime)) as DaysOfData
        from  stocksohlcv_1440  s
        group by InstrumentId
    ) p
    ON p.InstrumentId = L.InstrumentId
WHERE
    PreviousDayClose IS NOT NULL -- Only show the latest day for which a previous day exists for calculation

GO

--============================================================

--============================================================
--============================================================