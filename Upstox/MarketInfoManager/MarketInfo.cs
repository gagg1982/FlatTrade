using Common.Helpers;
using Common.Throttle;
using Common.Types;
using Common.Transport;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Upstox.Types.Base;

namespace Upstox.MarketInfoManager
{    
    public class MarketInfo(RestHttpClient httpClient, ILoggerFactory loggerFactory)
    {
        private readonly ILogger<MarketInfo> _logger = loggerFactory.CreateLogger<MarketInfo>();
        private readonly RestHttpClient _httpClient = httpClient;      

        /// <summary>
        /// 
        /// </summary>
        /// <param name="exchange"></param>
        /// <param name="tradngSymbol"></param>
        /// <param name="startDateTime"></param>
        /// <param name="endDateTime"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        public async virtual Task<(TimePriceDataResponse?, string)> GetTimePriceDataAsync(Exchange exchange, string isin, DateTime startDateTime, DateTime endDateTime, ChartInterval interval)
        {           
            return await _httpClient.GetMessageAsync<TimePriceDataResponse, BaseErrorMessage>(EndPoints.GetOneMinuteTimePriceDataUrl(startDateTime, endDateTime, isin, exchange));
        }

      //[Throttle]
        /// <summary>
        /// 
        /// </summary>
        /// <param name="exchange"></param>
        /// <param name="tradingSymbol"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <returns></returns>
        public async virtual Task<(TimePriceDataResponse?, string)> GetEodChartDataAsync(Exchange exchange, string isin, DateTime fromDate, DateTime toDate)
        {
            return await _httpClient.GetMessageAsync<TimePriceDataResponse,BaseErrorMessage>(EndPoints.GetEodChartDataUrl(fromDate, toDate, isin, exchange));
        }
   
    }
}
