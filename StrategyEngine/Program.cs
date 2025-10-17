// See https://aka.ms/new-console-template for more information
using FlatTrade;
using FlatTrade.Common.Helpers;
using FlatTrade.Common.Throttle;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using StrategyEngine;
using StrategyEngine.Strategy;

//Subscribe to orders
//Subscribe to trades
//Subscribe to Quotes
//Get All Orders
//Get All Open Positions
//Get Available balance.
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
var throttler = new OutstandingThrottleInterceptor(config, loggerFactory);

//=====================================================================


Api api = new(config["Api:Key"] ?? String.Empty,
              config["Api:RedirectUrl"] ?? String.Empty,
              config["Api:Secret"] ?? String.Empty,
              config["Api:AccessTokenFilePath"] ?? String.Empty,
              loggerFactory,
              throttler); //apikey

IStrategy strategy = new TestStrategy(config, api, loggerFactory);
StrategyProcessor obj = new(config, api, strategy, loggerFactory);


Console.ReadKey();
//=====================================================================