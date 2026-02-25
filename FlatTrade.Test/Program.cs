namespace FlatTrade.Test
{
    using FlatTrade;
    using Microsoft.Extensions.Logging;
    using Serilog;
    using System;

    internal static class Program
    {
        public static async Task<int> Main(string[] _)
        {
            Log.Logger = new LoggerConfiguration()
           .MinimumLevel.Debug()
           .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}")
           .Enrich.FromLogContext()
           .CreateLogger();

            var loggerFactory = new LoggerFactory().AddSerilog();

            Api api = new("9f0692ce836c4ac38fc46665496d1593",
                          "http://localhost:9001/FlatTradeBroker/",
                          "",
                          "",
                          "",
                          "",
                          string.Empty,
                          loggerFactory,
                          null); //apikey

            await AuthenticationTest.Execute(api);
            await OrderTest.Execute(api);

            await AlertTest.Execute(api);
            await FundsTests.Execute(api);
            await HoldingsTest.Execute(api);
            await LimitsTest.Execute(api);
            await ScripTest.Execute(api);
            await MarketInfoTest.Execute(api);


            //-----------------------------------------------------------------------------------------------
            //var userDetails = await api.User.GetUserDetailsAsync();
            //if (userDetails == null)
            //{
            //    return -2;
            //}

            ////-----------------------------------------------------------------------------------------------
            //var holdings = await api.Holdings.GetHoldingsAsync("C");
            //if (holdings == null)
            //{
            //    return -10;
            //}

            //var limits = await api.Limits.GetLimitsAsync();
            //if (limits == null)
            //{
            //    return -11;
            //}

            //var payOutReport = await api.Funds.GetPayOutReportAsync(DateTime.Now.AddDays(-75), DateTime.Now);
            //if (payOutReport == null)
            //{
            //    return -12;
            //}

            //var payInReport = await api.Funds.GetPayInReportAsync(DateTime.Now.AddDays(-80), DateTime.Now);
            //if (payInReport == null)
            //{
            //    return -13;
            //}

            //var cancelPayOut = await api.Funds.CancelPayOutAsync("20251740003586");
            //if (cancelPayOut == null)
            //{
            //    return -14;
            //}

            ////var fundsPayOutRequest = await api.Funds.FundsPayOutRequestAsync("test remarks ", 100);
            ////if (fundsPayOutRequest == null)
            ////{
            ////    return -15;
            ////}

            //var maxPayOutAmount = await api.Funds.GetMaxPayoutAmountAsync();
            //if (maxPayOutAmount == null)
            //{
            //    return -16;
            //}

            ////-----------------------------------------------------------------------------------------------
            //var indexList = await api.MarketInfo.GetIndexListAsync("NSE");
            //if (indexList == null)
            //{
            //    return -17;
            //}

            //var topList = await api.MarketInfo.GetTopListAsync("NSE", "Basket", "crit", "T");
            //if (topList == null)
            //{
            //    return -18;
            //}

            //var topListName = await api.MarketInfo.GetTopListNamesAsync("NSE");
            //if (topListName == null)
            //{
            //    return -19;
            //}

            //var exchangeMessage = await api.MarketInfo.GetExchangeMessageAsync("NSE");
            //if (exchangeMessage == null)
            //{
            //    return -20;
            //}

            //var brokerMessage = await api.MarketInfo.GetBrokerMessageAsync();
            //if (brokerMessage == null)
            //{
            //    return -21;
            //}

            //var spanCalculator = await api.MarketInfo.SpanCalculatorAsync([new PositionRequest { BuyQuantity=100,
            //                                                                                      Exchange ="NSE",
            //                                                                                      ExpiryDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-60)),
            //                                                                                       InstrumentName = "ETERNAL",
            //                                                                                     NetQuantity =50,
            //                                                                                     OptionType = "P",
            //                                                                                     SellQuantity = 50,
            //                                                                                     StrikePrice = 100.18,
            //                                                                                     SymbolName = "ETERNAL"}]);
            //if (spanCalculator == null)
            //{
            //    return -22;
            //}

            ////api.MarketInfo.GetOptionGreekAsync;
            ////api.MarketInfo.GetTimePriceDataAsync;
            ////api.MarketInfo.GetOptionChainAsync;
            ////api.MarketInfo.GetEodChartDataAsync;

            ////-----------------------------------------------------------------------------------------------
            //ConcurrentDictionary<string, List<ScripDetails>> allInstruments = new();
            //foreach (var exchange in userDetails.Exchanges.Where(exch => exch == "NSE"))
            //{
            //    if (exchange == null)
            //    {
            //        return -3; // No exchange found
            //    }

            //    var scripDetails = await api.Scrips.GetAllScripAsync(exchange);
            //    if (scripDetails == null)
            //    {
            //        return -4;
            //    }

            //    allInstruments.TryAdd(exchange, scripDetails);
            //}

            //if (!allInstruments.TryGetValue("NSE", out List<ScripDetails>? scrips) || scrips == null)
            //{
            //    return -7;
            //}

            //foreach (var exchange in userDetails.Exchanges.Where(exch => exch == "NSE"))
            //{
            //    var scripDetails = await api.Scrips.GetScripAsync(exchange, "A");
            //    if (scripDetails == null)
            //    {
            //        return -5;
            //    }

            //    scripDetails = await api.Scrips.GetScripAsync(exchange, "B");
            //    if (scripDetails == null)
            //    {
            //        return -6;
            //    }
            //}

            ////-----------------------------------------------------------------------------------------------
            //// GettingQuotes
            //ConcurrentDictionary<string, List<QuotesResponse>> allInstrumentQuotes = new();
            //var instrList = new List<QuotesResponse>();
            //foreach (var instr in allInstruments.Where(kvp => kvp.Key == "NSE"))
            //{
            //    foreach (var scrip in instr.Value.Where(v => v.Symbol.StartsWith("ETERNAL")))
            //    {
            //        var quotes = await api.Scrips.GetQuotesAsync(instr.Key, scrip.SymbolToken);
            //        if (quotes == null)
            //        {
            //            Console.Error.WriteLine($"Instr [{scrip.Symbol}:{scrip.SymbolToken}]  ");
            //            continue;
            //        }
            //        instrList.Add(quotes);
            //    }
            //    allInstrumentQuotes.TryAdd(instr.Key, instrList);
            //}

            ////------------------------------------------------------------------------------------------------
            //foreach (var kvp in allInstruments.Where(kvp => kvp.Key == "NSE"))
            //{
            //    foreach (var scrip in kvp.Value.Where(v => v.Symbol.StartsWith("ETERNAL")))
            //    {
            //        var res = await api.Subscription.OrderSubscription.Subscribe(userDetails.AccountId,
            //            new List<Task<EventHandler<string>?>> {
            //                Task.FromResult<EventHandler<string>?>(Client_OnMessageReceived),
            //               // Task.FromResult<EventHandler<string>?>(Client_OnMessageReceived2),
            //              //  Task.FromResult<EventHandler<string>?>(Client_OnMessageReceived3)
            //            });

            //        var a = res;
            //        await api.Subscription.TouchlineSubscription.Subscribe(
            //            new List<KeyValuePair<string, string>> { new(kvp.Key, scrip.SymbolToken.ToString()) },
            //            new List<Task<EventHandler<string>?>> {
            //                Task.FromResult<EventHandler<string>?>(Client_OnMessageReceived),
            //               // Task.FromResult<EventHandler<string>?>(Client_OnMessageReceived2),
            //               // Task.FromResult<EventHandler<string>?>(Client_OnMessageReceived3)
            //            });

            //        await api.Subscription.QuoteSubscription.Subscribe(
            //            new List<KeyValuePair<string, string>> { new(kvp.Key, scrip.SymbolToken.ToString()) },
            //            new List<Task<EventHandler<string>?>>
            //            {
            //                Task.FromResult<EventHandler<string>?>(Client_OnMessageReceived),
            //               // Task.FromResult<EventHandler<string>?>(Client_OnMessageReceived2),
            //               // Task.FromResult<EventHandler<string>?>(Client_OnMessageReceived3)
            //            });

            //        //await api.Subscription.OrderSubscription.Unsubscribe(
            //        //    new List<Task<EventHandler<string>?>>
            //        //    {
            //        //        Task.FromResult<EventHandler<string>?>(Client_OnMessageReceived),
            //        //       // Task.FromResult<EventHandler<string>?>(Client_OnMessageReceived3)
            //        //    });

            //        //await api.Subscription.TouchlineSubscription.Unsubscribe(
            //        //    new List<KeyValuePair<string, string>> { new(kvp.Key, scrip.SymbolToken.ToString()) },
            //        //    new List<Task<EventHandler<string>?>>
            //        //    {
            //        //        Task.FromResult<EventHandler<string>?>(Client_OnMessageReceived),
            //        //        //Task.FromResult<EventHandler<string>?>(Client_OnMessageReceived2),
            //        //        //Task.FromResult<EventHandler<string>?>(Client_OnMessageReceived3)
            //        //    });                    

            //        //await api.Subscription.QuoteSubscription.Unsubscribe(
            //        //    new List<KeyValuePair<string, string>> { new(kvp.Key, scrip.SymbolToken.ToString()) },
            //        //     new List<Task<EventHandler<string>?>>
            //        //    {
            //        //        Task.FromResult<EventHandler<string>?>(Client_OnMessageReceived),
            //        //        //Task.FromResult<EventHandler<string>?>(Client_OnMessageReceived2),
            //        //        //Task.FromResult<EventHandler<string>?>(Client_OnMessageReceived2)
            //        //    });                   

            //        //await api.Subscription.QuoteSubscription.Unsubscribe(
            //        //    new List<KeyValuePair<string, string>> { new(kvp.Key, "1000") },
            //        //    new List<Task<EventHandler<string>?>>
            //        //    {
            //        //        Task.FromResult<EventHandler<string>?>(Client_OnMessageReceived),
            //        //       // Task.FromResult<EventHandler<string>?>(Client_OnMessageReceived2),
            //        //       // Task.FromResult<EventHandler<string>?>(Client_OnMessageReceived3)
            //        //    });
            //    }
            //}

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            return 0;
        }


        //private static void Client_OnMessageReceived3(object? sender, string message)
        //{
        //    // IDE0060: Remove unused parameter 'sender'
        //    // Fix: Remove the unused 'sender' parameter as it is not used in the method body.
        //    Console.WriteLine($"\n[App Logic]: Message processed: '{message}'");
        //    return;
        //}
    }
}
