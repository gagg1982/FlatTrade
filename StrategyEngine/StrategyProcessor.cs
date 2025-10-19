using FlatTrade;
using FlatTrade.Common.Types.Base;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StrategyEngine.Model;
using StrategyEngine.Strategy;

namespace StrategyEngine
{
    internal class StrategyProcessor
    {
        internal delegate Task OnStrategyEvents(StrategyEvent strategyEvent);

        private readonly ILogger<StrategyProcessor> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly Api _api;

        private readonly OrderProcessor _orderProcessor;

        private readonly DirectFromServer _directFromServer;
        private readonly IStrategy _strategy;

        //subscription based
        private readonly OrderDetails _orderDetails;        
        private readonly TouchLineDetails _touchLineDetails;
        private readonly QuoteDetails _quoteDetails;
        //subscription based ends here

        private readonly OnStrategyEvents _onUpdates;

        private async Task OnUpdates(StrategyEvent strategyEvent)
        {
            var signal = await _strategy.Process(strategyEvent);
            if (signal is null)
                return;
            return;
            switch(signal.OutputDecision.OrderEventType)
            {
                case OrderEventType.Create:
                    if(signal.OutputDecision.CreateOrder is not null)
                        await _orderProcessor.CreateOrder(signal.OutputDecision.CreateOrder!);
                    break;
                case OrderEventType.Modify:
                    if (signal.OutputDecision.ModifyOrder is not null)
                        await _orderProcessor.ModifyOrder(signal.OutputDecision.ModifyOrder);
                    break;
                case OrderEventType.Cancel:
                    if (signal.OutputDecision.CancelOrder is not null)
                        await _orderProcessor.CancelOrder(signal.OutputDecision.CancelOrder);
                    break;
            }            
        }

        internal StrategyProcessor(IConfiguration config, Api api, IStrategy strategy, ILoggerFactory loggerFactory)
        {
            _api = api;
            _onUpdates = OnUpdates;
            _strategy = strategy;
            _loggerFactory = loggerFactory ?? new LoggerFactory();
            _logger = _loggerFactory.CreateLogger<StrategyProcessor>();
            _orderProcessor = new (_api, _loggerFactory);

            _logger.LogInformation("[Strategy] Initializing {startegyProcessor}", nameof(StrategyProcessor));

            List<SelectedSymbol> selectSymbolsFortrading = [
                new SelectedSymbol() { Exchange = Exchange.NSE, Token = 9552, TradingSymbol ="RVNL-EQ" }];

            List<Task> taskList = [];
            
            List<StrategyEngineEventType> supportedStrategyEngineEventTypes =  _strategy.GetStrategyEngineEventTypes().ToList() ?? [];
            var callbacks = supportedStrategyEngineEventTypes.ToDictionary(k => k, _ => _onUpdates) ?? [];            
            _directFromServer = new(_api, callbacks, _logger);
            _logger.LogInformation("[1] Initializing Securities...");
            // Making it blocking as Token is missing in OrderUpdates
             _directFromServer.UpdateSecurityInfo(selectSymbolsFortrading).GetAwaiter().GetResult();

            _logger.LogInformation("[2] Initializing TradeBook");
            taskList.Add(_directFromServer.UpdateTradeDetails());

            _logger.LogInformation("[3] Initializing Positions");
            taskList.Add(_directFromServer.UpdatePositions());

            _logger.LogInformation("[4] Initializing Holdings");
            taskList.Add(_directFromServer.UpdateHoldingDetails());

            _logger.LogInformation("[5] Initializing CandlePrices");
            IEnumerable<ChartInterval> priceIntervals = [ChartInterval.One, ChartInterval.Three, ChartInterval.Five,
                                                         ChartInterval.Ten, ChartInterval.Fifteen, ChartInterval.Thirty];
            taskList.Add(_directFromServer.UpdateCandles(selectSymbolsFortrading, priceIntervals));

            _logger.LogInformation("[6] Initializing OrderBook");
            _orderDetails = new(_api, _directFromServer, supportedStrategyEngineEventTypes.Contains(StrategyEngineEventType.Orders)? OnUpdates : null, _loggerFactory);
            taskList.Add(_orderDetails.UpdateOrderBook());
            taskList.Add(_orderDetails.SubscribeOrderUpdates());

            _logger.LogInformation("[7] Initializing TouchLines");
            _touchLineDetails = new(_api, supportedStrategyEngineEventTypes.Contains(StrategyEngineEventType.TouchLine) ? OnUpdates : null, _loggerFactory);
            taskList.Add(_touchLineDetails.SubscribeTouchLineAsync(selectSymbolsFortrading));

            _logger.LogInformation("[8] Initializing Quotes With Market Depth");
            _quoteDetails = new(_api, supportedStrategyEngineEventTypes.Contains(StrategyEngineEventType.Quotes) ? OnUpdates : null, _loggerFactory);
            taskList.Add(_quoteDetails.SubscribeQuoteAsync(selectSymbolsFortrading));           

            //ReadConfigFile(configFile);
            //ReadRMSRules(configFile);
            //FetchSymbolDetails().GetAwaiter().GetResult();

            //FetchTradeDetails().GetAwaiter().GetResult();
            //FetchPositionDetails().GetAwaiter().GetResult();
            //FetchCandleData().GetAwaiter().GetResult();

            ////===============================================================================================
        }

        //public async Task Execute()
        //{

        //}

        //public async Task FetchCandleData()
        //{
        //    foreach (var (symbol, symbolDetails) in symbolInfo)
        //    {
        //        var ok = await _api.Subscription.OrderSubscription.Subscribe(_userDetails.AccountId, );

        //        if (ok)
        //        {
        //            symbolDetails.Orders = scrip.First();
        //            Console.WriteLine($"Fetched details for symbol: {symbol}");
        //        }
        //        else
        //        {
        //            Console.WriteLine($"Failed to fetch details for symbol: {symbol}");
        //        }
        //    }
        //}
        //public async Task FetchPositionDetails()
        //{
        //    foreach (var (symbol, symbolDetails) in symbolInfo)
        //    {
        //        var ok = await _api.Subscription.OrderSubscription.Subscribe(_userDetails.AccountId, symbol);

        //        if (ok)
        //        {
        //            symbolDetails.Orders = scrip.First();
        //            Console.WriteLine($"Fetched details for symbol: {symbol}");
        //        }
        //        else
        //        {
        //            Console.WriteLine($"Failed to fetch details for symbol: {symbol}");
        //        }
        //    }
        //}
        //public async Task FetchTradeDetails()
        //{
        //    foreach (var (symbol, symbolDetails) in symbolInfo)
        //    {
        //        var ok = await _api.Subscription.OrderSubscription.Subscribe(_userDetails.AccountId, symbol);

        //        if (ok)
        //        {
        //            symbolDetails.Orders = scrip.First();
        //            Console.WriteLine($"Fetched details for symbol: {symbol}");
        //        }
        //        else
        //        {
        //            Console.WriteLine($"Failed to fetch details for symbol: {symbol}");
        //        }
        //    }
        //}

        //public async Task FetchSymbolDetails()
        //{
        //    foreach (var (symbol, symbolDetails) in symbolInfo)
        //    {
        //        var scrip = await _api.Scrips.GetScripAsync(_exchange.ToString(), symbol);

        //        if (scrip != null && scrip.Count == 1)
        //        {
        //            symbolDetails.SymbolData = scrip.First();
        //            Console.WriteLine($"Fetched details for symbol: {symbol}");
        //        }
        //        else
        //        {
        //            Console.WriteLine($"Failed to fetch details for symbol: {symbol}");
        //        }
        //    }
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
