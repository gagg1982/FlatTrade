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


Api api = new("9f0692ce836c4ac38fc46665496d1593",
              "http://localhost:9001/FlatTradeBroker/",
              "2025.4490c8154e9046c5aa0306b5c7f958baf6e3e1c66336f86b",
              string.Empty,
              loggerFactory,
              throttler); //apikey

IStrategy strategy = new TestStrategy(config, api, loggerFactory);
StrategyProcessor obj = new(config, api, strategy, loggerFactory);


Console.ReadKey();
//=====================================================================