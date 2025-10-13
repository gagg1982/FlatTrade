
using FlatTrade.Common.Types.Base;
using System.Text;

namespace FlatTrade.SubscriptionManager.Helper
{
    internal static class HelperUtility
    {
        internal static string CreateExchangeSymbolTokenPair(IEnumerable<KeyValuePair<Exchange, long>> exchangeSymbolTokenPair)
        {
            StringBuilder exchangeSymbolToken = new();
            foreach (var kvp in exchangeSymbolTokenPair)
            {
                exchangeSymbolToken.Append($"{kvp.Key}|{kvp.Value}#");
            }
            return exchangeSymbolToken.ToString().TrimEnd('#');
        }

        internal static IEnumerable<KeyValuePair<Exchange, long>> GetValidExchangePairs(IEnumerable<KeyValuePair<Exchange, long>> pair)
        {
            return new List<KeyValuePair<Exchange, long>>(pair).FindAll(p => p.Value > 0);
        }

        //internal static Task<EventHandler<string>> IsHandlerValid(Task<EventHandler<string>>? handlers)
        //{
        //    Task<EventHandler<string>> validHandlers ;
        //    if (handlers == null || !handlers.Status)
        //    {
        //        return validHandlers;
        //    }

        //    handlers.Where(h => h != null ).ToList().ForEach(h => validHandlers.Add(h));

        //    return validHandlers;
        //}
    }
}
