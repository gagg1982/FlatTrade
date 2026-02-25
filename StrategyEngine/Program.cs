// See https://aka.ms/new-console-template for more information
using FlatTrade;
using Common.Helpers;
using Common.Throttle;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using StrategyEngine;
using StrategyEngine.OrderProcessors.BackTesting;
using StrategyEngine.RMS;
using StrategyEngine.Strategies.Test;

Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

var appConfigFile = FileHelper.GetConfigFile(args.Length == 1 ? args[0] : "","AppConfig.json");
var rmsConfigFile = FileHelper.GetConfigFile(args.Length == 2 ? args[1] : "", "RmsConfig.json");
var strategyConfigFile = FileHelper.GetConfigFile(args.Length == 3 ? args[2] : "", "StrategyConfig.json");

IConfiguration config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory) // Use AppContext.BaseDirectory for console apps
            .AddJsonFile(appConfigFile, optional: false, reloadOnChange: true)
            .AddJsonFile(rmsConfigFile, optional: false, reloadOnChange: true)
            .AddJsonFile(strategyConfigFile, optional: false, reloadOnChange: true)
            .Build();

Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(config)
                .Enrich.FromLogContext()
                .CreateLogger();

AppDomain.CurrentDomain.ProcessExit += (s, e) => Log.CloseAndFlush();

var loggerFactory = new LoggerFactory().AddSerilog(Log.Logger, dispose:true);

var throttler = new RateLimiterThrottleInterceptor(config, loggerFactory);

//=====================================================================

Api api = new(config["Api:Key"] ?? string.Empty,
              config["Api:RedirectUrl"] ?? string.Empty,
              config["Api:Secret"] ?? string.Empty,
              config["Api:AccessTokenFilePath"] ?? string.Empty,
              config["Api:Uid"] ?? string.Empty,
              config["Api:Password"] ?? string.Empty,
              config["Api:QrCode"] ?? string.Empty,
              loggerFactory,
              throttler);

// For Backtesting, just implement IOrderProcessor interface and u r done. 
// No need to change the strategy class.
var orderProcessor = new BackTestingOrderProcessor(config, api, loggerFactory);

var rms = new RmsManager(config, api, orderProcessor, loggerFactory);
rms.Register(new MaxQuantityPerOrder(config, api, orderProcessor, loggerFactory));
//rms.Register(new MaxLossPerDay(config, api, orderProcessor, loggerFactory));
//rms.Register(new MaxLossPerOrder(config, api, orderProcessor, loggerFactory));
rms.Register(new SufficientBalance(config, api, orderProcessor, loggerFactory));


var strategy = new TestStrategy(config, api, loggerFactory);
StrategyProcessor strategyProcessor = new(config, api, rms, strategy, orderProcessor, loggerFactory);

Console.ReadKey();

await api.DisposeAsync();
await strategy.DisposeAsync();
await strategyProcessor.DisposeAsync();
//=====================================================================