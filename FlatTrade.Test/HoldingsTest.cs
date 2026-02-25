using FlatTrade.HoldingsManager;
using FlatTrade.Types.Base;

namespace FlatTrade.Test
{
    internal static class HoldingsTest
    {
        public static async Task Execute(Api api)
        {
            var (_, _) = await GetHoldingsTestAsync(api);
        }

        public async static Task<(IEnumerable<HoldingsResponse>?, string)> GetHoldingsTestAsync(Api api)
        {
            var (response, eMsg) = await api.Holdings.GetHoldingsAsync();
            if (response is null)
            {
                await Console.Error.WriteLineAsync($"Failed GetHoldingsTestAsync : {eMsg}");
            }
            else
            {
                decimal totalInvestment = 0;
                response.ToList().ForEach(holding =>
                {
                    holding.ExchangeSymbolResponse
                         .Where(exch => exch.Exchange == Exchange.NSE).ToList()
                         .ForEach(exchSymbolPair =>
                         {
                             decimal holdingInvestment = holding.AvgPriceUploadedAlongWithHoldings * (holding.NonPoaDisplayQuantity + holding.NonPoaDisplayT1Quantity);
                             totalInvestment += holding.AvgPriceUploadedAlongWithHoldings * (holding.NonPoaDisplayQuantity + holding.NonPoaDisplayT1Quantity);
                             Console.WriteLine($"Passed GetHoldingsTestAsync : " +
                             $"Get holdings for symbol [{exchSymbolPair.TradingSymbol}], " +
                             $"Qty [{holding.NonPoaDisplayQuantity}] " +
                             $"T1Qty [{holding.NonPoaDisplayT1Quantity}] " +
                             $"AveragePrice [{holding.AvgPriceUploadedAlongWithHoldings}] " +
                             $"HoldingInvestment [{holdingInvestment}]");
                         });
                }
                );
                Console.WriteLine($"Passed GetHoldingsTestAsync : Total investment value of holdings [{totalInvestment}]");
            }
            return (response, eMsg);
        }

    }
}
