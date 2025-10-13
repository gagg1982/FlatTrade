using FlatTrade.AuthenticationManager;
using FlatTrade.Common.Helpers;
using FlatTrade.Common.Throttle;
using FlatTrade.Common.Transport;
using FlatTrade.Common.Types.Base;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FlatTrade.HoldingsManager
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="authentication"></param>
    /// <param name="httpClient"></param>
    public class Holdings(Authentication authentication, RestHttpClient httpClient, ILoggerFactory loggerFactory)
    {
        private readonly ILogger<Holdings> _logger = loggerFactory.CreateLogger<Holdings>();
        private readonly Authentication _authentication = authentication;
        private readonly RestHttpClient _httpClient = httpClient;

      //[Throttle]
        /// <summary>
        /// 
        /// </summary>
        /// <param name="productType"></param>
        /// <returns></returns>
        public async virtual Task<(IEnumerable<HoldingsResponse>?, string)> GetHoldingsAsync()
        {
            var (accessTokenResult, eMsg) = await _authentication.GetAccessTokenAsync();
            if (accessTokenResult is null)
            {
                eMsg = $"Cannot fetch holdings. {eMsg}";
                return (default, eMsg);
            }

            var holdings = new HoldingsRequest
            {
                UserId = accessTokenResult.ClientCode,
                AccountId = accessTokenResult.ClientCode,
                ProductType = ProductType.Delivery
            };
            var serializedUserDetails = JsonConvert.SerializeObject(holdings);

            string requestParams = $"jData={serializedUserDetails}&jKey={Uri.EscapeDataString(accessTokenResult.AccessToken)}";

            return await _httpClient.PostMessageAsync<IEnumerable<HoldingsResponse>>(EndPoints.HoldingsUrl, requestParams);
        }
    }
}
