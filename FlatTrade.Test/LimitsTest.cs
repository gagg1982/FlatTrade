using FlatTrade.LimitsManager;
namespace FlatTrade.Test
{
    internal static class LimitsTest
    {
        public static async Task Execute(Api api)
        {
            var (_, _) = await GetLimitsTestAsync(api);
        }

        public async static Task<(LimitsResponse?, string)> GetLimitsTestAsync(Api api)
        {
            var (response, eMsg) = await api.Limits.GetLimitsAsync();
            if (response is null)
            {
                await Console.Error.WriteLineAsync($"Failed GetLimitsTestAsync : {eMsg}");
            }
            else
            {
                Console.WriteLine($"Passed GetLimitsTestAsync : Total margin [{response.MarginCashAvailable}], Current used margin [{response.TotalMarginUsedToday}], Available Margin [{response.MarginCashAvailable - response.TotalMarginUsedToday}]");
            }
            return (response, eMsg);
        }
    }
}
