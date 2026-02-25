using FlatTrade.AuthenticationManager;

namespace FlatTrade.Test
{
    internal static class AuthenticationTest
    {
        public static async Task Execute(Api api)
        {
            var (_, _) = await GetAccessTokenTestAsync(api);
        }

        public async static Task<(AccessTokenInfo?, string)> GetAccessTokenTestAsync(Api api)
        {
            var (response, eMsg) = await api.Authentication.GetAccessTokenAsync();
            if (response is null)
            {
                await Console.Error.WriteLineAsync($"Failed GetAccessTokenTestAsync : {eMsg}");
            }
            else
            {
                Console.WriteLine($"Passed GetAccessTokenTestAsync : Get Access token [{response.AccessToken}]");
            }
            return (response, eMsg);
        }
    }
}
