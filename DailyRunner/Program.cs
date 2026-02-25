using DailyRunner;
using Ft = DailyRunner.FlatTrade;
using Us = DailyRunner.Upstox;
using Common.Helpers;
using Common.Throttle;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;

//=====================================================================

Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

var configFile = FileHelper.GetConfigFile(args.Length == 1 ? args[0] : "", "AppConfig.json");

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
FlatTrade.Api ftApi = new(config["Api:Key"] ?? String.Empty,
              config["Api:RedirectUrl"] ?? String.Empty,
              config["Api:Secret"] ?? String.Empty,
              config["Api:AccessTokenFilePath"] ?? String.Empty,
              config["Api:Uid"] ?? String.Empty,
              config["Api:Password"] ?? String.Empty,
              config["Api:QrCode"] ?? String.Empty,
              loggerFactory,
              throttler); //apikey

//=====================================================================

var bookKeeping = new BookKeeping(ftApi, config, loggerFactory);  // for orderbook, tradebook, singleorderhistory
await bookKeeping.GenerateAndLoad();

//=====================================================================

var exchangeGenerator = new ExchangeGenerator(ftApi, config, loggerFactory);
await exchangeGenerator.GenerateAndLoad();

//=====================================================================

var stocksGenerator = new Ft.StocksGenerator(ftApi, config, loggerFactory);
var listOfExchangeTokenSymbolTuple = await stocksGenerator.GenerateAndLoad();
//IEnumerable<(string, long, string, string)> listOfExchangeTokenSymbolTuple = [("NSE", 9552, "RVNL-EQ", "INE415G01027")];
//=====================================================================

var stocksOhlcvGeneratorFt = new Ft.StocksOhlcvGenerator(ftApi, config, listOfExchangeTokenSymbolTuple, loggerFactory);
await stocksOhlcvGeneratorFt.GenerateAndLoad();

//=====================================================================
var fillMissingOhlcvFt = new Ft.FillMissingOhlcv(ftApi, config, loggerFactory);
await fillMissingOhlcvFt.GenerateAndLoad();

////=====================================================================
//Upstox.Api usApi = new(
//              loggerFactory,
//              throttler); //apikey

//var stocksOhlcvGeneratorUs = new Us.StocksOhlcvGenerator(usApi, config, listOfExchangeTokenSymbolTuple, loggerFactory);
//await stocksOhlcvGeneratorUs.GenerateAndLoad();

////=====================================================================
//var fillMissingOhlcvUs = new Us.FillMissingOhlcv(usApi, config, loggerFactory);
//await fillMissingOhlcvUs.GenerateAndLoad();

////=====================================================================