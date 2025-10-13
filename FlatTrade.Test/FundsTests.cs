using FlatTrade.FundManager;

namespace FlatTrade.Test
{
    internal static class FundsTests
    {
        public static async Task Execute(Api api)
        {
            var (_, _) = await FundsPayOutRequestTestAsync(api, "Test Sdk", 1000);
            var (_, _) = await CancelPayOutTestAsync(api, 20251810002995, "Test Sdk");

            var (_, _) = await GetPayOutReportTestAsync(api, new DateOnly(2025, 04, 01), new DateOnly(2025, 06, 30));
            var (_, _) = await GetPayInReportTestAsync(api, new DateOnly(2025, 04, 01), new DateOnly(2025, 06, 30));

            var (_, _) = await GetMaxPayoutAmountTestAsync(api);
        }

        public async static Task<(MaxPayOutAmountResponse?, string)> GetMaxPayoutAmountTestAsync(Api api)
        {
            var (response, eMsg) = await api.Funds.GetMaxPayoutAmountAsync();
            if (response is null)
            {
                await Console.Error.WriteLineAsync($"Failed GetMaxPayoutAmountTestAsync : {eMsg}");
            }
            else
            {
                Console.WriteLine($"Passed GetMaxPayoutAmountTestAsync : Get max payout amount [{response.MaximumPayOutAmount}]");
            }
            return (response, eMsg);
        }
        public async static Task<(FundsPayOutResponse?, string)> FundsPayOutRequestTestAsync(Api api, string remarks, decimal payOutAmount)
        {
            var (response, eMsg) = await api.Funds.FundsPayOutRequestAsync(remarks, payOutAmount);
            if (response is null)
            {
                await Console.Error.WriteLineAsync($"Failed FundsPayOutRequestTestAsync : {eMsg}");
            }
            else
            {
                Console.WriteLine($"Passed FundsPayOutRequestTestAsync : Funds payout request for transaction id [{response.TransactionId}]");
            }
            return (response, eMsg);
        }
        public async static Task<(GetPayInReportResponse?, string)> GetPayInReportTestAsync(Api api, DateOnly fromDate, DateOnly toDate)
        {
            var (response, eMsg) = await api.Funds.GetPayInReportAsync(fromDate, toDate);
            if (response is null)
            {
                await Console.Error.WriteLineAsync($"Failed GetPayInReportTestAsync : {eMsg}");
            }
            else
            {
                Console.WriteLine($"Passed GetPayInReportTestAsync : Get payin report for transaction ref. & status[{response.TransactionReferenceNumber}:{response.TransactionStatus}]");
            }
            return (response, eMsg);
        }
        public async static Task<(GetPayOutReportResponse?, string)> GetPayOutReportTestAsync(Api api, DateOnly fromDate, DateOnly toDate)
        {
            var (response, eMsg) = await api.Funds.GetPayOutReportAsync(fromDate, toDate);
            if (response is null)
            {
                await Console.Error.WriteLineAsync($"Failed GetPayOutReportTestAsync : {eMsg}");
            }
            else
            {
                Console.WriteLine($"Passed GetPayOutReportTestAsync : Get payout report for transaction ref. & status[{response.TransactionReferenceNumber}:{response.TransactionStatus}]");
            }
            return (response, eMsg);
        }
        public async static Task<(CancelPayOutResponse?, string)> CancelPayOutTestAsync(Api api, long referenceNumber, string brokerName)
        {
            var (response, eMsg) = await api.Funds.CancelPayOutAsync(referenceNumber, brokerName);
            if (response is null)
            {
                await Console.Error.WriteLineAsync($"Failed CancelPayOutTestAsync : {eMsg}");
            }
            else
            {
                Console.WriteLine($"Passed CancelPayOutTestAsync : Cancel payout request transaction status [{response.TransactionStatus}]");
            }
            return (response, eMsg);
        }
    }
}
