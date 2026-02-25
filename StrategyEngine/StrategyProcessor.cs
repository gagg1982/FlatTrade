using FlatTrade;
using FlatTrade.Types.Base;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StrategyEngine.BrokerData;
using StrategyEngine.Model;
using StrategyEngine.OrderProcessors;
using StrategyEngine.RMS;
using StrategyEngine.Strategies;

namespace StrategyEngine
{
    internal class StrategyProcessor : IAsyncDisposable, IDisposable
    {
        private bool _disposed = false;
        private readonly ILogger<StrategyProcessor> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly Api _api;        

        private readonly ContextAccessor _contextAccessor;
        private readonly IStrategy _strategy;
        private readonly IOrderProcessor _orderProcessor;
        private readonly IRms _rms;
        
        private List<Task> _tasks = [];
        internal StrategyProcessor(IConfiguration config,
                                   Api api,
                                   IRms rms,
                                   IStrategy strategy,
                                   IOrderProcessor orderProcessor,
                                   ILoggerFactory loggerFactory)
        {
            _api = api;
            _strategy = strategy;
            _orderProcessor = orderProcessor;
            _rms = rms;
            _loggerFactory = loggerFactory ?? new LoggerFactory();
            _logger = _loggerFactory.CreateLogger<StrategyProcessor>();

            _logger.LogInformation("[Strategy] Initializing {startegyProcessor}", nameof(StrategyProcessor));

            List<SelectedSymbol> selectSymbolsFortrading = [
                new SelectedSymbol() {Exchange = Exchange.NSE, Token = 9552, TradingSymbol ="RVNL-EQ"},
                new SelectedSymbol() { Exchange = Exchange.NSE, Token = 5097, TradingSymbol ="ETERNAL-EQ"},
                new SelectedSymbol() { Exchange = Exchange.NSE, Token = 27066, TradingSymbol ="SWIGGY-EQ"}];

            //important to attach to handler before creation of object
            //so that as soon as object is created and subscription started, events will not miss
            ContextAccessor.RegisterHandler([Process, _rms.OnUpdate, _orderProcessor.OnUpdate]);

            _contextAccessor = new(config, _api, _loggerFactory);
            _logger.LogInformation("[1] Initializing Securities...");
            _tasks.Add(_contextAccessor.Security.UpdateSecurityInfo(selectSymbolsFortrading));

            _logger.LogInformation("[2] Initializing TradeBook");
            _tasks.Add(_contextAccessor.Trade.UpdateTradeDetails());

            _logger.LogInformation("[3] Initializing Positions");
            _tasks.Add(_contextAccessor.Position.UpdatePositions());

            _logger.LogInformation("[4] Initializing Holdings");
            _tasks.Add(_contextAccessor.Holding.UpdateHoldingDetails());

            _logger.LogInformation("[5] Initializing CandlePrices");
            _tasks.Add(_contextAccessor.Candle.GetHistoricCandlesFromServerAsync(selectSymbolsFortrading));

            _logger.LogInformation("[6] Initializing OrderBook");
            _tasks.Add(_contextAccessor.Order.UpdateOrderBook());

            _logger.LogInformation("[7] Initializing Subscriptions");            
            _logger.LogInformation("   [7-A] Initializing Order Updates");
            _tasks.Add(_contextAccessor.Order.SubscribeOrderUpdates());            
            _logger.LogInformation("   [7-B] Initializing TouchLine Updates");
            _tasks.Add(_contextAccessor.TouchLine.SubscribeTouchLineAsync(selectSymbolsFortrading));            
            _logger.LogInformation("   [7-C] Initializing Quote Updates");
            _tasks.Add(_contextAccessor.Quote.SubscribeQuoteAsync(selectSymbolsFortrading));

            //ReadConfigFile(configFile);
            //ReadRMSRules(configFile);
            ////===============================================================================================
        }

        private bool IsInvalid(StrategySignal? signal) => signal is null;

        public async Task Process(object? obj)
        {
            if (obj is null)
            {
                _logger.LogWarning("{0}: received null object to process. ",GetType().Name);
                return;
            }
            try
            {
                var signal = await _strategy.Process(obj);                
                if (IsInvalid(signal))
                    return;

                if (!await _rms.IsValidationSucceeded(signal!))
                    return;

                //return;
                switch (signal!.OutputDecision)
                {
                    case OutputDecision.Create create:
                        _tasks.Add(_orderProcessor.CreateOrder(signal.StrategyName, create.Order));
                        break;

                    case OutputDecision.Modify modify:
                        _tasks.Add(_orderProcessor.ModifyOrder(signal.StrategyName, modify.Order));
                        break;

                    case OutputDecision.Cancel cancel:
                        _tasks.Add(_orderProcessor.CancelOrder(signal.StrategyName,cancel.Order));
                        break;

                    default:
                        _logger.LogError(
                            "{Type}: StrategyName:[{Strategy}] Invalid OutputDecision type [{DecisionType}]",
                            GetType().Name,
                            signal.StrategyName,
                            signal.OutputDecision.GetType().Name);
                        break;
                }               
            }
            catch (Exception ex)
            {
                _logger.LogInformation("{0}: Error: {1}", GetType().Name, ex);
            }
            return;
        }

        public void Dispose()
        {
            DisposeAsyncCore().AsTask().GetAwaiter().GetResult(); // Safe sync fallback
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore();
            GC.SuppressFinalize(this);
        }

        private async ValueTask DisposeAsyncCore()
        {
            if (_disposed)
                return;

            _disposed = true;

            ContextAccessor.UnRegisterHandler([Process, _rms.OnUpdate, _orderProcessor.OnUpdate]);
            await _contextAccessor.DisposeAsync();
            await Task.WhenAll(_tasks);
            _logger.LogInformation("{0}: Disposed gracefully", GetType().Name);
            // Dispose other sync-only resources here (e.g., timers, files)
        }

        //public async Task Execute()
        //{

        //}


        //public void ReadConfigFile(string configFile)
        //{
        //    var configRoot = new ConfigurationBuilder().AddJsonFile(configFile, false, true).Build();
        //    if (configRoot == null)
        //    {
        //        ArgumentNullException argumentNullException = new(nameof(configFile), "Configuration file not found or invalid.");
        //        throw argumentNullException;
        //    }

        //    _exchange = configRoot.GetValue<Exchange>("Exchange");

        //    if (string.IsNullOrEmpty(_exchange.ToString()))
        //    {
        //        throw new ArgumentNullException(nameof(configFile), "Exchange not found in the configuration file.");
        //    }

        //    configRoot.GetValue<List<string>>("Symbol")?.ForEach(symbol =>
        //    {
        //        symbolInfo.TryAdd(symbol, new());
        //    });

        //    if (symbolInfo.Count == 0)
        //    {
        //        throw new ArgumentException("No symbols found in the configuration file.", nameof(configFile));
        //    }
        //}

        //public void ReadRMSRules(string configFile)
        //{
        //    var configRoot = new ConfigurationBuilder().AddJsonFile(configFile, false, true).Build();
        //    if (configRoot == null)
        //    {
        //        throw new ArgumentNullException(nameof(configRoot), "Configuration file not found or invalid.");
        //    }

        //    configRoot.GetSection("RMSRules").Bind(_rmsRules);

        //    if (_rmsRules == null)
        //    {
        //        throw new ArgumentNullException(nameof(_exchange), "RMSRules not found in the configuration file.");
        //    }
        //}
    }
}
