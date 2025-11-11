using FlatTrade.AuthenticationManager;
using FlatTrade.Common.Helpers;
using FlatTrade.Common.Transport;
using FlatTrade.Common.Types.Base;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FlatTrade.OrderManager
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="authentication"></param>
    /// <param name="httpClient"></param>
    public class Order
    {
        private readonly ILogger<Order> _logger;
        private readonly Authentication _authentication;
        private readonly RestHttpClient _httpClient;
        public string UserId { get; } = string.Empty;
        public string AccountId { get; } = string.Empty;

        public Order(Api api, RestHttpClient httpClient, ILoggerFactory loggerFactory)
        {
             _authentication = api.Authentication;
            _httpClient = httpClient;
            _logger = loggerFactory.CreateLogger<Order>();

            var userResult = Task.Run(async () =>
            {
                var (userResult, eMsg) = await api.User.GetUserDetailsAsync().ConfigureAwait(false);
                if (userResult is null)
                    throw new InvalidOperationException($"Cannot fetch user details. {eMsg}");

               
                return userResult;
            }).GetAwaiter().GetResult();

            UserId = userResult.UserId;
            AccountId = userResult.AccountId;
        }

      //[Throttle]
        /// <summary>
        /// 
        /// </summary>
        /// <param name="placeGTTOrderRequest"></param>
        /// <returns></returns>
        public async virtual Task<(PlaceGttOrderResponse?, string)> PlaceGTTOrderAsync(PlaceGttOrderRequest placeGTTOrderRequest)
        {
            var (accessTokenResult, eMsg) = await _authentication.GetAccessTokenAsync();
            if (accessTokenResult is null)
            {
                eMsg = $"Cannot place GTT order. {eMsg}";
                return (default, eMsg);
            }

            placeGTTOrderRequest.UserId = accessTokenResult.ClientCode; // Ensure UserId is set from access token
            placeGTTOrderRequest.AccountId = accessTokenResult.ClientCode; // Ensure AccountId is set from access token
            var serializedPlaceGttOrderRequest = JsonConvert.SerializeObject(placeGTTOrderRequest);

            string requestParams = $"jData={serializedPlaceGttOrderRequest}&jKey={Uri.EscapeDataString(accessTokenResult.AccessToken)}";
            return await _httpClient.PostMessageAsync<PlaceGttOrderResponse>(EndPoints.PlaceGttOrderUrl, requestParams);
        }

      //[Throttle]
        /// <summary>
        /// 
        /// </summary>
        /// <param name="modifyGTTOrderRequest"></param>
        /// <returns></returns>
        public async virtual Task<(ModifyGttOrderResponse?, string)> ModifyGTTOrderAsync(ModifyGttOrderRequest modifyGTTOrderRequest)
        {
            var (accessTokenResult, eMsg) = await _authentication.GetAccessTokenAsync();
            if (accessTokenResult is null)
            {
                eMsg = $"Cannot modify GTT order. {eMsg}";
                return (default, eMsg);
            }

            modifyGTTOrderRequest.UserId = accessTokenResult.ClientCode; // Ensure UserId is set from access token
            modifyGTTOrderRequest.AccountId = accessTokenResult.ClientCode; // Ensure AccountId is set from access token

            var serializedModifyGttOrderRequest = JsonConvert.SerializeObject(modifyGTTOrderRequest);

            string requestParams = $"jData={serializedModifyGttOrderRequest}&jKey={Uri.EscapeDataString(accessTokenResult.AccessToken)}";
            return await _httpClient.PostMessageAsync<ModifyGttOrderResponse>(EndPoints.ModifyGttOrderUrl, requestParams);
        }

      //[Throttle]
        /// <summary>
        /// 
        /// </summary>
        /// <param name="alertId"></param>
        /// <returns></returns>
        public async virtual Task<(CancelGttOrderResponse?, string)> CancelGTTOrderAsync(long alertId)
        {
            var (accessTokenResult, eMsg) = await _authentication.GetAccessTokenAsync();
            if (accessTokenResult is null)
            {
                eMsg = $"Cannot cancel GTT order. {eMsg}";
                return (default, eMsg);
            }

            var cancelGTTOrderResponse = new CancelGttOrderRequest
            {
                UserId = accessTokenResult.ClientCode, // Ensure UserId is set from access token
                AlertId = alertId // Set the AlertId for cancellation
            };

            var serializedCancelGttOrderResponse = JsonConvert.SerializeObject(cancelGTTOrderResponse);

            string requestParams = $"jData={serializedCancelGttOrderResponse}&jKey={Uri.EscapeDataString(accessTokenResult.AccessToken)}";
            return await _httpClient.PostMessageAsync<CancelGttOrderResponse>(EndPoints.CancelGttOrderUrl, requestParams);
        }

      //[Throttle]
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async virtual Task<(IEnumerable<PendingGttOrderResponse>?, string)> GetPendingGTTOrderAsync()
        {
            var (accessTokenResult, eMsg) = await _authentication.GetAccessTokenAsync();
            if (accessTokenResult is null)
            {
                eMsg = $"Cannot fetch pending GTT order. {eMsg}";
                return (default, eMsg);
            }

            var pendingGTTOrderRequest = new PendingGttOrderRequest
            {
                UserId = accessTokenResult.ClientCode, // Ensure UserId is set from access token                
            };

            var serializedPendingGttOrderRequest = JsonConvert.SerializeObject(pendingGTTOrderRequest);

            string requestParams = $"jData={serializedPendingGttOrderRequest}&jKey={Uri.EscapeDataString(accessTokenResult.AccessToken)}";
            return await _httpClient.PostMessageAsync<IEnumerable<PendingGttOrderResponse>>(EndPoints.PendingGttOrderUrl, requestParams);

        }

      //[Throttle]
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async virtual Task<(EnabledGTTsResponse?, string)> GetEnabledGTTsAsync()
        {
            var (accessTokenResult, eMsg) = await _authentication.GetAccessTokenAsync();
            if (accessTokenResult is null)
            {
                eMsg = $"Cannot fetch enabled GTTs. {eMsg}";
                return (default, eMsg);
            }

            var enabledGTTsRequest = new EnabledGTTsRequest
            {
                UserId = accessTokenResult.ClientCode, // Ensure UserId is set from access token
            };

            var serializedEnabledGttsRequest = JsonConvert.SerializeObject(enabledGTTsRequest);

            string requestParams = $"jData={serializedEnabledGttsRequest}&jKey={Uri.EscapeDataString(accessTokenResult.AccessToken)}";
            return await _httpClient.PostMessageAsync<EnabledGTTsResponse>(EndPoints.EnabledGttsUrl, requestParams);
        }      

      ////[Throttle]
      //  /// <summary>
      //  /// 
      //  /// </summary>
      //  /// <param name="modifyOcoOrderRequest"></param>
      //  /// <returns></returns>
      //  public async virtual Task<(ModifyOcoOrderResponse?, string)> ModifyOCOOrderAsync(ModifyOcoOrderRequest modifyOcoOrderRequest)
      //  {
      //      var (accessTokenResult, eMsg) = await _authentication.GetAccessTokenAsync();
      //      if (accessTokenResult is null)
      //      {
      //          eMsg = $"Cannot modify OCO order. {eMsg}";
      //          return (default, eMsg);
      //      }

      //      modifyOcoOrderRequest.UserId = accessTokenResult.ClientCode;
      //      modifyOcoOrderRequest.AccountId = accessTokenResult.ClientCode; // Ensure AccountId is set from access token

      //      modifyOcoOrderRequest.PlaceOrderParameters.ForEach(param =>
      //      {
      //          param.UserId = accessTokenResult.ClientCode;
      //          param.AccountId = accessTokenResult.ClientCode; // Ensure AccountId is set from access token
      //      });

      //      var serializedModifyOcoOrderRequest = JsonConvert.SerializeObject(modifyOcoOrderRequest);

      //      string requestParams = $"jData={serializedModifyOcoOrderRequest}&jKey={Uri.EscapeDataString(accessTokenResult.AccessToken)}";
      //      return await _httpClient.PostMessageAsync<ModifyOcoOrderResponse>(EndPoints.ModifyCOOrderUrl, requestParams);

      //  }

        //[Throttle]
        /// <summary>
        /// 
        /// </summary>
        /// <param name = "alertId" ></ param >
        /// < returns ></ returns >
        //public async virtual Task<(CancelOcoOrderResponse?, string)> CancelOCOOrderAsync(long alertId)
        //{
        //    var (accessTokenResult, eMsg) = await _authentication.GetAccessTokenAsync();
        //    if (accessTokenResult is null)
        //    {
        //        eMsg = $"Cannot cancel OCO order. {eMsg}";
        //        return (default, eMsg);
        //    }

        //    var cancelOcoOrderRequest = new CancelOcoOrderRequest
        //    {
        //        UserId = accessTokenResult.ClientCode, // Ensure UserId is set from access token
        //        AlertId = alertId // Set the AlertId for cancellation   
        //    };

        //    var serializedCancelOcoOrderRequest = JsonConvert.SerializeObject(cancelOcoOrderRequest);

        //    string requestParams = $"jData={serializedCancelOcoOrderRequest}&jKey={Uri.EscapeDataString(accessTokenResult.AccessToken)}";
        //    return await _httpClient.PostMessageAsync<CancelOcoOrderResponse>(EndPoints.CancelCOOrderUrl, requestParams);
        //}

        //[Throttle]
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="orderNumber"></param>
        /// <returns></returns>
        public async virtual Task<(IEnumerable<SingleOrderHistoryResponse>?, string)> GetSingleOrderHistoryAsync(long orderNumber)
        {
            var (accessTokenResult, eMsg) = await _authentication.GetAccessTokenAsync();
            if (accessTokenResult is null)
            {
                eMsg = $"Cannot fetch single order history. {eMsg}";
                return (default, eMsg);
            }

            var singleOrderHistoryRequest = new SingleOrderHistoryRequest
            {
                UserId = accessTokenResult.ClientCode, // Ensure UserId is set from access token
                NorenOrderNumber = orderNumber
            };

            var serializedSingleOrderHistory = JsonConvert.SerializeObject(singleOrderHistoryRequest);

            string requestParams = $"jData={serializedSingleOrderHistory}&jKey={Uri.EscapeDataString(accessTokenResult.AccessToken)}";
            return await _httpClient.PostMessageAsync<IEnumerable<SingleOrderHistoryResponse>>(EndPoints.SingleOrderHistoryUrl, requestParams);
        }

      //[Throttle]
        /// <summary>
        /// 
        /// </summary>
        /// <param name="productType"></param>
        /// <returns></returns>
        public async virtual Task<(MultiLegOrderBookResponse?, string)> GetMultiLegOrderBookAsync(ProductType productType)
        {
            var (accessTokenResult, eMsg) = await _authentication.GetAccessTokenAsync();
            if (accessTokenResult is null)
            {
                eMsg = $"Cannot fetch multi-leg order book. {eMsg}";
                return (default, eMsg);
            }

            var multiLegOrderBookRequest = new MultiLegOrderBookRequest
            {
                UserId = accessTokenResult.ClientCode, // Ensure UserId is set from access token
                ProductType = productType
            };

            var serializedMultiLegOrderBook = JsonConvert.SerializeObject(multiLegOrderBookRequest);

            string requestParams = $"jData={serializedMultiLegOrderBook}&jKey={Uri.EscapeDataString(accessTokenResult.AccessToken)}";
            return await _httpClient.PostMessageAsync<MultiLegOrderBookResponse>(EndPoints.OrderBookUrl, requestParams);
        }

      //[Throttle]
        /// <summary>
        /// 
        /// </summary>
        /// <param name="productType"></param>
        /// <returns></returns>
        public async virtual Task<(IEnumerable<OrderBookResponse>?, string)> GetOrderBookAsync()
        {
            var (accessTokenResult, eMsg) = await _authentication.GetAccessTokenAsync();
            if (accessTokenResult is null)
            {
                eMsg = $"Cannot fetch order book. {eMsg}";
                return (default, eMsg);
            }

            var orderBook = new OrderBookRequest { UserId = accessTokenResult.ClientCode };
            var serializedOrderBook = JsonConvert.SerializeObject(orderBook);

            string requestParams = $"jData={serializedOrderBook}&jKey={Uri.EscapeDataString(accessTokenResult.AccessToken)}";
            return await _httpClient.PostMessageAsync<IEnumerable<OrderBookResponse>>(EndPoints.OrderBookUrl, requestParams);
        }

      //[Throttle]
        /// <summary>
        /// 
        /// </summary>
        /// <param name="orderMarginRequest"></param>
        /// <returns></returns>
        public async virtual Task<(OrderMarginResponse?, string)> GetOrderMarginAsync(OrderMarginRequest orderMarginRequest)
        {
            var (accessTokenResult, eMsg) = await _authentication.GetAccessTokenAsync();
            if (accessTokenResult is null)
            {
                eMsg = $"Cannot fetch order margin. {eMsg}";
                return (default, eMsg);
            }

            orderMarginRequest.UserId = accessTokenResult.ClientCode; // Ensure UserId is set from access token
            orderMarginRequest.AccountId = accessTokenResult.ClientCode;
            var serializedOrderMargin = JsonConvert.SerializeObject(orderMarginRequest);

            string requestParams = $"jData={serializedOrderMargin}&jKey={Uri.EscapeDataString(accessTokenResult.AccessToken)}";
            return await _httpClient.PostMessageAsync<OrderMarginResponse>(EndPoints.OrderMarginUrl, requestParams);
        }

      //[Throttle]
        /// <summary>
        /// 
        /// </summary>
        /// <param name="basketMarginRequest"></param>
        /// <returns></returns>
        public async virtual Task<(BasketMarginResponse?, string)> GetBasketMarginAsync(BasketMarginRequest basketMarginRequest)
        {
            var (accessTokenResult, eMsg) = await _authentication.GetAccessTokenAsync();
            if (accessTokenResult is null)
            {
                eMsg = $"Cannot fetch basket margin. {eMsg}";
                return (default, eMsg);
            }

            basketMarginRequest.UserId = accessTokenResult.ClientCode; // Ensure UserId is set from access token
            basketMarginRequest.AccountId = accessTokenResult.ClientCode;
            var serializedBasketMargin = JsonConvert.SerializeObject(basketMarginRequest);

            string requestParams = $"jData={serializedBasketMargin}&jKey={Uri.EscapeDataString(accessTokenResult.AccessToken)}";
            return await _httpClient.PostMessageAsync<BasketMarginResponse>(EndPoints.BasketMarginUrl, requestParams);
        }        

        ////[Throttle]
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="placeCOOrderRequest"></param>
        ///// <returns></returns>
        //public async virtual Task<(PlaceCOOrderResponse?, string)> PlaceCOOrderAsync(PlaceCOOrderRequest placeCOOrderRequest)
        //{
        //    var (accessTokenResult, eMsg) = await _authentication.GetAccessTokenAsync();
        //    if (accessTokenResult is null)
        //    {
        //        eMsg = $"Cannot place order. {eMsg}";
        //        return (default, eMsg);
        //    }

        //    placeCOOrderRequest.UserId = accessTokenResult.ClientCode; // Ensure UserId is set from access token
        //    placeCOOrderRequest.AccountId = accessTokenResult.ClientCode;
        //    var serializedPlaceCOOrder = JsonConvert.SerializeObject(placeCOOrderRequest);

        //    string requestParams = $"jData={serializedPlaceCOOrder}&jKey={Uri.EscapeDataString(accessTokenResult.AccessToken)}";
        //    return await _httpClient.PostMessageAsync<PlaceCOOrderResponse>(EndPoints.PlaceCOOrderUrl, requestParams);
        //}

        //[Throttle]
        /// <summary>
        /// 
        /// </summary>
        /// <param name="placeOrderRequest"></param>
        /// <returns></returns>
        public async virtual Task<(PlaceOrderResponse?, string)> PlaceOrderAsync(PlaceOrderRequest placeOrderRequest)
        {
            var (accessTokenResult, eMsg) = await _authentication.GetAccessTokenAsync();
            if (accessTokenResult is null)
            {
                eMsg = $"Cannot place order. {eMsg}";
                return (default, eMsg);
            }

            placeOrderRequest.UserId = accessTokenResult.ClientCode; // Ensure UserId is set from access token
            placeOrderRequest.AccountId = accessTokenResult.ClientCode;
            var serializedPlaceOrder = JsonConvert.SerializeObject(placeOrderRequest);

            string requestParams = $"jData={serializedPlaceOrder}&jKey={Uri.EscapeDataString(accessTokenResult.AccessToken)}";
            return await _httpClient.PostMessageAsync<PlaceOrderResponse>(EndPoints.PlaceOrderUrl, requestParams);
        }

      //[Throttle]
        /// <summary>
        /// 
        /// </summary>
        /// <param name="modifyOrderRequest"></param>
        /// <returns></returns>
        public async virtual Task<(ModifyOrderResponse?, string)> ModifyOrderAsync(ModifyOrderRequest modifyOrderRequest)
        {
            var (accessTokenResult, eMsg) = await _authentication.GetAccessTokenAsync();
            if (accessTokenResult is null)
            {
                eMsg = $"Cannot modify order. {eMsg}";
                return (default, eMsg);
            }

            modifyOrderRequest.UserId = accessTokenResult.ClientCode; // Ensure UserId is set from access token
            var serializedModifyOrder = JsonConvert.SerializeObject(modifyOrderRequest);

            string requestParams = $"jData={serializedModifyOrder}&jKey={Uri.EscapeDataString(accessTokenResult.AccessToken)}";
            return await _httpClient.PostMessageAsync<ModifyOrderResponse>(EndPoints.ModifyOrderUrl, requestParams);
        }

      //[Throttle]
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancelOrderNumber"></param>
        /// <returns></returns>
        public async virtual Task<(CancelOrderResponse?, string)> CancelOrderAsync(long cancelOrderNumber)
        {
            var (accessTokenResult, eMsg) = await _authentication.GetAccessTokenAsync();
            if (accessTokenResult is null)
            {
                eMsg = $"Cannot cancel order. {eMsg}";
                return (default, eMsg);
            }
            var cancelOrderRequest = new CancelOrderRequest
            {
                NorenOrderNumber = cancelOrderNumber,
                UserId = accessTokenResult.ClientCode, // Ensure UserId is set from access token            
            };

            var serializedCancelOrder = JsonConvert.SerializeObject(cancelOrderRequest);

            string requestParams = $"jData={serializedCancelOrder}&jKey={Uri.EscapeDataString(accessTokenResult.AccessToken)}";
            return await _httpClient.PostMessageAsync<CancelOrderResponse>(EndPoints.CancelOrderUrl, requestParams);
        }

      //[Throttle]
        /// <summary>
        /// 
        /// </summary>
        /// <param name="exitOrderNumber"></param>
        /// <param name="productType"></param>
        /// <returns></returns>
        public async virtual Task<(ExitSnoOrderResponse?, string)> ExitSnoOrderOrderAsync(long exitOrderNumber, ProductType productType)
        {
            var (accessTokenResult, eMsg) = await _authentication.GetAccessTokenAsync();
            if (accessTokenResult is null)
            {
                eMsg = $"Cannot exit order. {eMsg}";
                return (default, eMsg);
            }
            var exitSnoOrderRequest = new ExitSnoOrderRequest
            {
                NorenOrderNumber = exitOrderNumber,
                ProductType = productType,
                UserId = accessTokenResult.ClientCode, // Ensure UserId is set from access token            
            };

            var serializedExistSnoOrder = JsonConvert.SerializeObject(exitSnoOrderRequest);

            string requestParams = $"jData={serializedExistSnoOrder}&jKey={Uri.EscapeDataString(accessTokenResult.AccessToken)}";
            return await _httpClient.PostMessageAsync<ExitSnoOrderResponse>(EndPoints.ExitSnoOrderUrl, requestParams);
        }
    }
}
