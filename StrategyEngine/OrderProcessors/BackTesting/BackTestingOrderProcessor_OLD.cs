//using FlatTrade;
//using FlatTrade.Types.Base;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Logging;
//using StrategyEngine.Model;
//using StrategyEngine.OrderProcessors.SingleLegOrder;
//using System.Collections.Concurrent;

//namespace StrategyEngine.OrderProcessors.BackTesting
//{
//    internal class TestOrderInfo
//    {
//        internal TestOrderInfo? ParentOrderReference { get; set; } = null;
//        internal TestOrderInfo? TargetProfitOrderReference { get; set; } = null;
//        internal TestOrderInfo? StopLossLimitOrderReference { get; set; } =  null;
//        internal DateTime OrderCreationTime { get; set; } = DateTime.Now;
//        internal string TradingSymbol { get; set; } = string.Empty;
//        internal long Token { get; set; }
//        internal Exchange Exchange { get; set; }
//        internal Guid OrderNumber { get; set; }   
//        internal TransactionType TransactionType { get; set; }
//        internal int Quantity { get; set; }
//        internal decimal LimitPrice { get; set; }
//        internal DateTime FillTime { get; set; } = DateTime.MinValue;
//        internal decimal FillPrice { get; set; }
//    };

//    internal class BackTestingOrderProcessor_OLD : AsbtractOrderProcessor<SingleLegManagedOrderProcessor>, IAsyncDisposable, IDisposable
//    {
//        private bool _disposed = false;
//        private readonly string _tradeFilePath = $"..//..//..//BackTestingTrades.csv";
//        private ConcurrentDictionary<string, decimal> _lastTradePrice = [];
//        private TimeSpan _squareOffTime = new TimeSpan(15, 10, 0);
//        private readonly StreamWriter _writer;
//        private ConcurrentDictionary<string, ConcurrentDictionary<Guid, TestOrderInfo>> _orders = [];

//        protected override string Name => $"{GetType().Name}";

//        internal BackTestingOrderProcessor_OLD(IConfiguration config, Api api, ILoggerFactory loggerFactory)
//            :base(config, api, loggerFactory)
//        {
//            _writer = new StreamWriter(_tradeFilePath, append: true)
//            {
//                AutoFlush = true // flush after each write
//            };
//            var header = string.Format($"StrategyName,OrderCreationTimeStamp,TradingSymbol,Exchange,Token,LimitPrice,Quantity,EntryTime,EntryPrice,TransactionType,StopLossLimitPrice,TargetProfitLimitPrice,ExitTime,ExitPrice,PAndL");
//            _writer.WriteLine(header);

//            Task.Run(() => BackgroundAutoSquareOffWorker());
//        }

//        private async Task BackgroundAutoSquareOffWorker()
//        {
//            _logger.LogInformation("BackgroundAutoSquareOffWorker task started.");
//            while (true)
//            {
//                var now = DateTime.Now;

//                if (now.TimeOfDay >= _squareOffTime)
//                {
//                    foreach (var (symbol, order) in _orders)
//                    {
//                        foreach (var (orderNumber, orderDetails) in order)
//                        {
//                            if(orderDetails.StopLossLimitOrderReference?.OrderNumber is not null &&
//                                orderDetails.TargetProfitOrderReference?.OrderNumber is not null)
//                            {
//                                //must be a parent order
//                                // no action required (just cancellation).
//                                _orders.GetOrAdd(orderDetails.TradingSymbol, _ => new ConcurrentDictionary<Guid, TestOrderInfo>())
//                                       .TryRemove(orderDetails.OrderNumber, out _);
//                                continue;
//                            }

//                            orderDetails.LimitPrice  = _lastTradePrice.GetOrAdd(orderDetails.TradingSymbol, 0.0m);

//                            var targetProfit = 0.0m;
//                            var stopLoss = 0.0m;
//                            if (orderDetails.TargetProfitOrderReference is null)
//                            {
//                                //its a targetProfitOrder only
//                                targetProfit = orderDetails.LimitPrice;
//                                stopLoss = orderDetails.StopLossLimitOrderReference!.LimitPrice;
//                            }

//                            if (orderDetails.StopLossLimitOrderReference is null)
//                            {
//                                //its a stop loss limit order only
//                                stopLoss = orderDetails.LimitPrice;
//                                targetProfit = orderDetails.TargetProfitOrderReference!.LimitPrice;
//                            }

//                            var lastTradeprice = _lastTradePrice.GetOrAdd(orderDetails.TradingSymbol, 0.0m);
//                            var str = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14}",
//                                      Name,
//                                      orderDetails.ParentOrderReference?.OrderCreationTime,
//                                      orderDetails.TradingSymbol,
//                                      orderDetails.Exchange,
//                                      orderDetails.Token,
//                                      orderDetails.ParentOrderReference?.LimitPrice,
//                                      orderDetails.ParentOrderReference?.Quantity,
//                                      orderDetails.ParentOrderReference?.FillTime,
//                                      orderDetails.ParentOrderReference?.FillPrice,
//                                      orderDetails.ParentOrderReference?.TransactionType,
//                                      stopLoss,
//                                      targetProfit,
//                                      DateTime.Now,
//                                      lastTradeprice,
//                                      (lastTradeprice - orderDetails.ParentOrderReference?.FillPrice) * orderDetails.ParentOrderReference?.Quantity
//                                      );

//                            _writer.WriteLine(str);

//                            //remove both the target profit and stoploss limit order
//                            _orders.GetOrAdd(orderDetails.TradingSymbol, _ => new ConcurrentDictionary<Guid, TestOrderInfo>())
//                                       .TryRemove(orderDetails.OrderNumber, out _);

//                            _orders.GetOrAdd(orderDetails.TradingSymbol, _ => new ConcurrentDictionary<Guid, TestOrderInfo>())
//                                       .TryRemove(orderDetails.StopLossLimitOrderReference?.OrderNumber ??
//                                                  orderDetails.TargetProfitOrderReference?.OrderNumber ?? Guid.Empty, out _);
//                        }
//                    }
//                    break;
//                }
//                await Task.Delay(TimeSpan.FromSeconds(_squareOffTime.TotalSeconds - now.TimeOfDay.TotalSeconds));
//            }
//            _logger.LogInformation("BackgroundAutoSquareOffWorker task finished.");
//        }

//        public override Task CancelOrder(string strategyName, CancelOrder cancelOrder)
//        {
//            throw new NotImplementedException();
//        }

//        public override Task CreateOrder(string strategyName, CreateOrder createOrder)
//        {
//            var parentOrder = new TestOrderInfo
//            {
//                Exchange = createOrder.Exchange,
//                TradingSymbol = createOrder.TradingSymbol,
//                Token = createOrder.Token,
//                LimitPrice = createOrder.LimitPrice,
//                Quantity = createOrder.Quantity,
//                OrderNumber = Guid.NewGuid(),
//                TransactionType = createOrder.TransactionType,
//                ParentOrderReference = null,
//            };

//            var stopLossLimitOrder = new TestOrderInfo
//            {
//                Exchange = createOrder.Exchange,
//                TradingSymbol = createOrder.TradingSymbol,
//                Token = createOrder.Token,
//                LimitPrice = createOrder.LimitPrice + (createOrder.TransactionType == TransactionType.Buy ? -createOrder.DifferentialSLPrice: createOrder.DifferentialSLPrice),
//                Quantity = createOrder.Quantity,
//                OrderNumber = Guid.NewGuid(),
//                TransactionType = createOrder.TransactionType == TransactionType.Buy ? TransactionType.Sell : TransactionType.Buy,
//                ParentOrderReference = parentOrder,
//                StopLossLimitOrderReference = null,
//            }; 

//            var targetProfitOrder = new TestOrderInfo
//            {
//                Exchange = createOrder.Exchange,
//                TradingSymbol = createOrder.TradingSymbol,
//                Token = createOrder.Token,
//                LimitPrice = createOrder.LimitPrice + (createOrder.TransactionType == TransactionType.Buy ? createOrder.DifferentialProfitPrice : -createOrder.DifferentialProfitPrice),
//                Quantity = createOrder.Quantity,
//                TransactionType = createOrder.TransactionType == TransactionType.Buy ? TransactionType.Sell : TransactionType.Buy,
//                OrderNumber = Guid.NewGuid(),
//                ParentOrderReference = parentOrder,
//                StopLossLimitOrderReference = stopLossLimitOrder,
//            };

//            stopLossLimitOrder.TargetProfitOrderReference = targetProfitOrder;
            
//            parentOrder.TargetProfitOrderReference = targetProfitOrder;
//            parentOrder.StopLossLimitOrderReference = stopLossLimitOrder;

//            if(!_orders.GetOrAdd(parentOrder.TradingSymbol, _ => new ConcurrentDictionary<Guid, TestOrderInfo>())
//                   .TryAdd(parentOrder.OrderNumber, parentOrder))
//            {
//                _logger.LogWarning("{0}:[CreateOrder] Ideally this should not be printed.", strategyName);               
//            }

//            _logger.LogInformation("{0}: Pending order count in order book [{1}]", Name, _orders.Sum(c => c.Value.Count));
//            return Task.CompletedTask;
//        }

//        public override Task ModifyOrder(string strategyName, ModifyOrder modifyOrder)
//        {
//            throw new NotImplementedException();
//        }

//        protected override Task OnUpdateInternal(StrategyOnTouchLineSnapshot input)
//        {            
//            var lastTradeprice = _lastTradePrice.AddOrUpdate(input.updates.TradingSymbol, input.updates.LastTradePrice, (_,_)=> input.updates.LastTradePrice);            

//            if (_orders.TryGetValue(input.updates.TradingSymbol, out ConcurrentDictionary<Guid, TestOrderInfo>? orderInfo) && orderInfo is not null)                
//            {
//                var orders = orderInfo.Values.Where(o =>
//                {
//                    if (o.TargetProfitOrderReference is not null)
//                    { //parent order or stop loss limit order
//                        if (o.TransactionType == TransactionType.Buy && lastTradeprice >= o.LimitPrice)
//                            return true;

//                        if (o.TransactionType == TransactionType.Sell && lastTradeprice <= o.LimitPrice)
//                            return true;

//                        return false;
//                    }

//                    if (o.StopLossLimitOrderReference is not null && o.ParentOrderReference is not null)
//                    { //target profit order
//                        if (o.TransactionType == TransactionType.Buy && lastTradeprice <= o.LimitPrice)
//                            return true;

//                        if (o.TransactionType == TransactionType.Sell && lastTradeprice >= o.LimitPrice)
//                            return true;

//                        return false;
//                    }
                    
//                    return false;
//                });
                
//                if (orders is not null && orders.Any())
//                {
//                    foreach (var order in orders)
//                    {
//                        if(order.ParentOrderReference is null)
//                        {
//                            //its a parent order, execute it.
//                            // place the stoploss and targetprofit order
//                            // remove the parent order
//                            // will print its details once stoploss limit or targetprofit order fulfilled.
//                            order.FillPrice = lastTradeprice;
//                            order.FillTime = DateTime.Now;

//                            _orders.GetOrAdd(order.TradingSymbol, _ => new ConcurrentDictionary<Guid, TestOrderInfo>())
//                                   .TryAdd(order.TargetProfitOrderReference!.OrderNumber, order.TargetProfitOrderReference);

//                            _orders.GetOrAdd(order.TradingSymbol, _ => new ConcurrentDictionary<Guid, TestOrderInfo>())
//                                   .TryAdd(order.StopLossLimitOrderReference!.OrderNumber, order.StopLossLimitOrderReference);

//                            _orders.GetOrAdd(order.TradingSymbol, _ => new ConcurrentDictionary<Guid, TestOrderInfo>())
//                                   .TryRemove(order.OrderNumber, out _);

//                            _logger.LogInformation("{0}: Pending order count in order book after parent order execution [{1}]", Name, _orders.Sum(c => c.Value.Count));
//                            continue;
//                        }

//                        var targetProfit = 0.0m;
//                        var stopLoss = 0.0m;
//                        if (order.TargetProfitOrderReference is null)
//                        {
//                            //its a targetProfitOrder only
//                            targetProfit = order.LimitPrice;
//                            stopLoss = order.StopLossLimitOrderReference!.LimitPrice;
//                        }

//                        if (order.StopLossLimitOrderReference is null)
//                        {
//                            //its a stop loss limit order only
//                            stopLoss = order.LimitPrice;
//                            targetProfit = order.TargetProfitOrderReference!.LimitPrice;
//                        }

//                        var str = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14}",
//                                       Name,
//                                       order.ParentOrderReference?.OrderCreationTime,
//                                       order.TradingSymbol,
//                                       order.Exchange,
//                                       order.Token,
//                                       order.ParentOrderReference?.LimitPrice,
//                                       order.ParentOrderReference?.Quantity,
//                                       order.ParentOrderReference?.FillTime,
//                                       order.ParentOrderReference?.FillPrice,
//                                       order.ParentOrderReference?.TransactionType,
//                                       stopLoss,
//                                       targetProfit,
//                                       DateTime.Now,
//                                       lastTradeprice,
//                                       (lastTradeprice - order.ParentOrderReference?.FillPrice) *  order.ParentOrderReference?.Quantity
//                                       );

//                        _writer.WriteLine(str);

//                        //remove both the target profit and stoploss limit order
//                        _orders.GetOrAdd(order.TradingSymbol, _ => new ConcurrentDictionary<Guid, TestOrderInfo>())
//                                   .TryRemove(order.OrderNumber, out _);

//                        _orders.GetOrAdd(order.TradingSymbol, _ => new ConcurrentDictionary<Guid, TestOrderInfo>())
//                                   .TryRemove(order.StopLossLimitOrderReference?.OrderNumber ??
//                                              order.TargetProfitOrderReference?.OrderNumber ?? Guid.Empty, out _);

//                        _logger.LogInformation("{0}: Pending order count in order book after child order execution [{1}]", Name, _orders.Sum(c => c.Value.Count));
//                    }
//                }
//            }
            
//            return Task.CompletedTask;
//        }

//        public virtual void Dispose()
//        {
//            DisposeAsyncCore().AsTask().GetAwaiter().GetResult(); // Safe sync fallback
//            GC.SuppressFinalize(this);
//        }

//        public virtual async ValueTask DisposeAsync()
//        {
//            await DisposeAsyncCore();
//            GC.SuppressFinalize(this);
//        }

//        private async ValueTask DisposeAsyncCore()
//        {
//            if (_disposed)
//                return;

//            _disposed = true;

//            // Dispose async resources
//            if (_writer is not null)
//                await _writer.DisposeAsync();
            

//            _logger.LogInformation("{0}: Disposed gracefully", GetType().Name);
//            // Dispose other sync-only resources here (e.g., timers, files)
//        }
//    }
//}
