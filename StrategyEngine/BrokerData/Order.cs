using FlatTrade;
using FlatTrade.Common.Types.Base;
using FlatTrade.OrderManager;
using FlatTrade.SubscriptionManager;
using FlatTrade.SubscriptionManager.Order;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StrategyEngine.Model;
using StrategyEngine.Strategies;

namespace StrategyEngine.BrokerData
{
    internal class Order : IAsyncDisposable
    {
        private bool _disposed = false;

        private readonly IConfiguration _config;
        private readonly Api _api;
        private readonly ILogger<Order> _logger;
        private readonly ContextAccessor _contextAccessor;

        private readonly Helpers.Queue<OrderSubscriptionUpdates> _queue;

        private readonly OnUpdate? OnOrders;
        public Order(IConfiguration config, ContextAccessor contextAccessor, Api api, OnUpdate? onOrders, ILoggerFactory loggerFactory)
        {
            _api = api;
            _config = config;
            _logger = loggerFactory.CreateLogger<Order>();
            _contextAccessor = contextAccessor;
            OnOrders = onOrders;
            _queue = new(5000, "OrderUpdateQueue", OnOrderUpdates, loggerFactory);
        }

        public static async Task<IEnumerable<OrderBookResponse>> GetOrderBookFromServerAsync(Api api, ILogger logger)
        {

            var (orderBook, mesg) = await api.Order.GetOrderBookAsync();
            if (orderBook is null)
            {
                if (mesg != Constants.StatusOk && !mesg.Contains("no data"))
                    logger.LogError("Error while fetching orders : {mesg}", mesg);                
            }
            else
            {
                logger.LogInformation("Fetched {orderBookCount} orders from the order book. Rejected:{1}, Cancelled:{3}, Completed:{2}", 
                                        orderBook.Count(),
                                        orderBook.Where(o => o.OrderStatus == OrderStatus.Rejected).Count(),
                                        orderBook.Where(o => o.OrderStatus == OrderStatus.Cancelled).Count(),
                                        orderBook.Where(o => o.OrderStatus == OrderStatus.Completed).Count());
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

            _logger.LogDebug("Subscription request for order updates sent successfully.");
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
            List<OrderInfo> result = [];
            result.Capacity = orders.Count();

            foreach (var order in orders)
            {
                var details = GlobalDataSet.Data.GetOrAdd(order.TradingSymbol, _ => new());
                isCompleted = order.OrderStatus == OrderStatus.Completed;
                if (isCompleted ||
                        order.OrderStatus == OrderStatus.Rejected ||
                        order.OrderStatus == OrderStatus.AmoCancelled ||
                        order.OrderStatus == OrderStatus.Cancelled)
                {
                    result.Add(order);
                    details!.OpenOrders.Remove(order.NorenOrderNumber, out OrderInfo? _);
                    details!.ClosedOrders.AddOrUpdate(order.NorenOrderNumber, order, (key, existingValue) => order);
                }
                else
                {
                    var ord = details!.OpenOrders.AddOrUpdate(order.NorenOrderNumber, order, (key, existingValue) => order);
                }

                //sending notification only for OrderStatus =Completed,  Rejected, AmoCancelled, Cancelled
                //Open, AmoOpen
                if (order.OrderStatus == OrderStatus.Open || order.OrderStatus == OrderStatus.AmoOpen)
                    result.Add(order);

                if (OnOrders is not null)
                    foreach (var ord in result)
                        await OnOrders(new StrategyOnOrderSnapshot
                        {
                            Exchange = order.Exchange,
                            OrderStatus = order.OrderStatus,
                            NorenOrderNumber = order.NorenOrderNumber,
                            TradingSymbol = order.TradingSymbol,
                        });

                if (isCompleted)
                {
                    List<Task> tasks = [];
                    tasks.Add(_contextAccessor.Position.UpdatePositions(order.Exchange, order.TradingSymbol));
                    tasks.Add(_contextAccessor.Trade.UpdateTradeDetails(order.Exchange, order.TradingSymbol, order.NorenOrderNumber));

                    await Task.WhenAll(tasks);
                }
            }
        }

        private async Task OnOrderUpdates(OrderSubscriptionUpdates Object)
        {
            if (Object is not null)
            {
                var orderInfo = OrderInfo.ConvertFrom(Object);
                if (orderInfo is not null)
                    await UpdateOrderBook([orderInfo]);
            }
        }

        private async Task OnOrderUpdates(object? _, SubscriptionType subscriptionType, string rawMessage, object? subscriptionObject)
        {
            var msg = string.Format("Message processed: '{0}'", rawMessage);

            switch (subscriptionType)
            {
                case SubscriptionType.ConnectAck:
                    _logger.LogDebug("[OnOrderUpdates-ConnectAck] {msg}", msg);
                    var orders = await GetOrderBookFromServerAsync(_api, _logger);
                    await UpdateOrderBook(OrderInfo.ConvertFrom(orders ?? []));
                    await SubscribeOrderUpdates();
                    break;
                case SubscriptionType.SubscribeOrderAck:
                    _logger.LogInformation("[OnOrderUpdates-SubscribeOrderAck] {msg}", msg);
                    break;
                case SubscriptionType.UnsubscribeOrderAck:
                    _logger.LogInformation("[OnOrderUpdates-UnsubscribeOrderAck] {msg}", msg);
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
            DisposeAsyncCore().AsTask().GetAwaiter().GetResult(); // Safe sync fallback
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore();
            GC.SuppressFinalize(this);
        }

        private async ValueTask DisposeAsyncCore()
        {
            if (_disposed)
                return;

            _disposed = true;

            // Dispose async resources
            await _queue.DisposeAsync();

            _logger.LogInformation("{0}: Disposed gracefully", GetType().Name);
            // Dispose other sync-only resources here (e.g., timers, files)
        }       
    }
}
