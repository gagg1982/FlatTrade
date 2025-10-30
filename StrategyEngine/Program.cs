// See https://aka.ms/new-console-template for more information
using FlatTrade;
using FlatTrade.Common.Helpers;
using FlatTrade.Common.Throttle;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using StrategyEngine;
using StrategyEngine.OrderProcessors;
using StrategyEngine.RMS;
using StrategyEngine.Strategies;
using StrategyEngine.Strategies.Test;

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

var loggerFactory = new LoggerFactory().AddSerilog();
var throttler = new RateLimiterThrottleInterceptor(config, loggerFactory);

//=====================================================================

Api api = new(config["Api:Key"] ?? string.Empty,
              config["Api:RedirectUrl"] ?? string.Empty,
              config["Api:Secret"] ?? string.Empty,
              config["Api:AccessTokenFilePath"] ?? string.Empty,
              loggerFactory,
              throttler);

var rms = new RmsManager();
    rms.Register(new MaxLossLimitPerDay());
    rms.Register(new MaxLossLimitPerTrade());
    rms.Register(new SufficientBalance(api, config, loggerFactory));

// For Backtesting, just implement IOrderProcessor interface and u r done. 
// No need to change the strategy class.
var orderProcessor = new OrderProcessor(api, loggerFactory);

IStrategy strategy = new TestStrategy(config, api, rms, orderProcessor, loggerFactory);
StrategyProcessor strategyProcessor = new(config, api, strategy, loggerFactory);

Console.ReadKey();
//=====================================================================