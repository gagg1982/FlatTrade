using DailyRunner;
using FlatTrade;
using FlatTrade.Common.Helpers;
using FlatTrade.Common.Throttle;
using FlatTrade.Common.Types.Base;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;

//=====================================================================

Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

var configFile = FileHelper.GetConfigFile(args.Length == 1 ? args[0] : "");

IConfiguration config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory) // Use AppContext.BaseDirectory for console apps
            .AddJsonFile(configFile, optional: false, reloadOnChange: true)
            .Build();

Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(config)
                .CreateLogger();

AppDomain.CurrentDomain.ProcessExit += (s, e) => Log.CloseAndFlush();

var loggerFactory = new LoggerFactory().AddSerilog(Log.Logger);
//var throttler = config.GetSection("Api:Throttling:RateLimiter").Get<RateLimiterThrottleSetting>();
//var throttler = new OutstandingThrottleInterceptor(config);
var throttler = new RateLimiterThrottleInterceptor(config, loggerFactory);

//=====================================================================
//using var playwright = await Playwright.CreateAsync();

//await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
//{
//    Headless = true,
//    Args = new[]
//    {
//        "--disable-http2",
//        "--disable-quic",
//        "--disable-blink-features=AutomationControlled"
//    }
//});

//var context = await browser.NewContextAsync(new BrowserNewContextOptions
//{
//    UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120 Safari/537.36"
//});


//await context.AddInitScriptAsync("Object.defineProperty(navigator, 'webdriver', { get: () => undefined })");

//var page = await context.NewPageAsync();

//// Visit homepage to establish cookies
//await page.GotoAsync("https://www.nseindia.com", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });


//page.RequestFinished += (_, request) =>
//{
//    Console.WriteLine($"Request: {request.Url}");
//    foreach (var h in request.Headers)
//        Console.WriteLine($"{h.Key}: {h.Value}");
//};

//// Now hit API
//var response = await page.GotoAsync(
//    "https://www.nseindia.com/api/corporates-corporateActions?index=equities&from_date=01-01-2025&to_date=27-11-2025",
//    new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

//var json = await response.TextAsync();
//Console.WriteLine(json);

//=====================================================================

Api api = new(config["Api:Key"] ?? String.Empty,
              config["Api:RedirectUrl"] ?? String.Empty,
              config["Api:Secret"] ?? String.Empty,
              config["Api:AccessTokenFilePath"] ?? String.Empty,
              loggerFactory,
              throttler); //apikey

//=====================================================================

//var corporateActionsGenerator = new CorporateActionsGenerator(api, config, loggerFactory);
//await corporateActionsGenerator.GenerateAndLoad();

//=====================================================================
var bookKeeping = new BookKeeping(api, config, loggerFactory);  // for orderbook, tradebook, singleorderhistory
await bookKeeping.GenerateAndLoad();

//=====================================================================

var exchangeGenerator = new ExchangeGenerator(api, config, loggerFactory);
await exchangeGenerator.GenerateAndLoad();

//=====================================================================

var stocksGenerator = new StocksGenerator(api, config, loggerFactory);
var listOfExchangeTokenSymbolTuple = await stocksGenerator.GenerateAndLoad();
//IEnumerable<(Exchange, long, string)> listOfExchangeTokenSymbolTuple = [(Exchange.NSE, 9552, "RVNL-EQ")];
//=====================================================================

var stocksOhlcvGenerator = new StocksOhlcvGenerator(api, config, listOfExchangeTokenSymbolTuple, loggerFactory);
await stocksOhlcvGenerator.GenerateAndLoad();

//=====================================================================
var fillMissingOhlcv = new FillMissingOhlcv(api, config, loggerFactory);
await fillMissingOhlcv.GenerateAndLoad();

//=====================================================================