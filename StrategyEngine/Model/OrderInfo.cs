using FlatTrade.OrderManager;
using FlatTrade.SubscriptionManager.Order;
using FlatTrade.Types.Base;

namespace StrategyEngine.Model
{
    public class OrderInfo
    {
        public long NorenOrderNumber { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string AccountId { get; set; } = string.Empty;
        public long CancelledQuantity { get; set; }
        public Exchange Exchange { get; set; }
        public string TradingSymbol { get; set; } = string.Empty;
        public TransactionType TransactionType { get; set; }
        public decimal Quantity { get; set; }
        public decimal TriggerPrice { get; set; }
        public decimal Price { get; set; }
        public ProductType ProductType { get; set; }
        public string Remarks { get; set; } = string.Empty;
        public string RejectionReason { get; set; } = string.Empty;
        public OrderStatus OrderStatus { get; set; }
        public string ReportType { get; set; } = string.Empty;
        public PriceType PriceType { get; set; }  //LMT/MKT
        public RetentionType RetentionType { get; set; } //DAY/IOC/EOS
        public string ExchangeOrderNumber { get; set; } = string.Empty;
        public decimal DisclosedQuantity { get; set; }
        public DateTime ExchangeTime { get; set; }
        public bool Amo { get; set; } = false;
        public decimal BookProfitPrice { get; set; }
        public decimal BookLossPrice { get; set; }
        public decimal TrailingPrice { get; set; }
        public long FillQuantity { get; set; }
        public decimal FillPrice { get; set; }
        public long FillId { get; set; }
        public decimal AveragePriceOfTradedQuantity { get; set; }
        public DateTime FillDateTime { get; set; }
        public long TotalFilled { get; set; }
       
        public static IEnumerable<OrderInfo> ConvertFrom(IEnumerable<OrderBookResponse> orderBookResponse)
        {
            List<OrderInfo> orderInfo = [];

            foreach (var item in orderBookResponse)
            {
                orderInfo.Add(new OrderInfo
                {
                    NorenOrderNumber = item.NorenOrderNumber,
                    UserId = item.UserId,
                    AccountId = item.AccountId,
                    Exchange = item.Exchange,
                    TradingSymbol = item.TradingSymbol,
                    TransactionType = item.TransactionType,
                    Quantity = item.Quantity,
                    //TriggerPrice = item.Trigger,
                    Price = item.Price,
                    ProductType = item.ProductType,
                    RejectionReason = item.RejectionReason,
                    OrderStatus = item.OrderStatus,
                    //ReportType = item.ReportType,
                    PriceType = item.PriceType,
                    RetentionType = item.RetentionType,
                    ExchangeOrderNumber = item.ExchangeOrderNumber,
                    DisclosedQuantity = item.DisclosedQuantity,
                    ExchangeTime = item.ExchangeTime,
                    BookProfitPrice = item.BookProfitPrice,
                    BookLossPrice = item.BookLossPrice,
                    TrailingPrice = item.TrailingPrice,
                    //TotalFilled = item.Tot
                    //Amo = !string.IsNullOrEmpty(item.Amo),
                });
            }
            return orderInfo;
        }

        public static OrderInfo? ConvertFrom(OrderSubscriptionUpdates orderSubscriptionUpdates)
        {
            return orderSubscriptionUpdates is null ? default : new OrderInfo
            {
                NorenOrderNumber = orderSubscriptionUpdates.NorenOrderNumber,
                UserId = orderSubscriptionUpdates.UserId,
                AccountId = orderSubscriptionUpdates.AccountId,
                Exchange = orderSubscriptionUpdates.Exchange,
                TradingSymbol = orderSubscriptionUpdates.TradingSymbol,
                TransactionType = orderSubscriptionUpdates.TransactionType,
                CancelledQuantity = orderSubscriptionUpdates.CancelledQuantity,
                Quantity = orderSubscriptionUpdates.Quantity,
                TriggerPrice = orderSubscriptionUpdates.TriggerPrice,
                Price = orderSubscriptionUpdates.Price,
                ProductType = orderSubscriptionUpdates.ProductType,
                RejectionReason = orderSubscriptionUpdates.RejectionReason,
                OrderStatus = orderSubscriptionUpdates.OrderStatus,
                ReportType = orderSubscriptionUpdates.ReportType,
                PriceType = orderSubscriptionUpdates.PriceType,
                RetentionType = orderSubscriptionUpdates.RetentionType,
                ExchangeOrderNumber = orderSubscriptionUpdates.ExchangeOrderNumber,
                DisclosedQuantity = orderSubscriptionUpdates.DisclosedQuantity,
                FillId = orderSubscriptionUpdates.FillId,
                FillQuantity = orderSubscriptionUpdates.FillQuantity,
                FillPrice = orderSubscriptionUpdates.FillPrice,
                Remarks = orderSubscriptionUpdates.Remarks,
                AveragePriceOfTradedQuantity = orderSubscriptionUpdates.AvgPriceOfTradedQuantity,
                FillDateTime = orderSubscriptionUpdates.FillDateTime,
                ExchangeTime = orderSubscriptionUpdates.ExchangeTime,
                BookLossPrice = orderSubscriptionUpdates.BookLossPrice,
                BookProfitPrice = orderSubscriptionUpdates.BookProfitPrice,
                TrailingPrice = orderSubscriptionUpdates.TrailingPrice,
                TotalFilled = orderSubscriptionUpdates.TotalFilled,
                Amo = !string.IsNullOrEmpty(orderSubscriptionUpdates.Amo) && string.Compare(orderSubscriptionUpdates.Amo, "Yes", true) == 0
            };
        }
    }
}
