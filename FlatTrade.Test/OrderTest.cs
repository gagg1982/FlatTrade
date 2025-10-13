using FlatTrade.Common.Types.Base;
using FlatTrade.OrderManager;

namespace FlatTrade.Test
{
    internal static class OrderTest
    {
        public static async Task Execute(Api api)
        {
            var req = new PlaceGttOrderRequest
            {
                AccountId = string.Empty,           // Api will overwrite
                UserId = string.Empty,              // Api will overwrite

                Exchange = Exchange.NSE,
                AlertType = GTTAlertType.LtpGreaterThan,
                DataToBeComparedWithLTP = 300.00m,
                DisclosedQuantity = 1,
                PriceType = PriceType.StopLossMarket,
                ProductType = ProductType.Delivery,
                TransactionType = TransactionType.Buy,
                Validity = RetentionType.GTT,
                TriggeringPrice = 300.00m,

                Price = 400.10m,
                Quantity = 2,
                Remarks = "Test GTT Order",
                RetentionType = RetentionType.GTT,
                TradingSymbol = "ETERNAL-EQ",
            };

            var (resp, _) = await PlaceGTTOrderTestAsync(api, req);
            var (_, _) = await GetPendingGTTOrderTestAsync(api);

            var modifyGTTOrderRequest = new ModifyGttOrderRequest
            {
                AccountId = string.Empty,           // Api will overwrite
                UserId = string.Empty,              // Api will overwrite
                AlertId = 25060700000011,
                Exchange = Exchange.NSE,
                AlertType = GTTAlertType.LtpGreaterThan,
                DataToBeComparedWithLTP = 350.00m,
                DisclosedQuantity = 1,
                PriceType = PriceType.StopLossMarket,
                ProductType = ProductType.Delivery,
                TransactionType = TransactionType.Buy,
                Validity = RetentionType.GTT,
                TriggeringPrice = 350.00m,
                Price = 400.10m,
                Quantity = 2,
                Remarks = "Modified GTT Order",
                RetentionType = RetentionType.GTT,
                TradingSymbol = "ETERNAL-EQ",
            };
            var (_, _) = await ModifyGTTOrderTestAsync(api, modifyGTTOrderRequest);

            if (resp is not null)
            {
                modifyGTTOrderRequest.AlertId = resp.AlertId;
                modifyGTTOrderRequest.DataToBeComparedWithLTP = req.DataToBeComparedWithLTP + 16.23m;
                modifyGTTOrderRequest.DisclosedQuantity = req.DisclosedQuantity * 2;
                modifyGTTOrderRequest.PriceType = PriceType.Limit;
                modifyGTTOrderRequest.ProductType = ProductType.IntraDay;
                modifyGTTOrderRequest.TransactionType = req.TransactionType;
                modifyGTTOrderRequest.TriggeringPrice = req.TriggeringPrice + 12.00m;

                modifyGTTOrderRequest.Price = req.Price + 11.98m;
                modifyGTTOrderRequest.Quantity = req.Quantity * 2;
                modifyGTTOrderRequest.Remarks = "Test GTT Order modified";
                modifyGTTOrderRequest.TradingSymbol = req.TradingSymbol;

                var (_, _) = await ModifyGTTOrderTestAsync(api, modifyGTTOrderRequest);
            }

            var (resp1, _) = await GetPendingGTTOrderTestAsync(api);
            resp1?.ToList().ForEach(order => CancelGTTOrderTestAsync(api, order.AlertId).GetAwaiter().GetResult());

            var (_, _) = await CancelGTTOrderTestAsync(api, 25060700000011);

            //var (_, _) = await PlaceOCOOrderTestAsync(api, placeOCOOrderRequest);
            //var (_, _) = await ModifyOCOOrderTestAsync(api, modifyOcoOrderRequest);
            //var (_, _) = await CancelOCOOrderTestAsync(api, alertId);

            var (_, _) = await GetOrderBookTestAsync(api);

            //var (_, _) = await GetSingleOrderHistoryTestAsync(api, orderNumber);
            //var (_, _) = await GetMultiLegOrderBookTestAsync(api, productType );

            //var (_, _) = await GetOrderMarginTestAsync(api, orderMarginRequest);
            //var (_, _) = await GetBasketMarginTestAsync(api, basketMarginRequest);

            //var (_, _) = await PlaceOrderTestAsync(api, placeOrderRequest);
            //var (_, _) = await ModifyOrderTestAsync(api, modifyOrderRequest);
            //var (_, _) = await CancelOrderTestAsync(api, cancelOrderNumber);
            //var (_, _) = await ExitSnoOrderOrderTestAsync(api, exitOrderNumber, productType);
        }

        public static async Task<(PlaceGttOrderResponse?, string)> PlaceGTTOrderTestAsync(Api api, PlaceGttOrderRequest placeGTTOrderRequest)
        {
            var (response, eMsg) = await api.Order.PlaceGTTOrderAsync(placeGTTOrderRequest);
            if (response is null)
            {
                await Console.Error.WriteLineAsync($"Failed PlaceGTTOrderTestAsync : {eMsg}");
            }
            else
            {
                Console.WriteLine($"Passed PlaceGTTOrderTestAsync : OrderId/AlertId [{response.AlertId}], status [{response.Status}]");
            }
            return (response, eMsg);
        }

        public static async Task<(ModifyGttOrderResponse?, string)> ModifyGTTOrderTestAsync(Api api, ModifyGttOrderRequest modifyGTTOrderRequest)
        {
            var (response, eMsg) = await api.Order.ModifyGTTOrderAsync(modifyGTTOrderRequest);
            if (response is null)
            {
                await Console.Error.WriteLineAsync($"Failed ModifyGTTOrderTestAsync : {eMsg}");
            }
            else
            {
                Console.WriteLine($"Passed ModifyGTTOrderTestAsync : AlertId [{response.AlertId}], status [{response.Status}]");
            }
            return (response, eMsg);
        }

        public static async Task<(CancelGttOrderResponse?, string)> CancelGTTOrderTestAsync(Api api, long alertId)
        {
            var (response, eMsg) = await api.Order.CancelGTTOrderAsync(alertId);
            if (response is null)
            {
                await Console.Error.WriteLineAsync($"Failed CancelGTTOrderTestAsync : {eMsg}");
            }
            else
            {
                Console.WriteLine($"Passed CancelGTTOrderTestAsync : Alert cancelled [{response.AlertId}], status [{response.Status}]");
            }
            return (response, eMsg);
        }

        public static async Task<(IEnumerable<PendingGttOrderResponse>?, string)> GetPendingGTTOrderTestAsync(Api api)
        {
            var (response, eMsg) = await api.Order.GetPendingGTTOrderAsync();
            if (response is null)
            {
                await Console.Error.WriteLineAsync($"Failed GetPendingGTTOrderTestAsync : {eMsg}");
            }
            else
            {
                response.ToList().ForEach(order => Console.WriteLine($"Passed GetPendingGTTOrderTestAsync : Trading Symbol [{order.TradingSymbol}({order.Token})], AlertId [{order.AlertId}], RetentionType[{order.RetentionType}], Condition[{order.AlertType}({order.DataToBeComparedWithLTP})]"));
            }
            return (response, eMsg);
        }

        //public static async Task<(PlaceOcoOrderResponse?, string)> PlaceOCOOrderTestAsync(Api api, PlaceOcoOrderRequest placeOCOOrderRequest)
        //{
        //    var (response, eMsg) = await api.Order.PlaceOCOOrderAsync(placeOCOOrderRequest);
        //    if (response is null)
        //    {
        //        await Console.Error.WriteLineAsync($"Failed PlaceOCOOrderTestAsync : {eMsg}");
        //    }
        //    else
        //    {
        //        Console.WriteLine($"Passed PlaceOCOOrderTestAsync : Total margin [{response.MarginCashAvailable}], Current used margin [{response.TotalMarginUsedToday}], Available Margin [{response.MarginCashAvailable - response.TotalMarginUsedToday}]");
        //    }
        //    return (response, eMsg);
        //}

        //public static async Task<(ModifyOcoOrderResponse?, string)> ModifyOCOOrderTestAsync(Api api, ModifyOcoOrderRequest modifyOcoOrderRequest)
        //{
        //    var (response, eMsg) = await api.Order.ModifyOCOOrderAsync(modifyOcoOrderRequest);
        //    if (response is null)
        //    {
        //        await Console.Error.WriteLineAsync($"Failed ModifyOCOOrderTestAsync : {eMsg}");
        //    }
        //    else
        //    {
        //        Console.WriteLine($"Passed ModifyOCOOrderTestAsync : Total margin [{response.MarginCashAvailable}], Current used margin [{response.TotalMarginUsedToday}], Available Margin [{response.MarginCashAvailable - response.TotalMarginUsedToday}]");
        //    }
        //    return (response, eMsg);
        //}

        //public static async Task<(CancelOcoOrderResponse?, string)> CancelOCOOrderTestAsync(Api api, long alertId)
        //{
        //    var (response, eMsg) = await api.Order.CancelOCOOrderAsync(alertId);
        //    if (response is null)
        //    {
        //        await Console.Error.WriteLineAsync($"Failed CancelOCOOrderTestAsync : {eMsg}");
        //    }
        //    else
        //    {
        //        Console.WriteLine($"Passed CancelOCOOrderTestAsync : Total margin [{response.MarginCashAvailable}], Current used margin [{response.TotalMarginUsedToday}], Available Margin [{response.MarginCashAvailable - response.TotalMarginUsedToday}]");
        //    }
        //    return (response, eMsg);
        //}

        //public static async Task<(SingleOrderHistoryResponse?, string)> GetSingleOrderHistoryTestAsync(Api api, long orderNumber)
        //{
        //    var (response, eMsg) = await api.Order.GetSingleOrderHistoryAsync(orderNumber);
        //    if (response is null)
        //    {
        //        await Console.Error.WriteLineAsync($"Failed GetSingleOrderHistoryTestAsync : {eMsg}");
        //    }
        //    else
        //    {
        //        Console.WriteLine($"Passed GetSingleOrderHistoryTestAsync : Total margin [{response.MarginCashAvailable}], Current used margin [{response.TotalMarginUsedToday}], Available Margin [{response.MarginCashAvailable - response.TotalMarginUsedToday}]");
        //    }
        //    return (response, eMsg);
        //}

        //public static async Task<(MultiLegOrderBookResponse?, string)> GetMultiLegOrderBookTestAsync(Api api, ProductType productType)
        //{
        //    var (response, eMsg) = await api.Order.GetMultiLegOrderBookAsync(productType);
        //    if (response is null)
        //    {
        //        await Console.Error.WriteLineAsync($"Failed GetMultiLegOrderBookTestAsync : {eMsg}");
        //    }
        //    else
        //    {
        //        Console.WriteLine($"Passed GetMultiLegOrderBookTestAsync : Total margin [{response.MarginCashAvailable}], Current used margin [{response.TotalMarginUsedToday}], Available Margin [{response.MarginCashAvailable - response.TotalMarginUsedToday}]");
        //    }
        //    return (response, eMsg);
        //}

        public static async Task<(IEnumerable<OrderBookResponse>?, string)> GetOrderBookTestAsync(Api api)
        {
            var (response, eMsg) = await api.Order.GetOrderBookAsync();
            if (response is null)
            {
                await Console.Error.WriteLineAsync($"Failed GetOrderBookTestAsync : {eMsg}");
            }
            else
            {
                response.ToList().ForEach(order => Console.WriteLine($"Passed GetOrderBookTestAsync for [{order.TradingSymbol}(){order.Token}], Noren/Exchange Order no#[({order.NorenOrderNumber})({order.ExchangeOrderNumber})], Time[{order.EpochOrderEntryDateTime}], Side[{order.TransactionType}], Status[{order.Status}], Price/type/qty [{order.Price}/{order.PriceType}/{order.Quantity}], Product type[{order.ProductType}], Retention type[{order.RetentionType}]"));
            }
            return (response, eMsg);
        }

        //public  static async Task<(OrderMarginResponse?, string)> GetOrderMarginTestAsync(Api api, OrderMarginRequest orderMarginRequest)
        //{
        //    var (response, eMsg) = await api.Order.GetOrderMarginAsync(orderMarginRequest);
        //    if (response is null)
        //    {
        //        await Console.Error.WriteLineAsync($"Failed GetOrderMarginTestAsync : {eMsg}");
        //    }
        //    else
        //    {
        //        Console.WriteLine($"Passed GetOrderMarginTestAsync : Total margin [{response.MarginCashAvailable}], Current used margin [{response.TotalMarginUsedToday}], Available Margin [{response.MarginCashAvailable - response.TotalMarginUsedToday}]");
        //    }
        //    return (response, eMsg);
        //}

        //public  static async Task<(BasketMarginResponse?, string)> GetBasketMarginTestAsync(Api api, BasketMarginRequest basketMarginRequest)
        //{
        //    var (response, eMsg) = await api.Order.GetBasketMarginAsync(basketMarginRequest);
        //    if (response is null)
        //    {
        //        await Console.Error.WriteLineAsync($"Failed GetBasketMarginTestAsync : {eMsg}");
        //    }
        //    else
        //    {
        //        Console.WriteLine($"Passed GetBasketMarginTestAsync : Total margin [{response.MarginCashAvailable}], Current used margin [{response.TotalMarginUsedToday}], Available Margin [{response.MarginCashAvailable - response.TotalMarginUsedToday}]");
        //    }
        //    return (response, eMsg);
        //}

        //public  static async Task<(PlaceOrderResponse?, string)> PlaceOrderTestAsync(Api api, PlaceOrderRequest placeOrderRequest)
        //{
        //    var (response, eMsg) = await api.Order.PlaceOrderAsync(placeOrderRequest);
        //    if (response is null)
        //    {
        //        await Console.Error.WriteLineAsync($"Failed PlaceOrderTestAsync : {eMsg}");
        //    }
        //    else
        //    {
        //        Console.WriteLine($"Passed PlaceOrderTestAsync : Total margin [{response.MarginCashAvailable}], Current used margin [{response.TotalMarginUsedToday}], Available Margin [{response.MarginCashAvailable - response.TotalMarginUsedToday}]");
        //    }
        //    return (response, eMsg);
        //}

        //public static async Task<(ModifyOrderResponse?, string)> ModifyOrderTestAsync(Api api, ModifyOrderRequest modifyOrderRequest)
        //{
        //    var (response, eMsg) = await api.Order.ModifyOrderAsync(modifyOrderRequest);
        //    if (response is null)
        //    {
        //        await Console.Error.WriteLineAsync($"Failed ModifyOrderTestAsync : {eMsg}");
        //    }
        //    else
        //    {
        //        Console.WriteLine($"Passed ModifyOrderTestAsync : Total margin [{response.MarginCashAvailable}], Current used margin [{response.TotalMarginUsedToday}], Available Margin [{response.MarginCashAvailable - response.TotalMarginUsedToday}]");
        //    }
        //    return (response, eMsg);
        //}

        //public  static async Task<(CancelOrderResponse?, string)> CancelOrderTestAsync(Api api, long cancelOrderNumber)
        //{
        //    var (response, eMsg) = await api.Order.CancelOrderAsync(cancelOrderNumber);
        //    if (response is null)
        //    {
        //        await Console.Error.WriteLineAsync($"Failed CancelOrderTestAsync : {eMsg}");
        //    }
        //    else
        //    {
        //        Console.WriteLine($"Passed CancelOrderTestAsync : Total margin [{response.MarginCashAvailable}], Current used margin [{response.TotalMarginUsedToday}], Available Margin [{response.MarginCashAvailable - response.TotalMarginUsedToday}]");
        //    }
        //    return (response, eMsg);
        //}

        //public  static async Task<(ExitSnoOrderResponse?, string)> ExitSnoOrderOrderTestAsync(Api api, long exitOrderNumber, ProductType productType)
        //{
        //    var (response, eMsg) = await api.Order.ExitSnoOrderOrderAsync(exitOrderNumber, productType);
        //    if (response is null)
        //    {
        //        await Console.Error.WriteLineAsync($"Failed ExitSnoOrderOrderTestAsync : {eMsg}");
        //    }
        //    else
        //    {
        //        Console.WriteLine($"Passed ExitSnoOrderOrderTestAsync : Total margin [{response.MarginCashAvailable}], Current used margin [{response.TotalMarginUsedToday}], Available Margin [{response.MarginCashAvailable - response.TotalMarginUsedToday}]");
        //    }
        //    return (response, eMsg);
        //}

        //public async Task PlaceGTTOrder1Async()
        //{

        //    var req = new PlaceGttOrderRequest
        //    {
        //        AccountId = string.Empty,           // Api will overwrite
        //        UserId = string.Empty,              // Api will overwrite

        //        Exchange = Exchange.NSE,
        //        AlertType = AlertType.TOIGreaterThan,
        //        DataToBeComparedWithLTP = 1000.00m,
        //        DisclosedQuantity = 1,
        //        PriceType = PriceType.Limit,
        //        ProductType = ProductType.Delivery,
        //        TransactionType = TransactionType.Buy,
        //        Validity = RetentionType.GTT,

        //        Price = 1000.10m,
        //        Quantity = 2,
        //        Remarks = "Test GTT Order",
        //        RetentionType = RetentionType.GTT,
        //        TradingSymbol = "NTPC",
        //    };

        //    var (result, eMsg) = await _api.Order.PlaceGTTOrderAsync(req);
        //    if (result == null)
        //    {
        //        await Console.Error.WriteLineAsync($"Error placing GTT order: {eMsg}");
        //        return;
        //    }

        //    if(result.Status != Constants.StatusOk)
        //    {
        //        await Console.Error.WriteLineAsync($"Error placing GTT order: {result.ErrorMsg}");
        //        return;
        //    }

        //    Console.WriteLine($"GTT Order placed successfully. Alert ID: {result.AlertId}");
        //}
    }
}
