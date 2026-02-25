using System.Threading.Channels;

namespace Common.Helpers
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
