using FlatTrade.Common.Types.Base;
using FlatTrade.MarketInfoManager;

namespace FlatTrade.Test
{
    internal static class MarketInfoTest
    {
        public static async Task Execute(Api api)
        {
            Console.WriteLine("=============================================================");
            var (_, _) = await GetTopListNamesTestAsync(api, Exchange.NSE);
            Console.WriteLine("=============== Top LTP ===================================");
            var (_, _) = await GetTopListTestAsync(api, Exchange.NSE, BasketType.NSEEQ, Criteria.LTP, TopBottomType.Top);
            Console.WriteLine("=============== Bottom LTP ===================================");
            var (_, _) = await GetTopListTestAsync(api, Exchange.NSE, BasketType.NSEEQ, Criteria.LTP, TopBottomType.Bottom);
            Console.WriteLine("=============================================================");
            var (_, _) = await GetIndexListTestAsync(api, Exchange.NSE);
            Console.WriteLine("=============================================================");
            var (_, _) = await GetTimePriceDataTestAsync(api, Exchange.NSE, "ETERNAL-EQ", DateTime.Now.AddDays(-30), DateTime.Now, ChartInterval.Thirty);
            Console.WriteLine("=============================================================");
            var (_, _) = await GetTimePriceDataTestAsync(api, Exchange.NSE, "RVNL-EQ", DateTime.Now.AddMinutes(-1440), DateTime.Now, ChartInterval.One);
            Console.WriteLine("=============================================================");
            var (_, _) = await GetEodChartDataTestAsync(api, Exchange.NSE, "ETERNAL-EQ", DateTime.Now.AddDays(-100), DateTime.Now);
            Console.WriteLine("=============================================================");
            var (_, _) = await GetExchangeMessageTestAsync(api, Exchange.NSE);
            Console.WriteLine("=============================================================");
            var (_, _) = await GetBrokerMessageTestAsync(api);
            Console.WriteLine("=============================================================");
            List<PositionRequest> positions = [new PositionRequest
            {
                BuyQuantity =1000,
                ProductType = ProductType.Delivery,
                Exchange = Exchange.NFO,
                OptionType = OptionType.Call_European,
                SymbolName = "ETERNAL31JUL25P275",
                NetQuantity =-24250,
                SellQuantity=500,
                StrikePrice=275.00m,
                InstrumentName = InstrumentName.OPTSTK,
                ExpiryDate = new DateOnly(2025,08,28)
            }];
            var (_, _) = await SpanCalculatorTestAsync(api, positions);
            Console.WriteLine("=============================================================");
            var (_, _) = await GetOptionChainTestAsync(api, "ETERNAL28AUG25P340", Exchange.NFO, 263.55m, 10);
            Console.WriteLine("=============================================================");
            var optionGreekRequest = new OptionGreekRequest
            {
                ExpiryDate = new DateOnly(2025, 08, 28),
                InterestRate = 0.07m,
                SpotPrice = 264.55m,
                OptionType = OptionType.Call_European,
                StrikePrice = 275.00m,
                Volaitility = 0.15m
            };
            var (_, _) = await GetOptionGreekTestAsync(api, optionGreekRequest);
            Console.WriteLine("=============================================================");
        }

        public async static Task<(IEnumerable<SpanCalculatorResponse>?, string)> SpanCalculatorTestAsync(Api api, List<PositionRequest> positions)
        {
            var (response, eMsg) = await api.MarketInfo.SpanCalculatorAsync(positions);
            if (response is null)
            {
                await Console.Error.WriteLineAsync($"Failed SpanCalculatorTestAsync : {eMsg}");
            }
            else
            {
                response.ToList().ForEach(span => Console.WriteLine($"Passed SpanCalculatorTestAsync : SpanValue - Exposure Margin [{span.SpanValue} - {span.ExposureMargin}]"));
            }
            return (response, eMsg);
        }

        public async static Task<(IEnumerable<OptionChainDataResponse>?, string)> GetOptionChainTestAsync(Api api, string tradingSymbol, Exchange exchange, decimal strikePrice, int OneSideCountForPutAndCall)
        {
            var (response, eMsg) = await api.MarketInfo.GetOptionChainAsync(tradingSymbol, exchange, strikePrice, OneSideCountForPutAndCall);
            if (response is null)
            {
                await Console.Error.WriteLineAsync($"Failed GetOptionChainTestAsync : {eMsg}");
            }
            else
            {
                response.ToList().ForEach(data => Console.WriteLine($"Passed GetOptionChainTestAsync : Get option chain for [{data.TradingSymbol}({data.Token}) - {data.OptionType} - {data.TickSize} - {data.LotSize} - {data.StrikePrice}]"));
            }
            return (response, eMsg);
        }

        public async static Task<(OptionGreekResponse?, string)> GetOptionGreekTestAsync(Api api, OptionGreekRequest optionGreekRequest)
        {
            var (response, eMsg) = await api.MarketInfo.GetOptionGreekAsync(optionGreekRequest);
            if (response is null)
            {
                await Console.Error.WriteLineAsync($"Failed GetOptionGreekTestAsync : {eMsg}");
            }
            else
            {
                Console.WriteLine($"Passed GetOptionGreekTestAsync : CallPrice[{response.CallPrice}] - PutPrice[{response.PutPrice}]");
            }
            return (response, eMsg);
        }


        public async static Task<(TopListNamesResponse?, string)> GetTopListNamesTestAsync(Api api, Exchange exchange)
        {
            var (response, eMsg) = await api.MarketInfo.GetTopListNamesAsync(exchange);
            if (response is null)
            {
                await Console.Error.WriteLineAsync($"Failed GetTopListNamesTestAsync : {eMsg}");
            }
            else
            {
                response.BasketCriteriaPair.ForEach(basketCriteriaPair => Console.WriteLine($"Passed GetTopListNamesTestAsync : Get top list names [{basketCriteriaPair.BasketName} - {basketCriteriaPair.Criteria}]"));
            }
            return (response, eMsg);
        }
        public async static Task<(TopListResponse?, string)> GetTopListTestAsync(Api api, Exchange exchange, BasketType basket, Criteria criteria, TopBottomType topOrBottom)
        {
            var (response, eMsg) = await api.MarketInfo.GetTopListAsync(exchange, basket, criteria, topOrBottom);
            if (response is null)
            {
                await Console.Error.WriteLineAsync($"Failed GetTopListTestAsync : {eMsg}");
            }
            else
            {
                response.TopBottomContracts.ToList().ForEach(topBottomContracts => Console.WriteLine($"Passed GetTopListNamesTestAsync : Get top list names [{topBottomContracts.TradingSymbol} - {topBottomContracts.LastTradePrice} - {topBottomContracts.PreviousClosePrice} - {topBottomContracts.LastTradePricePercentageChange}]"));
            }
            return (response, eMsg);
        }
        public async static Task<(IndexListResponse?, string)> GetIndexListTestAsync(Api api, Exchange exchange)
        {
            var (response, eMsg) = await api.MarketInfo.GetIndexListAsync(exchange);
            if (response is null)
            {
                await Console.Error.WriteLineAsync($"Failed GetIndexListTestAsync : {eMsg}");
            }
            else
            {
                response.IndexNameTokenList.ForEach(indexNameToken => Console.WriteLine($"Passed GetIndexListTestAsync : Get index list [{indexNameToken.IndexName} - {indexNameToken.Token}]"));
            }
            return (response, eMsg);
        }
        public async static Task<(IEnumerable<TimePriceDataResponse>?, string)> GetTimePriceDataTestAsync(Api api, Exchange exchange, string tradingSymbol, DateTime startDateTime, DateTime endDateTime, ChartInterval interval)
        {
            var (response, eMsg) = await api.MarketInfo.GetTimePriceDataAsync(exchange, tradingSymbol, startDateTime, endDateTime, interval);
            if (response is null)
            {
                await Console.Error.WriteLineAsync($"Failed GetTimePriceDataTestAsync : {eMsg}");
            }
            else
            {
                response.ToList().ForEach(candle => Console.WriteLine($"Passed GetTimePriceDataTestAsync : Get ohlcv for '{tradingSymbol} ({(int)interval}min.)' Rows[{response.Count()}][{candle.StartDateTime} - {candle.OpenPrice}, {candle.HighPrice}, {candle.LowPrice}, {candle.ClosePrice}, {candle.Volume}]"));
            }
            return (response, eMsg);
        }
        public async static Task<(IEnumerable<EodChartDataResponse>?, string)> GetEodChartDataTestAsync(Api api, Exchange exchange, string tradingSymbol, DateTime fromDate, DateTime toDate)
        {
            var (response, eMsg) = await api.MarketInfo.GetEodChartDataAsync(exchange, tradingSymbol, fromDate, toDate);
            if (response is null)
            {
                await Console.Error.WriteLineAsync($"Failed GetEodChartDataTestAsync : {eMsg}");
            }
            else
            {
                response.ToList().ForEach(candle => Console.WriteLine($"Passed GetEodChartDataTestAsync : Get ohlcv for '{tradingSymbol} (Daily)' Rows[{response.Count()}][{candle.StartDateTime} - {candle.OpenPrice}, {candle.HighPrice}, {candle.LowPrice}, {candle.ClosePrice}, {candle.Volume}]"));
            }
            return (response, eMsg);
        }

        public async static Task<(IEnumerable<ExchangeMessageResponse>?, string)> GetExchangeMessageTestAsync(Api api, Exchange exchange)
        {
            var (response, eMsg) = await api.MarketInfo.GetExchangeMessageAsync(exchange);
            if (response is null)
            {
                await Console.Error.WriteLineAsync($"Failed GetExchangeMessageTestAsync : {eMsg}");
            }
            else
            {
                response.ToList().ForEach(msg => Console.WriteLine($"Passed GetExchangeMessageTestAsync : Get broker messages [{msg.ExchangeTime} - {msg.ExchangeMessage}]"));
            }
            return (response, eMsg);
        }
        public async static Task<(IEnumerable<BrokerMessageResponse>?, string)> GetBrokerMessageTestAsync(Api api)
        {
            var (response, eMsg) = await api.MarketInfo.GetBrokerMessageAsync();
            if (response is null)
            {
                await Console.Error.WriteLineAsync($"Failed GetBrokerMessageTestAsync : {eMsg}");
            }
            else
            {
                response.ToList().ForEach(msg => Console.WriteLine($"Passed GetBrokerMessageTestAsync : Get broker messages [{msg.NorenTime} - {msg.MessageType}]"));
            }
            return (response, eMsg);
        }
    }
}
