using FlatTrade;
using FlatTrade.Common.Types.Base;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StrategyEngine.BrokerData;
using StrategyEngine.Model;
using StrategyEngine.Strategy;

namespace StrategyEngine
{
    internal class StrategyProcessor
    {
        private readonly ILogger<StrategyProcessor> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly Api _api;        

        private readonly ContextAccessor _contextAccessor;
        private readonly IStrategy _strategy;

        internal StrategyProcessor(IConfiguration config, Api api, IStrategy strategy, ILoggerFactory loggerFactory)
        {
            _api = api;
            _strategy = strategy;
            _loggerFactory = loggerFactory ?? new LoggerFactory();
            _logger = _loggerFactory.CreateLogger<StrategyProcessor>();

            _logger.LogInformation("[Strategy] Initializing {startegyProcessor}", nameof(StrategyProcessor));

            List<SelectedSymbol> selectSymbolsFortrading = [
                new SelectedSymbol() { Exchange = Exchange.NSE, Token = 9552, TradingSymbol ="RVNL-EQ" }];

            List<Task> taskList = [];

            _contextAccessor = new(config, _api, _strategy.Process, _loggerFactory);
            _logger.LogInformation("[1] Initializing Securities...");

            taskList.Add(_contextAccessor.Security.UpdateSecurityInfo(selectSymbolsFortrading));

            _logger.LogInformation("[2] Initializing TradeBook");
            taskList.Add(_contextAccessor.Trade.UpdateTradeDetails());

            _logger.LogInformation("[3] Initializing Positions");
            taskList.Add(_contextAccessor.Position.UpdatePositions());

            _logger.LogInformation("[4] Initializing Holdings");
            taskList.Add(_contextAccessor.Holding.UpdateHoldingDetails());

            _logger.LogInformation("[5] Initializing CandlePrices");           
            taskList.Add(_contextAccessor.Candle.UpdateCandlesAsync(selectSymbolsFortrading));

            _logger.LogInformation("[6] Initializing OrderBook");            
            taskList.Add(_contextAccessor.Order.UpdateOrderBook());
            taskList.Add(_contextAccessor.Order.SubscribeOrderUpdates());

            _logger.LogInformation("[7] Initializing TouchLines");
            taskList.Add(_contextAccessor.TouchLine.SubscribeTouchLineAsync(selectSymbolsFortrading));

            _logger.LogInformation("[8] Initializing Quotes With Market Depth");
            taskList.Add(_contextAccessor.Quote.SubscribeQuoteAsync(selectSymbolsFortrading));

            //ReadConfigFile(configFile);
            //ReadRMSRules(configFile);
            
            Task.WhenAll(taskList).GetAwaiter().GetResult();
            ////===============================================================================================
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
