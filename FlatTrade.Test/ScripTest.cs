using FlatTrade.ScripManager;
using FlatTrade.Types.Base;

namespace FlatTrade.Test
{
    internal static class ScripTest
    {
        public static async Task Execute(Api api)
        {
            var (_, _) = await GetAllScripTestAsync(api, Exchange.NSE);
            var (_, _) = await GetScripTestAsync(api, Exchange.NSE, "ETERNAL");
            var (_, _) = await GetQuotesTestAsync(api, Exchange.NSE, 5097);
            var (_, _) = await GetLinkedScripsTestAsync(api, Exchange.NSE, 5097);
            var (_, _) = await GetSecurityInfoTestAsync(api, Exchange.NSE, 5097);
        }

        public static async Task<(IEnumerable<ScripDetails>?, string)> GetAllScripTestAsync(Api api, Exchange exchange)
        {
            var (response, eMsg) = await api.Scrips.GetAllScripAsync(exchange);
            if (response is null)
            {
                await Console.Error.WriteLineAsync($"Failed GetAllScripTestAsync : {eMsg}");
            }
            else
            {
                response.ToList().ForEach(scrip => Console.WriteLine($"Passed GetAllScripTestAsync : [{scrip.TradingSymbol}({scrip.Token}) - {scrip.Exchange} - {scrip.LotSize} ]"));
            }
            return (response, eMsg);
        }

        public static async Task<(IEnumerable<ScripDetails>?, string)> GetScripTestAsync(Api api, Exchange exchange, string searchText)
        {
            var (response, eMsg) = await api.Scrips.GetScripAsync(exchange, searchText);
            if (response is null)
            {
                await Console.Error.WriteLineAsync($"Failed GetScripTestAsync : {eMsg}");
            }
            else
            {
                response.ToList().ForEach(scrip => Console.WriteLine($"Passed GetScripTestAsync : [{scrip.TradingSymbol}({scrip.Token}) - {scrip.Exchange} - {scrip.LotSize} ]"));
            }
            return (response, eMsg);
        }

        public static async Task<(QuotesResponse?, string)> GetQuotesTestAsync(Api api, Exchange exchange, int token)
        {
            var (response, eMsg) = await api.Scrips.GetQuotesAsync(exchange, token);
            if (response is null)
            {
                await Console.Error.WriteLineAsync($"Failed GetQuotesTestAsync : {eMsg}");
            }
            else
            {
                Console.WriteLine($"Passed GetQuotesTestAsync : [{response.TradingSymbol}({response.Token}) - {response.Exchange} - {response.LotSize}]");
            }
            return (response, eMsg);
        }

        public static async Task<(LinkedScripsResponse?, string)> GetLinkedScripsTestAsync(Api api, Exchange exchange, int token)
        {
            var (response, eMsg) = await api.Scrips.GetLinkedScripsAsync(exchange, token);
            if (response is null)
            {
                await Console.Error.WriteLineAsync($"Failed GetLinkedScripsTestAsync : {eMsg}");
            }
            else
            {
                Console.WriteLine("Passed GetLinkedScripsTestAsync Futures:");
                response.LinkedFutures.ForEach(fut => Console.WriteLine($"{fut.TradingSymbol}({fut.Token}) - [{fut.Expirydate}]"));
                Console.WriteLine("Passed GetLinkedScripsTestAsync Equities:");
                response.LinkedEquities.ForEach(eq => Console.WriteLine($"{eq.TradingSymbol}({eq.Token}) - [{eq.Exchange}] - [{eq.TickSize}]"));
                Console.WriteLine("Passed GetLinkedScripsTestAsync Options:");
                response.LinkedOptions.ForEach(opt => Console.WriteLine($"{opt.TradingSymbol}] - [{opt.Exchange}] - [{opt.Expirydate}]"));
            }
            return (response, eMsg);
        }

        public static async Task<(SecurityInfoResponse?, string)> GetSecurityInfoTestAsync(Api api, Exchange exchange, int token)
        {
            var (response, eMsg) = await api.Scrips.GetSecurityInfoAsync(exchange, token);
            if (response is null)
            {
                await Console.Error.WriteLineAsync($"Failed GetSecurityInfoTestAsync : {eMsg}");
            }
            else
            {
                Console.WriteLine($"Passed GetSecurityInfoTestAsync : [{response.SymbolName}({response.Token}) - [{response.UpperCircuitLimit}, {response.LowerCircuitLimit}] - {response.TradingSymbol}]");
            }
            return (response, eMsg);
        }
    }
}
