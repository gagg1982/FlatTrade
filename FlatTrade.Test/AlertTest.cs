using FlatTrade.AlertManager;
using FlatTrade.Common.Types.Base;

namespace FlatTrade.Test
{
    internal static class AlertTest
    {
        public static async Task Execute(Api api)
        {
            var (_, _) = await GetUnSettledTradingDateTestAsync(api);
            var (_, _) = await GetEnabledAlertTypesTestAsync(api);
            var (_, _) = await SetAlertTestAsync(api);
            var (_, _) = await GetEnabledAlertTypesTestAsync(api);
            var (resp, _) = await GetPendingAlertTestAsync(api);
            resp?.ToList().ForEach(alert => ModifyAlertTestAsync(api, alert.AlertId, +2, "Test sdk modify test").GetAwaiter().GetResult());
            resp?.ToList().ForEach(alert => CancelAlertTestAsync(api, alert.AlertId).GetAwaiter().GetResult());
        }

        private async static Task<(UnSettledTradingDateResponse?, string)> GetUnSettledTradingDateTestAsync(Api api)
        {
            var (response, eMsg) = await api.Alerts.GetUnSettledTradingDateAsync();
            if (response is null)
            {
                await Console.Error.WriteLineAsync($"Failed GetUnSettledTradingDateTestAsync : {eMsg}");
            }
            else
            {
                response.TradeDate.ForEach(tradeDate => Console.WriteLine($"Passed GetUnSettledTradingDateTestAsync : Get Unsettled trading dates [{tradeDate}]"));
            }
            return (response, eMsg);
        }
        private static async Task<(EnabledAlertTypesResponse?, string)> GetEnabledAlertTypesTestAsync(Api api)
        {
            var (response, eMsg) = await api.Alerts.GetEnabledAlertTypesAsync();
            if (response is null)
            {
                await Console.Error.WriteLineAsync($"Failed GetEnabledAlertTypesTestAsync : {eMsg}");
            }
            else
            {
                response.AlertTypes.ForEach(alertType => Console.WriteLine($"Passed GetEnabledAlertTypesTestAsync : Get Enabled Alert Types [{alertType}]"));
            }
            return (response, eMsg);
        }
        private static async Task<(IEnumerable<PendingAlertResponse>?, string)> GetPendingAlertTestAsync(Api api)
        {
            var (response, eMsg) = await api.Alerts.GetPendingAlertAsync();
            if (response is null)
            {
                await Console.Error.WriteLineAsync($"Failed GetPendingAlertTestAsync : {eMsg}");
            }
            else
            {
                response.ToList().ForEach(alert => Console.WriteLine($"Passed GetPendingAlertTestAsync : Get Pending AlertId [{alert.AlertId}]"));
            }
            return (response, eMsg);
        }
        private static async Task<(ModifyAlertResponse?, string)> ModifyAlertTestAsync(Api api, long alertId, decimal valueToBeModified, string remarks)
        {
            var (response, eMsg) = await api.Alerts.ModifyAlertAsync(alertId, valueToBeModified, remarks);
            if (response is null)
            {
                await Console.Error.WriteLineAsync($"Failed ModifyAlertTestAsync : {eMsg}");
            }
            else
            {
                Console.WriteLine($"Passed ModifyAlertTestAsync : Modify AlertId [{response.AlertId}] - [{response.Status}]");
            }
            return (response, eMsg);
        }
        private static async Task<(CancelAlertResponse?, string)> CancelAlertTestAsync(Api api, long alertId)
        {
            var (response, eMsg) = await api.Alerts.CancelAlertAsync(alertId);
            if (response is null)
            {
                await Console.Error.WriteLineAsync($"Failed CancelAlertTestAsync : {eMsg}");
            }
            else
            {
                Console.WriteLine($"Passed CancelAlertTestAsync : Cancel AlertId [{response.AlertId}]- [{response.Status}]");
            }
            return (response, eMsg);
        }
        private static async Task<(SetAlertResponse?, string)> SetAlertTestAsync(Api api)
        {
            var setAlertRequest = new SetAlertRequest
            {
                UserId = string.Empty,
                AlertType = AlertType.AverageTradePriceGreaterThan,
                Validity = RetentionType.DAY,
                Exchange = Exchange.NSE,
                TradingSymbol = "ETERNAL-EQ",
                Remarks = "Test Sdk",
                DataToBeComparedWith = 100.01m
            };

            var (response, eMsg) = await api.Alerts.SetAlertAsync(setAlertRequest);
            if (response is null)
            {
                await Console.Error.WriteLineAsync($"Failed SetAlertTestAsync : {eMsg}");
            }
            else
            {
                Console.WriteLine($"Passed SetAlertTestAsync : Set AlertId [{response.AlertId}] - [{response.Status}]");
            }
            return (response, eMsg);
        }
    }
}
