using FlatTrade.AuthenticationManager;
using FlatTrade.Common.Helpers;
using FlatTrade.Common.Throttle;
using FlatTrade.Common.Transport;
using FlatTrade.Common.Types.Base;
using FlatTrade.SubscriptionManager.Helper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace FlatTrade.ScripManager
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="authentication"></param>
    /// <param name="httpClient"></param>
    public class Scrips(Authentication authentication, RestHttpClient httpClient, ILoggerFactory loggerFactory)
    {
        private readonly ILogger<Scrips> _logger = loggerFactory.CreateLogger<Scrips>();
        public static IEnumerable<string> InstrumentsPrefix { get; set; } = ["0","1","2","3","4","5","6","7","8","9",
                                                                      "A","B","C","D","E","F","G","H","I","J",
                                                                      "K","L","M","N","O","P","Q","R","S","T",
                                                                      "U","V","W","X","Y","Z"];

        private readonly Authentication _authentication = authentication;
        private readonly RestHttpClient _httpClient = httpClient;

        //[Throttle]
        /// <summary>
        /// 
        /// </summary>
        /// <param name="exchange"></param>
        /// <returns></returns>
        public async Task<(IEnumerable<ScripDetails>?, string)> GetAllScripAsync(Exchange exchange)
        {
            ConcurrentBag<ScripDetails> scrips = [];

            List<Task?> tasks = [];

            foreach (var instrBatch in InstrumentsPrefix.Chunk(2))
            {
                tasks.Add(Task.Run(async () =>
                {
                    foreach (var instr in instrBatch)
                    {
                        foreach (var instrPrefix in InstrumentsPrefix)
                        {
                            var (scripDetails, _) = await GetScripAsync(exchange, $"{instr}{instrPrefix}");
                            if (scripDetails is not null)
                            {
                                foreach (var scrip in scripDetails)
                                    scrips.Add(scrip);
                            }
                        }
                    }
                }));                
            }
         
            await Task.WhenAll(tasks.Where(t => t != null)!);        
            return (scrips.DistinctBy(instr => instr.Token).ToList(), Constants.StatusOk);
        }

        //[Throttle]
        /// <summary>
        /// 
        /// </summary>
        /// <param name="exchange"></param>
        /// <param name="searchText"></param>
        /// <returns></returns>
        public async Task<(IEnumerable<ScripDetails>?, string)> GetScripAsync(Exchange exchange, string searchText)
        {
            var (accessTokenResult, eMsg) = await _authentication.GetAccessTokenAsync();
            if (accessTokenResult is null)
            {
                eMsg = $"Cannot fetch scrips. {eMsg}";
                return (default, eMsg);
            }

            if (string.IsNullOrEmpty(searchText))
            {
                eMsg = "GetScripAsync function arguments are incorrect.";
                return (default, eMsg);
            }

            var userDetails = new ScripDetailsRequest
            {
                UserId = accessTokenResult.ClientCode,
                Exchange = exchange,
                SearchText = Uri.EscapeDataString(searchText)
            };
            var serializedUserDetails = JsonConvert.SerializeObject(userDetails);

            string requestParams = $"jData={serializedUserDetails}&jKey={Uri.EscapeDataString(accessTokenResult.AccessToken)}";
            var (response, eMsg1) = await _httpClient.PostMessageAsync<ScripDetailsResponse>(EndPoints.ScripDetailsUrl, requestParams, true);

            if (response is not null && response.ScripDetails is not null)
            {
                return (response.ScripDetails, Constants.StatusOk);
            }
            return (default, eMsg1);
        }

        //[Throttle]
        /// <summary>
        /// 
        /// </summary>
        /// <param name="exchange"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async virtual Task<(QuotesResponse?, string)> GetQuotesAsync(Exchange exchange, long token)
        {
            var (accessTokenResult, eMsg) = await _authentication.GetAccessTokenAsync();
            if (accessTokenResult is null)
            {
                eMsg = $"Cannot fetch quotes. {eMsg}";
                return (default, eMsg);
            }

            if (token <= 0)
            {
                eMsg = "GetQuotesAsync function arguments are incorrect.";
                return (default, eMsg);
            }

            var quoteDetails = new QuotesRequest { UserId = accessTokenResult.ClientCode, Exchange = exchange, Token = token };
            var serializedUserDetails = JsonConvert.SerializeObject(quoteDetails);

            string requestParams = $"jData={serializedUserDetails}&jKey={Uri.EscapeDataString(accessTokenResult.AccessToken)}";
            return await _httpClient.PostMessageAsync<QuotesResponse>(EndPoints.QuotesUrl, requestParams, true);
        }

        //[Throttle]
        /// <summary>
        /// 
        /// </summary>
        /// <param name="exchange"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async virtual Task<(LinkedScripsResponse?, string)> GetLinkedScripsAsync(Exchange exchange, long token)
        {
            var (accessTokenResult, eMsg) = await _authentication.GetAccessTokenAsync();
            if (accessTokenResult is null)
            {
                eMsg = $"Cannot fetch linked securities. {eMsg}";
                return (default, eMsg);
            }

            if (token <= 0)
            {
                eMsg = "GetLinkedScripsAsync function arguments are incorrect.";
                return (default, eMsg);
            }

            var linkedScripsRequest = new LinkedScripsRequest { UserId = accessTokenResult.ClientCode, Exchange = exchange, Token = token };
            var serializedLinkedScripsRequest = JsonConvert.SerializeObject(linkedScripsRequest);

            string requestParams = $"jData={serializedLinkedScripsRequest}&jKey={Uri.EscapeDataString(accessTokenResult.AccessToken)}";
            return await _httpClient.PostMessageAsync<LinkedScripsResponse>(EndPoints.GetLinkedScripsUrl, requestParams);
        }

        //[Throttle]
        /// <summary>
        /// 
        /// </summary>
        /// <param name="exchange"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<(SecurityInfoResponse?, string)> GetSecurityInfoAsync(Exchange exchange, long token)
        {
            var (accessTokenResult, eMsg) = await _authentication.GetAccessTokenAsync();
            if (accessTokenResult is null)
            {
                eMsg = $"Cannot fetch security info. {eMsg}";
                return (default, eMsg);
            }

            if (token <= 0)
            {
                eMsg = "GetSecurityInfoAsync function arguments are incorrect.";
                return (default, eMsg);
            }

            var securityInfoRequest = new SecurityInfoRequest { UserId = accessTokenResult.ClientCode, Exchange = exchange, Token = token };
            var serializedSecurityInfoRequest = JsonConvert.SerializeObject(securityInfoRequest);

            string requestParams = $"jData={serializedSecurityInfoRequest}&jKey={Uri.EscapeDataString(accessTokenResult.AccessToken)}";
            return await _httpClient.PostMessageAsync<SecurityInfoResponse>(EndPoints.GetSecurityInfoUrl, requestParams, true);
        }
    }
}
