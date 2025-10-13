using FlatTrade.AuthenticationManager;
using FlatTrade.Common.Helpers;
using FlatTrade.Common.Throttle;
using FlatTrade.Common.Transport;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FlatTrade.LimitsManager
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="authentication"></param>
    /// <param name="httpClient"></param>
    public class Limits(Authentication authentication, RestHttpClient httpClient, ILoggerFactory loggerFactory)
    {
        private readonly ILogger<Limits> _logger = loggerFactory.CreateLogger<Limits>();
        private readonly Authentication _authentication = authentication;
        private readonly RestHttpClient _httpClient = httpClient;

        //[Throttle]
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async virtual Task<(LimitsResponse?, string)> GetLimitsAsync()
        {
            var (accessTokenResult, eMsg) = await _authentication.GetAccessTokenAsync();
            if (accessTokenResult is null)
            {
                eMsg = $"Cannot fetch limits. {eMsg}";
                return (default, eMsg);
            }

            var limits = new LimitsRequest
            {
                UserId = accessTokenResult.ClientCode,
                AccountId = accessTokenResult.ClientCode
            };
            var serializedUserDetails = JsonConvert.SerializeObject(limits);

            string requestParams = $"jData={serializedUserDetails}&jKey={Uri.EscapeDataString(accessTokenResult.AccessToken)}";
            return await _httpClient.PostMessageAsync<LimitsResponse>(EndPoints.LimitsUrl, requestParams);
        }
    }
}
