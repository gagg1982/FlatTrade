using FlatTrade;
using FlatTrade.Common.Types.Base;
using FlatTrade.OrderManager;
using FlatTrade.SubscriptionManager;
using FlatTrade.SubscriptionManager.Order;
using Microsoft.Extensions.Logging;
using StrategyEngine.Model;
using StrategyEngine.Strategy;
using System.Diagnostics;
using static StrategyEngine.StrategyProcessor;

namespace StrategyEngine
{
    internal class OrderDetails :IDisposable
    {
        private bool _disposed = false;

        private readonly Api _api;
        private readonly ILogger<OrderDetails> _logger ;
        private readonly DirectFromServer _directFromServer;
        private readonly Helpers.Queue<OrderSubscriptionUpdates> _queue;

        private readonly OnStrategyEvents? _onStrategyEvents;
        public OrderDetails(Api api, DirectFromServer directFromServer, OnStrategyEvents? onStrategyEvents, ILoggerFactory loggerFactory)
        {
            _api = api;
            _logger = loggerFactory.CreateLogger<OrderDetails>();
            _directFromServer = directFromServer;
            _onStrategyEvents = onStrategyEvents;
            _queue = new(5000, "OrderUpdateQueue", OnOrderUpdates, loggerFactory);
        }

        public static async Task<IEnumerable<OrderBookResponse>> GetOrderBookFromServerAsync(Api api, ILogger logger)
        {

            var (orderBook, mesg) = await api.Order.GetOrderBookAsync();
            if (orderBook is null)
            {
                if (mesg != Constants.StatusOk)
                    logger.LogError("Error while fetching orders : {mesg}", mesg);
                else
                    logger.LogInformation("No orders found: {mesg}", mesg);
            }
            else
            {
                logger.LogInformation("Fetched {orderBookCount} orders from the order book.", orderBook.Count());
            }
            return orderBook ?? [];
        }

        public async Task UpdateOrderBook()
        {
            var orders = await GetOrderBookFromServerAsync(_api, _logger);
            await UpdateOrderBook(OrderInfo.ConvertFrom(orders));
        }

        public async Task SubscribeOrderUpdates()
        {
            var ok = await _api.Subscription.OrderSubscription.SubscribeAsync(OnOrderUpdates);
            if (!ok)
            {
                var msg = "Failed to subscribe to order updates.";
                _logger.LogError("{msg}", msg);
                throw new ApplicationException(msg);
            }

            _logger.LogInformation("Subscription request for order updates sent successfully.");
        }

        public async Task UnSubscribeOrderUpdates()
        {
            var ok = await _api.Subscription.OrderSubscription.UnsubscribeAsync(OnOrderUpdates);
            if (!ok)
            {
                var msg = "Failed to subscribe to order updates.";
                _logger.LogError("{msg}", msg);
                throw new ApplicationException(msg);
            }

            _logger.LogInformation("Subscription request for order updates sent successfully.");
        }

        private async Task UpdateOrderBook(IEnumerable<OrderInfo> orders)
        {
            bool isCompleted = false;
            bool isHoldingsUpdated = false;
            foreach (var order in orders)
            {
                var details = GlobalDataSet.Data.GetOrAdd(order.TradingSymbol, _ => new());
                isCompleted = order.OrderStatus == OrderStatus.Completed;
                isHoldingsUpdated = isCompleted && (order.ProductType == ProductType.Delivery);
                if (isCompleted ||
                        order.OrderStatus == OrderStatus.Rejected ||
                        order.OrderStatus == OrderStatus.Cancelled)
                {
                    details!.OpenOrders.Remove(order.NorenOrderNumber, out OrderInfo? _);
                    details!.ClosedOrders.AddOrUpdate(order.NorenOrderNumber, order, (key, existingValue) => order);                    
                }
                else
                {
                    details!.OpenOrders.AddOrUpdate(order.NorenOrderNumber, order, (key, existingValue) => order);
                }
            }

            if (isCompleted)
            {
                List<Task> tasks = [];
                tasks.Add(_directFromServer.UpdatePositions());
                tasks.Add(_directFromServer.UpdateTradeDetails());
                if (isHoldingsUpdated)
                    tasks.Add(_directFromServer.UpdateHoldingDetails());

                await Task.WhenAll(tasks);
            }
        }

        private async Task OnOrderUpdates(OrderSubscriptionUpdates Object)
        {
            if (Object is not null)
            {
                var orderInfo = OrderInfo.ConvertFrom(Object);
                if(orderInfo is not null)
                    await UpdateOrderBook([orderInfo]);
            }
        }

        private async Task OnOrderUpdates(object? _, SubscriptionType subscriptionType, string rawMessage, object? subscriptionObject)
        {
            var msg = string.Format($"[OnOrderUpdates]: Message processed: '{rawMessage}'");

            switch (subscriptionType)
            {
                case SubscriptionType.ConnectAck:
                    _logger.LogDebug("[OnOrderUpdates-ConnectAck] {msg}", msg);
                    var orders = await GetOrderBookFromServerAsync(_api, _logger);
                    await UpdateOrderBook(OrderInfo.ConvertFrom(orders ?? []));
                    await SubscribeOrderUpdates();
                    break;
                case SubscriptionType.SubscribeOrderAck:
                    _logger.LogDebug("[OnOrderUpdates-SubscribeOrderAck] {msg}", msg);
                    break;
                case SubscriptionType.UnsubscribeOrderAck:
                    _logger.LogDebug("[OnOrderUpdates-UnsubscribeOrderAck] {msg}", msg);
                    break;
                case SubscriptionType.SubscribeOrderUpdate:
                    _logger.LogDebug("[OnOrderUpdates-SubscribeOrderUpdate] {msg}", msg);
                    if (subscriptionObject is OrderSubscriptionUpdates Object)                    
                        await _queue.WriteAsync(Object);
                    break;
                default:
                    _logger.LogWarning("[OnOrderUpdates]: unknown message type '{type}' Msg '{msg}'", subscriptionType, msg);
                    break;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this); // Prevent finalizer from running again
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources here
                    _queue.WriteComplete().GetAwaiter().GetResult();
                    _queue.Dispose();
                }
                // Dispose unmanaged resources here if any
                _disposed = true;
            }
        }

        // Finalizer (only if you have unmanaged resources)
        ~OrderDetails()
        {
            Dispose(false);
        }
    }
}
