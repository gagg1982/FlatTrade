using FlatTrade.AuthenticationManager;
using FlatTrade.Common.Helpers;
using FlatTrade.Common.Throttle;
using FlatTrade.Common.Transport;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FlatTrade.FundManager
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="authentication"></param>
    /// <param name="httpClient"></param>
    public class Funds(Authentication authentication, RestHttpClient httpClient, ILoggerFactory loggerFactory)
    {
        private readonly ILogger<Funds> _logger = loggerFactory.CreateLogger<Funds>();
        private readonly Authentication _authentication = authentication;
        private readonly RestHttpClient _httpClient = httpClient;

      //[Throttle]
        /// <summary>
        /// 
        /// </summary>
        /// <param name="transactionReferenceNumber"></param>
        /// <param name="brokerName"></param>
        /// <returns></returns>
        public async virtual Task<(CancelPayOutResponse?, string)> CancelPayOutAsync(long transactionReferenceNumber, string brokerName)
        {
            var (accessTokenResult, eMsg) = await _authentication.GetAccessTokenAsync();
            if (accessTokenResult is null)
            {
                eMsg = $"Cannot cancel pay out. {eMsg}";
                return (default, eMsg);
            }

            var cancelPayOut = new CancelPayOutRequest
            {
                UserId = accessTokenResult.ClientCode,
                AccountId = accessTokenResult.ClientCode,
                TransactionReferenceNumber = transactionReferenceNumber,
                BrokerName = string.IsNullOrEmpty(brokerName) ? "FLATTRADE" : brokerName
            };
            var serializedCancelPayOut = JsonConvert.SerializeObject(cancelPayOut);

            string requestParams = $"jData={serializedCancelPayOut}&jKey={Uri.EscapeDataString(accessTokenResult.AccessToken)}";
            return await _httpClient.PostMessageAsync<CancelPayOutResponse>(EndPoints.CancelPayoutUrl, requestParams);
        }

      //[Throttle]
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <returns></returns>
        public async virtual Task<(GetPayOutReportResponse?, string)> GetPayOutReportAsync(DateOnly fromDate, DateOnly toDate)
        {
            var (accessTokenResult, eMsg) = await _authentication.GetAccessTokenAsync();
            if (accessTokenResult is null)
            {
                eMsg = $"Cannot fetch payout report. {eMsg}";
                return (default, eMsg);
            }

            var getPayOutReport = new GetPayOutReportRequest
            {
                AccountId = accessTokenResult.ClientCode,
                FromDate = fromDate,
                ToDate = toDate
            };
            var serializedGetPayOutReport = JsonConvert.SerializeObject(getPayOutReport);

            string requestParams = $"jData={serializedGetPayOutReport}&jKey={Uri.EscapeDataString(accessTokenResult.AccessToken)}";
            return await _httpClient.PostMessageAsync<GetPayOutReportResponse>(EndPoints.PayOutReportUrl, requestParams);
        }

      //[Throttle]
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <returns></returns>
        public async virtual Task<(GetPayInReportResponse?, string)> GetPayInReportAsync(DateOnly fromDate, DateOnly toDate)
        {
            var (accessTokenResult, eMsg) = await _authentication.GetAccessTokenAsync();
            if (accessTokenResult is null)
            {
                eMsg = $"Cannot fetch payin report. {eMsg}";
                return (default, eMsg);
            }

            var getPayInReport = new GetPayInReportRequest
            {
                AccountId = accessTokenResult.ClientCode,
                FromDate = fromDate,
                ToDate = toDate
            };
            var serializedGetPayInReport = JsonConvert.SerializeObject(getPayInReport);

            string requestParams = $"jData={serializedGetPayInReport}&jKey={Uri.EscapeDataString(accessTokenResult.AccessToken)}";
            return await _httpClient.PostMessageAsync<GetPayInReportResponse>(EndPoints.PayInReportUrl, requestParams);
        }

      //[Throttle]
        /// <summary>
        /// 
        /// </summary>
        /// <param name="remarks"></param>
        /// <param name="payOutAmount"></param>
        /// <returns></returns>
        public async virtual Task<(FundsPayOutResponse?, string)> FundsPayOutRequestAsync(string remarks, decimal payOutAmount)
        {
            var (accessTokenResult, eMsg) = await _authentication.GetAccessTokenAsync();
            if (accessTokenResult is null)
            {
                eMsg = $"Cannot fetch payout report. {eMsg}";
                return (default, eMsg);
            }

            var fundsPayOut = new FundsPayOutRequest
            {
                UserId = accessTokenResult.ClientCode,
                AccountId = accessTokenResult.ClientCode,
                PayOutAmount = payOutAmount * 100,
                Remarks = remarks
            };
            var serializedFundsPayOut = JsonConvert.SerializeObject(fundsPayOut);

            string requestParams = $"jData={serializedFundsPayOut}&jKey={Uri.EscapeDataString(accessTokenResult.AccessToken)}";
            return await _httpClient.PostMessageAsync<FundsPayOutResponse>(EndPoints.FundsPayOutRequestUrl, requestParams);
        }

      //[Throttle]
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async virtual Task<(MaxPayOutAmountResponse?, string)> GetMaxPayoutAmountAsync()
        {
            var (accessTokenResult, eMsg) = await _authentication.GetAccessTokenAsync();
            if (accessTokenResult is null)
            {
                eMsg = $"Cannot fetch max payout amount. {eMsg}";
                return (default, eMsg);
            }

            var getMaxPayOutAmount = new GetMaxPayOutAmountRequest { UserId = accessTokenResult.ClientCode, AccountId = accessTokenResult.ClientCode };
            var serializedGetMaxPayOutAmount = JsonConvert.SerializeObject(getMaxPayOutAmount);

            string requestParams = $"jData={serializedGetMaxPayOutAmount}&jKey={Uri.EscapeDataString(accessTokenResult.AccessToken)}";
            return await _httpClient.PostMessageAsync<MaxPayOutAmountResponse>(EndPoints.MaxPayOutAmountUrl, requestParams);
        }
    }
}
