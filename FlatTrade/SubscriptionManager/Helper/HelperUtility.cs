using FlatTrade.Types.Base;
using System.Text;
using System.Threading.Channels;

namespace FlatTrade.SubscriptionManager.Helper
{
    public static class HelperUtility
    {
        public static Channel<T> CreateBoundedChannel<T>(int capacity)
        {
            var options = new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true, // Can have multiple readers if needed
                SingleWriter = true // Can have multiple writers if needed      
            };
            return Channel.CreateBounded<T>(options);
        }

        public static string CreateExchangeSymbolTokenPair(IEnumerable<KeyValuePair<Exchange, long>> exchangeSymbolTokenPair)
        {
            StringBuilder exchangeSymbolToken = new();
            foreach (var kvp in exchangeSymbolTokenPair)
            {
                exchangeSymbolToken.Append($"{kvp.Key}|{kvp.Value}#");
            }
            return exchangeSymbolToken.ToString().TrimEnd('#');
        }

        public static IEnumerable<KeyValuePair<Exchange, long>> GetValidExchangePairs(IEnumerable<KeyValuePair<Exchange, long>> pair)
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
