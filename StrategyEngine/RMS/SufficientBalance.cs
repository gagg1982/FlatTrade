using FlatTrade;
using FlatTrade.MarketInfoManager;
using FlatTrade.OrderManager;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StrategyEngine.Model;

namespace StrategyEngine.RMS
{
    internal class SufficientBalance : IRMS
    {
        private readonly IConfiguration _config;
        private readonly Api _api;
        private readonly ILogger<SufficientBalance> _logger;
        private readonly decimal _initialCashAllocated = 0;

        public SufficientBalance(Api api, IConfiguration config, ILoggerFactory loggerFactory)
        {
            _api = api;
            _config = config;
            _logger = loggerFactory.CreateLogger<SufficientBalance>();
        }

        public async Task<bool> IsValidationSucceeded(StrategySignal strategySignal)
        {
            OrderMarginResponse orderMargin = new();
            string msg = string.Empty;
            //var (orderMargin,msg) = await _api.Order.GetOrderMarginAsync( new OrderMarginRequest());
            if (orderMargin is null)
            {
                _logger.LogWarning("RMS:SufficientBalance:ValidationFailed: Unable to get OrderMargin info. Error: {0}", msg);
                return false;
            }
            
            BrokerageResponse brokerageResponse = new();
            string msg1 = string.Empty;
            //var (brokerage, msg1) = await _api.MarketInfo.GetBrokerageAsync();
            if (brokerageResponse is null)
            {
                _logger.LogWarning("RMS:SufficientBalance:ValidationFailed: Unable to get brokerage info. Error: {0}", msg1);
                return false;
            }

            var availableAfterOrderFullFillment = _initialCashAllocated - orderMargin.TotalMarginUsed + brokerageResponse.TotalCharges;
            if (availableAfterOrderFullFillment < 0 )
            {
                _logger.LogWarning("RMS:SufficientBalance:ValidationFailed: Insufficient balance for order. Required: {0}, Available: {1}", orderMargin.OrderMargin + brokerageResponse.TotalCharges, _initialCashAllocated - orderMargin.MarginUsedPreviously);
                return false;
            }
            return true;
        }
    }
}
