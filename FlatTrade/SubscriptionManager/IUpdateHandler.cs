namespace FlatTrade.SubscriptionManager
{
    internal interface IUpdateHandler
    {
        Task OnMessageReceived(object? obj, SubscriptionEventArgs subscriptionEventArgs);
    }
}
