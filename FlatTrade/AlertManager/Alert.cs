using FlatTrade.AuthenticationManager;
using FlatTrade.Common.Helpers;
using FlatTrade.Common.Throttle;
using FlatTrade.Common.Transport;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
namespace FlatTrade.AlertManager
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="authentication"></param>
    /// <param name="httpClient"></param>
    public class Alert(Authentication authentication, RestHttpClient httpClient, ILoggerFactory loggerFactory)
    {
        private readonly ILogger<Alert> _logger = loggerFactory.CreateLogger<Alert>();
        private readonly Authentication _authentication = authentication;
        private readonly RestHttpClient _httpClient = httpClient;

        //[Throttle]
        /// <summary>
        /// 
        /// </summary>
        /// <param name="setAlertRequest"></param>
        /// <returns></returns>
        public async virtual Task<(SetAlertResponse?, string)> SetAlertAsync(SetAlertRequest setAlertRequest)
        {
            var (accessTokenResult, eMsg) = await _authentication.GetAccessTokenAsync();
            if (accessTokenResult is null)
            {
                eMsg = $"Cannot set alert. {eMsg}";
                return (default, eMsg);
            }

            setAlertRequest.UserId = accessTokenResult.ClientCode;
            var serializedSetAlertRequest = JsonConvert.SerializeObject(setAlertRequest);

            string requestParams = $"jData={serializedSetAlertRequest}&jKey={Uri.EscapeDataString(accessTokenResult.AccessToken)}";
            return await _httpClient.PostMessageAsync<SetAlertResponse>(EndPoints.SetAlertUrl, requestParams);
        }

        //[Throttle]
        /// <summary>
        /// 
        /// </summary>
        /// <param name="alertId"></param>
        /// <returns></returns>
        public async virtual Task<(CancelAlertResponse?, string)> CancelAlertAsync(long alertId)
        {
            var (accessTokenResult, eMsg) = await _authentication.GetAccessTokenAsync();
            if (accessTokenResult is null)
            {
                eMsg = $"Cannot cancel alert. {eMsg}";
                return (default, eMsg);
            }

            var cancelAlertRequest = new CancelAlertRequest
            {
                AlertId = alertId,
                UserId = accessTokenResult.ClientCode
            };

            var serializedCancelAlertRequest = JsonConvert.SerializeObject(cancelAlertRequest);

            string requestParams = $"jData={serializedCancelAlertRequest}&jKey={Uri.EscapeDataString(accessTokenResult.AccessToken)}";
            return await _httpClient.PostMessageAsync<CancelAlertResponse>(EndPoints.CancelAlertUrl, requestParams);
        }

      //[Throttle]
        /// <summary>
        /// 
        /// </summary>
        /// <param name="alertId"></param>
        /// <param name="valueToBeModified"></param>
        /// <param name="remarks"></param>
        /// <returns></returns>
        public async virtual Task<(ModifyAlertResponse?, string)> ModifyAlertAsync(long alertId, decimal valueToBeModified, string remarks)
        {
            var (accessTokenResult, eMsg) = await _authentication.GetAccessTokenAsync();
            if (accessTokenResult is null)
            {
                eMsg = $"Cannot modify alert. {eMsg}";
                return (default, eMsg);
            }

            var (resp, eMsg1) = await GetPendingAlertAsync(); // Ensure pending alerts are fetched before modifying an alert
            if (resp is null)
            {
                eMsg1 = $"Alert with ID {alertId} not found in pending alerts. {eMsg1}";
                return (default, eMsg1);
            }
            var alert = resp.FirstOrDefault(alert => alert.AlertId == alertId); // Ensure the alert exists
            if (alert is null)
            {
                eMsg1 = $"Alert with ID {alertId} not found in pending alerts. {eMsg1}";
                return (default, eMsg1);
            }

            var modifyAlertRequest = new ModifyAlertRequest
            {
                AlertId = alert.AlertId,
                UserId = accessTokenResult.ClientCode,
                AlertType = alert.AlertType,
                Exchange = alert.Exchange,
                TradingSymbol = alert.TradingSymbol,
                Validity = alert.Validity,
                DataToBeComparedWith = alert.DataToBeComparedWith + valueToBeModified, // Assuming valueToBeModified is a decimal
                Remarks = remarks ?? "Modified through FlatTrade Sdk"
            };
            var serializedModifyAlertRequest = JsonConvert.SerializeObject(modifyAlertRequest);

            string requestParams = $"jData={serializedModifyAlertRequest}&jKey={Uri.EscapeDataString(accessTokenResult.AccessToken)}";
            return await _httpClient.PostMessageAsync<ModifyAlertResponse>(EndPoints.ModifyAlertUrl, requestParams);
        }

      //[Throttle]
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async virtual Task<(IEnumerable<PendingAlertResponse>?, string)> GetPendingAlertAsync()
        {
            var (accessTokenResult, eMsg) = await _authentication.GetAccessTokenAsync();
            if (accessTokenResult is null)
            {
                eMsg = $"Cannot fetch pending alert. {eMsg}";
                return (default, eMsg);
            }

            var pendingAlert = new PendingAlertRequest { UserId = accessTokenResult.ClientCode };
            var serializedPendingAlert = JsonConvert.SerializeObject(pendingAlert);

            string requestParams = $"jData={serializedPendingAlert}&jKey={Uri.EscapeDataString(accessTokenResult.AccessToken)}";
            return await _httpClient.PostMessageAsync<IEnumerable<PendingAlertResponse>>(EndPoints.PendingAlertUrl, requestParams);
        }

      //[Throttle]
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async virtual Task<(EnabledAlertTypesResponse?, string)> GetEnabledAlertTypesAsync()
        {
            var (accessTokenResult, eMsg) = await _authentication.GetAccessTokenAsync();
            if (accessTokenResult is null)
            {
                eMsg = $"Cannot fetch enabled alert types. {eMsg}";
                return (default, eMsg);
            }

            var enabledAlertTypesRequest = new EnabledAlertTypesRequest { UserId = accessTokenResult.ClientCode };
            var serializedEnabledAlertTypesRequest = JsonConvert.SerializeObject(enabledAlertTypesRequest);

            string requestParams = $"jData={serializedEnabledAlertTypesRequest}&jKey={Uri.EscapeDataString(accessTokenResult.AccessToken)}";
            return await _httpClient.PostMessageAsync<EnabledAlertTypesResponse>(EndPoints.EnabledAlertTypesUrl, requestParams);
        }

        //[Throttle]
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async virtual Task<(UnSettledTradingDateResponse?, string)> GetUnSettledTradingDateAsync()
        {
            var (accessTokenResult, eMsg) = await _authentication.GetAccessTokenAsync();
            if (accessTokenResult is null)
            {
                eMsg = $"Cannot get unsettled trading date. {eMsg}";
                return (default, eMsg);
            }

            var unSettledTradingDateRequest = new UnSettledTradingDateRequest { UserId = accessTokenResult.ClientCode };
            var serializedUnSettledTradingDateRequest = JsonConvert.SerializeObject(unSettledTradingDateRequest);

            string requestParams = $"jData={serializedUnSettledTradingDateRequest}&jKey={Uri.EscapeDataString(accessTokenResult.AccessToken)}";
            return await _httpClient.PostMessageAsync<UnSettledTradingDateResponse>(EndPoints.UnSettledTradingDateUrl, requestParams);
        }
    }
}
