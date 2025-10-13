namespace FlatTrade.SubscriptionManager
{
    public interface ISubscriptionType
    {
        IEnumerable<SubscriptionType> GetSubscriptionTypes();
    }
}
