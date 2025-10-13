using FlatTrade.AuthenticationManager;
using FlatTrade.Common.Helpers;
using FlatTrade.Common.Throttle;
using FlatTrade.Common.Transport;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
namespace FlatTrade.UserManager
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="authentication"></param>
    /// <param name="httpClient"></param>
    public class User(Authentication authentication, RestHttpClient httpClient, ILoggerFactory loggerFactory)
    {
        private readonly ILogger<User> _logger = loggerFactory.CreateLogger<User>();
        private readonly Authentication _authentication = authentication;
        private readonly RestHttpClient _httpClient = httpClient;

        //[Throttle]
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async virtual Task<(UserDetailsResponse?, string)> GetUserDetailsAsync()
        {
            var (accessTokenResult, eMsg) = await _authentication.GetAccessTokenAsync();
            if (accessTokenResult is null)
            {
                eMsg = "Cannot fetch user details. " + eMsg;
                return (default, eMsg); // Return null if access token is not available
            }

            var userDetails = new UserDetailsRequest { UserId = accessTokenResult.ClientCode };
            var serializedUserDetails = JsonConvert.SerializeObject(userDetails);

            string requestParams = $"jData={serializedUserDetails}&jKey={Uri.EscapeDataString(accessTokenResult.AccessToken)}";
            return await _httpClient.PostMessageAsync<UserDetailsResponse>(EndPoints.UserDetailsUrl, requestParams);
        }
    }
}
