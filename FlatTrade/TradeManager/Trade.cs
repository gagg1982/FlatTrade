using FlatTrade.AuthenticationManager;
using Common.Helpers;
using Common.Throttle;
using Common.Transport;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;


namespace FlatTrade.TradeManager
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="authentication"></param>
    /// <param name="httpClient"></param>
    public class Trade(Authentication authentication, RestHttpClient httpClient, ILoggerFactory loggerFactory)
    {
        private readonly ILogger<Trade> _logger = loggerFactory.CreateLogger<Trade>();
        private readonly Authentication _authentication = authentication;
        private readonly RestHttpClient _httpClient = httpClient;

        //[Throttle]
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async virtual Task<(IEnumerable<TradeBookResponse>?, string)> GetTradeBookAsync()
        {
            var (accessTokenResult, eMsg) = await _authentication.GetAccessTokenAsync();
            if (accessTokenResult is null)
            {
                eMsg = $"Cannot fetch trade book. {eMsg}";
                return (default, eMsg);
            }

            var tradeBookRequest = new TradeBookRequest
            {
                UserId = accessTokenResult.ClientCode, // Ensure UserId is set from access token
                AccountId = accessTokenResult.ClientCode
            };

            var serializedTradeBookRequest = JsonConvert.SerializeObject(tradeBookRequest);

            string requestParams = $"jData={serializedTradeBookRequest}&jKey={Uri.EscapeDataString(accessTokenResult.AccessToken)}";
            return await _httpClient.PostMessageAsync<IEnumerable<TradeBookResponse>, BaseErrorMessageResponse>(EndPoints.TradeBookUrl, requestParams);
        }

        //[Throttle]
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async virtual Task<(IEnumerable<PositionBookResponse>?, string)> GetPositionBookAsync()
        {
            var (accessTokenResult, eMsg) = await _authentication.GetAccessTokenAsync();
            if (accessTokenResult is null)
            {
                eMsg = $"Cannot fetch position book. {eMsg}";
                return (default, eMsg);
            }

            var positionBookRequest = new PositionBookRequest
            {
                UserId = accessTokenResult.ClientCode, // Ensure UserId is set from access token
                AccountId = accessTokenResult.ClientCode
            };

            var serializedPositionBookRequest = JsonConvert.SerializeObject(positionBookRequest);

            string requestParams = $"jData={serializedPositionBookRequest}&jKey={Uri.EscapeDataString(accessTokenResult.AccessToken)}";
            return await _httpClient.PostMessageAsync<IEnumerable<PositionBookResponse>, BaseErrorMessageResponse>(EndPoints.PositionBookUrl, requestParams);
        }

        //[Throttle]
        /// <summary>
        /// 
        /// </summary>
        /// <param name="productConversionRequest"></param>
        /// <returns></returns>
        public async virtual Task<(ProductConversionResponse?, string)> ProductConversionAsync(ProductConversionRequest productConversionRequest)
        {
            var (accessTokenResult, eMsg) = await _authentication.GetAccessTokenAsync();
            if (accessTokenResult is null)
            {
                eMsg = $"Cannot fetch access token for product conversion. {eMsg}";
                return (default, eMsg);
            }

            productConversionRequest.UserId = accessTokenResult.ClientCode; // Ensure UserId is set from access token
            productConversionRequest.AccountId = accessTokenResult.ClientCode; // Ensure AccountId is set from access token
            var serializedProductConversionRequest = JsonConvert.SerializeObject(productConversionRequest);

            string requestParams = $"jData={serializedProductConversionRequest}&jKey={Uri.EscapeDataString(accessTokenResult.AccessToken)}";
            return await _httpClient.PostMessageAsync<ProductConversionResponse, BaseErrorMessageResponse>(EndPoints.ProductConversionUrl, requestParams);
        }
    }
}
