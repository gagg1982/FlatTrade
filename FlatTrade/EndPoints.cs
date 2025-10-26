namespace FlatTrade
{
    public static class EndPoints
    {
        public static string SetAlertUrl { get; } = "https://piconnect.flattrade.in/PiConnectTP/SetAlert";
        public static string CancelAlertUrl { get; } = "https://piconnect.flattrade.in/PiConnectTP/CancelAlert";
        public static string ModifyAlertUrl { get; } = "https://piconnect.flattrade.in/PiConnectTP/ModifyAlert";
        public static string PendingAlertUrl { get; } = "https://piconnect.flattrade.in/PiConnectTP/GetPendingAlert";
        public static string EnabledAlertTypesUrl { get; } = "https://piconnect.flattrade.in/PiConnectTP/GetEnabledAlertTypes";
        public static string UnSettledTradingDateUrl { get; } = "https://piconnect.flattrade.in/PiConnectTP/GetUnStledTradingDate";

        public static string TradeBookUrl { get; } = "https://piconnect.flattrade.in/PiConnectTP/TradeBook";
        public static string PositionBookUrl { get; } = "https://piconnect.flattrade.in/PiConnectTP/PositionBook";
        public static string ProductConversionUrl { get; } = "https://piconnect.flattrade.in/PiConnectTP/ProductConversion";
        public static string PlaceGttOrderUrl { get; } = "https://piconnect.flattrade.in/PiConnectTP/PlaceGTTOrder";
        public static string ModifyGttOrderUrl { get; } = "https://piconnect.flattrade.in/PiConnectTP/ModifyGTTOrder";
        public static string CancelGttOrderUrl { get; } = "https://piconnect.flattrade.in/PiConnectTP/CancelGTTOrder";
        public static string PendingGttOrderUrl { get; } = "https://piconnect.flattrade.in/PiConnectTP/GetPendingGTTOrder ";
        public static string EnabledGttsUrl { get; } = "https://piconnect.flattrade.in/PiConnectTP/GetEnabledGTTs";
        public static string PlaceCOOrderUrl { get; } = "https://piconnect.flattrade.in/PiConnectTP/PlaceOCOOrder";
        public static string ModifyCOOrderUrl { get; } = "https://piconnect.flattrade.in/PiConnectTP/ModifyOCOOrder";
        public static string CancelCOOrderUrl { get; } = "https://piconnect.flattrade.in/PiConnectTP/CancelOCOOrder";
        public static string PlaceOrderUrl { get; } = "https://piconnect.flattrade.in/PiConnectTP/PlaceOrder";
        public static string ModifyOrderUrl { get; } = "https://piconnect.flattrade.in/PiConnectTP/ModifyOrder";
        public static string CancelOrderUrl { get; } = "https://piconnect.flattrade.in/PiConnectTP/CancelOrder";
        public static string ExitSnoOrderUrl { get; } = "https://piconnect.flattrade.in/PiConnectTP/ExitSNOOrder";
        public static string OrderMarginUrl { get; } = "https://piconnect.flattrade.in/PiConnectTP/GetOrderMargin";
        public static string BrokerageUrl { get; } = "https://piconnect.flattrade.in/PiConnectTP/GetBrokerage";
        public static string BasketMarginUrl { get; } = "https://piconnect.flattrade.in/PiConnectTP/GetBasketMargin";
        public static string OrderBookUrl { get; } = "https://piconnect.flattrade.in/PiConnectTP/OrderBook";
        public static string MultiLegOrderBookUrl { get; } = "https://piconnect.flattrade.in/PiConnectTP/MultiLegOrderBook";
        public static string SingleOrderHistoryUrl { get; } = "https://piconnect.flattrade.in/PiConnectTP/SingleOrdHist";
        public static string IndexListUrl { get; } = "https://piconnect.flattrade.in/PiConnectTP/GetIndexList";
        public static string TopListNamesUrl { get; } = "https://piconnect.flattrade.in/PiConnectTP/TopListName";
        public static string TopListUrl { get; } = "https://piconnect.flattrade.in/PiConnectTP/TopList";
        public static string TimePriceDataUrl { get; } = "https://piconnect.flattrade.in/PiConnectTP/TPSeries";
        public static string EodChartDataUrl { get; } = "https://piconnect.flattrade.in/PiConnectTP/EODChartData";
        public static string OptionChainUrl { get; } = "https://piconnect.flattrade.in/PiConnectTP/GetOptionChain";
        public static string OptionGreekUrl { get; } = "https://piconnect.flattrade.in/PiConnectTP/GetOptionGreek";
        public static string ExchangeMessageUrl { get; } = "https://piconnect.flattrade.in/PiConnectTP/ExchMsg";
        public static string BrokerMessageUrl { get; } = "https://piconnect.flattrade.in/PiConnectTP/GetBrokerMsg";
        public static string SpanCalculatorUrl { get; } = "https://piconnect.flattrade.in/PiConnectTP/SpanCalc";
        public static string MarginCalculatorEquitiesUrl { get; } = "https://flattrade.in/margin-calculator-equities";
        public static string MaxPayOutAmountUrl { get; } = "https://piconnect.flattrade.in/PiConnectTP/GetMaxPayoutAmount";
        public static string FundsPayOutRequestUrl { get; } = "https://piconnect.flattrade.in/PiConnectTP/FundsPayOutReq";
        public static string PayInReportUrl { get; } = "https://piconnect.flattrade.in/PiConnectTP/GetPayinReport";
        public static string PayOutReportUrl { get; } = "https://piconnect.flattrade.in/PiConnectTP/GetPayoutReport";
        public static string CancelPayoutUrl { get; } = "https://piconnect.flattrade.in/PiConnectTP/CancelPayout";

        public static string LimitsUrl { get; } = "https://piconnect.flattrade.in/PiConnectTP/Limits";
        public static string HoldingsUrl { get; } = "https://piconnect.flattrade.in/PiConnectTP/Holdings";
        public static string WebSocketUrl { get; } = "wss://piconnect.flattrade.in/PiConnectWSTp/";
        public static string QuotesUrl { get; } = "https://piconnect.flattrade.in/PiConnectTP/GetQuotes";
        public static string ScripDetailsUrl { get; } = "https://piconnect.flattrade.in/PiConnectTP/SearchScrip";
        public static string GetLinkedScripsUrl { get; } = "https://piconnect.flattrade.in/PiConnectTP/GetLinkedScrips";
        public static string GetSecurityInfoUrl { get; } = "https://piconnect.flattrade.in/PiConnectTP/GetSecurityInfo";
        public static string UserDetailsUrl { get; } = "https://piconnect.flattrade.in/PiConnectTP/UserDetails";
        public static string AuthorizationUrl { get; } = "https://auth.flattrade.in/?app_key=APIKEY";
        public static string TokenAuthenticationUrl { get; } = "https://authapi.flattrade.in/trade/apitoken";


        public static string GetAuthorizationUrl(string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey))
                throw new ArgumentNullException(apiKey);
            return AuthorizationUrl.Replace("APIKEY", apiKey);
        }
    }
}
