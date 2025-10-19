// See https://aka.ms/new-console-template for more information
using FlatTrade;
using FlatTrade.Common.Helpers;
using FlatTrade.Common.Throttle;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using StrategyEngine;
using StrategyEngine.Model;
using StrategyEngine.RMS;
using StrategyEngine.Strategy;


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

Api api = new(config["Api:Key"] ?? String.Empty,
              config["Api:RedirectUrl"] ?? String.Empty,
              config["Api:Secret"] ?? String.Empty,
              config["Api:AccessTokenFilePath"] ?? String.Empty,
              loggerFactory,
              throttler); //apikey

var rms = new RmsManager();
rms.Register(new MaxLossLimitPerDay());
rms.Register(new MaxLossLimitPerTrade());

IEnumerable<StrategyEngineEventType> strategyEventTypesSubscription = [StrategyEngineEventType.Quotes,
                                                                      StrategyEngineEventType.TouchLine,
                                                                      StrategyEngineEventType.Candles,
                                                                      StrategyEngineEventType.Holdings,
                                                                      StrategyEngineEventType.Positions,
                                                                      StrategyEngineEventType.Securities,
                                                                      StrategyEngineEventType.Trades,
                                                                      StrategyEngineEventType.Orders];

IStrategy strategy = new TestStrategy(config, api, rms, strategyEventTypesSubscription, loggerFactory);
StrategyProcessor strategyProcessor = new(config, api, strategy, loggerFactory);

Console.ReadKey();
//=====================================================================