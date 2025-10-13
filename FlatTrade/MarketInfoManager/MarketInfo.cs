using FlatTrade.AuthenticationManager;
using FlatTrade.Common.Helpers;
using FlatTrade.Common.Throttle;
using FlatTrade.Common.Transport;
using FlatTrade.Common.Types.Base;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FlatTrade.MarketInfoManager
{
    /// <summary>
    /// Provides methods for retrieving market-related data, such as top lists, index lists, time price data, end-of-day
    /// chart data, option chains, option greeks, exchange messages, broker messages, and span calculations.
    /// </summary>
    /// <remarks>This class acts as a client for interacting with various market data endpoints. It requires
    /// valid authentication and a configured HTTP client to perform requests. Each method returns a tuple containing
    /// the requested data and an error message, if applicable.</remarks>
    /// <param name="authentication"></param>
    /// <param name="httpClient"></param>
    public class MarketInfo(Authentication authentication, RestHttpClient httpClient, ILoggerFactory loggerFactory)
    {
        private readonly ILogger<MarketInfo> _logger = loggerFactory.CreateLogger<MarketInfo>();
        private readonly Authentication _authentication = authentication;
        private readonly RestHttpClient _httpClient = httpClient;

      //[Throttle]
        /// <summary>
        /// 
        /// </summary>
        /// <param name="exchange"></param>
        /// <returns></returns>
        public async virtual Task<(TopListNamesResponse?, string)> GetTopListNamesAsync(Exchange exchange)
        {
            var (accessTokenResult, eMsg) = await _authentication.GetAccessTokenAsync();
            if (accessTokenResult is null)
            {
                eMsg = $"Cannot fetch top list names. {eMsg}";
                return (default, eMsg);
            }

            var topListNames = new TopListNamesRequest
            {
                UserId = accessTokenResult.ClientCode,
                Exchange = exchange
            };
            var serializedTopListNames = JsonConvert.SerializeObject(topListNames);

            string requestParams = $"jData={serializedTopListNames}&jKey={Uri.EscapeDataString(accessTokenResult.AccessToken)}";
            return await _httpClient.PostMessageAsync<TopListNamesResponse>(EndPoints.TopListNamesUrl, requestParams);
        }

      //[Throttle]
        /// <summary>
        /// 
        /// </summary>
        /// <param name="exchange"></param>
        /// <param name="basket"></param>
        /// <param name="criteria"></param>
        /// <param name="topOrBottom"></param>
        /// <returns></returns>
        public async virtual Task<(TopListResponse?, string)> GetTopListAsync(Exchange exchange, BasketType basket, Criteria criteria, TopBottomType topOrBottom)
        {
            var (accessTokenResult, eMsg) = await _authentication.GetAccessTokenAsync();
            if (accessTokenResult is null)
            {
                eMsg = $"Cannot fetch top list. {eMsg}";
                return (default, eMsg);
            }

            var topList = new TopListRequest
            {
                UserId = accessTokenResult.ClientCode,
                TopOrBottom = topOrBottom,
                BasketName = basket,
                Criteria = criteria,
                Exchange = exchange
            };
            var serializedTopList = JsonConvert.SerializeObject(topList);

            string requestParams = $"jData={serializedTopList}&jKey={Uri.EscapeDataString(accessTokenResult.AccessToken)}";
            return await _httpClient.PostMessageAsync<TopListResponse>(EndPoints.TopListUrl, requestParams);
        }

      //[Throttle]
        /// <summary>
        /// 
        /// </summary>
        /// <param name="exchange"></param>
        /// <returns></returns>
        public async virtual Task<(IndexListResponse?, string)> GetIndexListAsync(Exchange exchange)
        {
            var (accessTokenResult, eMsg) = await _authentication.GetAccessTokenAsync();
            if (accessTokenResult is null)
            {
                eMsg = $"Cannot fetch index list. {eMsg}";
                return (default, eMsg);
            }

            var indexList = new IndexListRequest
            {
                Exchange = exchange,
                UserId = accessTokenResult.ClientCode
            };
            var serializedIndexList = JsonConvert.SerializeObject(indexList);

            string requestParams = $"jData={serializedIndexList}&jKey={Uri.EscapeDataString(accessTokenResult.AccessToken)}";
            return await _httpClient.PostMessageAsync<IndexListResponse>(EndPoints.IndexListUrl, requestParams);
        }

      //[Throttle]
        /// <summary>
        /// 
        /// </summary>
        /// <param name="exchange"></param>
        /// <param name="tradngSymbol"></param>
        /// <param name="startDateTime"></param>
        /// <param name="endDateTime"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        public async virtual Task<(IEnumerable<TimePriceDataResponse>?, string)> GetTimePriceDataAsync(Exchange exchange, string tradngSymbol, DateTime startDateTime, DateTime endDateTime, ChartInterval interval)
        {
            var (accessTokenResult, eMsg) = await _authentication.GetAccessTokenAsync();
            if (accessTokenResult is null)
            {
                eMsg = $"Cannot access time price data. {eMsg}";
                return (default, eMsg);
            }

            var timePriceData = new TimePriceDataRequest
            {
                UserId = accessTokenResult.ClientCode,
                Exchange = exchange,
                TradingSymbol = Uri.EscapeDataString(tradngSymbol),
                EpochEndDateTime = (long)(endDateTime.Date - DateTime.UnixEpoch).TotalSeconds,
                EpochStartDateTime = (long)(startDateTime.Date - DateTime.UnixEpoch).TotalSeconds,
                Interval = interval
            };
            var serializedTimePriceData = JsonConvert.SerializeObject(timePriceData);

            string requestParams = $"jData={serializedTimePriceData}&jKey={Uri.EscapeDataString(accessTokenResult.AccessToken)}";
            return await _httpClient.PostMessageAsync<IEnumerable<TimePriceDataResponse>>(EndPoints.TimePriceDataUrl, requestParams);
        }

      //[Throttle]
        /// <summary>
        /// 
        /// </summary>
        /// <param name="exchange"></param>
        /// <param name="tradingSymbol"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <returns></returns>
        public async virtual Task<(IEnumerable<EodChartDataResponse>?, string)> GetEodChartDataAsync(Exchange exchange, string tradingSymbol, DateTime fromDate, DateTime toDate)
        {
            var (accessTokenResult, eMsg) = await _authentication.GetAccessTokenAsync();
            if (accessTokenResult is null)
            {
                eMsg = $"Cannot access EOD chart data. {eMsg}";
                return (default, eMsg);
            }

            var limits = new EodChartDataRequest
            {
                SymbolName = $"{exchange}:{Uri.EscapeDataString(tradingSymbol)}",
                EpochEndDateTime = (long)(toDate.Date - DateTime.UnixEpoch).TotalSeconds,
                EpochStartDateTime = (long)(fromDate.Date - DateTime.UnixEpoch).TotalSeconds,
            };
            var serializedUserDetails = JsonConvert.SerializeObject(limits);

            string requestParams = $"jData={serializedUserDetails}&jKey={Uri.EscapeDataString(accessTokenResult.AccessToken)}";
            return await _httpClient.PostMessageForDoubleSerializedAsync<EodChartDataResponse>(EndPoints.EodChartDataUrl, requestParams);
        }

      //[Throttle]
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tradingSymbol"></param>
        /// <param name="exchange"></param>
        /// <param name="strikePrice"></param>
        /// <param name="OneSideCountForPutAndCall"></param>
        /// <returns></returns>
        public async virtual Task<(IEnumerable<OptionChainDataResponse>?, string)> GetOptionChainAsync(string tradingSymbol, Exchange exchange, decimal strikePrice, int OneSideCountForPutAndCall)
        {
            var (accessTokenResult, eMsg) = await _authentication.GetAccessTokenAsync();
            if (accessTokenResult is null)
            {
                eMsg = $"Cannot access option chain. {eMsg}";
                return (default, eMsg);
            }
            var optionChainRequest = new OptionChainRequest
            {
                Exchange = exchange,
                TradingSymbol = Uri.EscapeDataString(tradingSymbol),
                OneSideCountForPutAndCall = OneSideCountForPutAndCall,
                StrikePrice = strikePrice,
                UserId = accessTokenResult.ClientCode
            };
            var serializedOptionChainRequest = JsonConvert.SerializeObject(optionChainRequest);

            string requestParams = $"jData={serializedOptionChainRequest}&jKey={Uri.EscapeDataString(accessTokenResult.AccessToken)}";
            var (resp, errormsg) = await _httpClient.PostMessageAsync<OptionChainResponse>(EndPoints.OptionChainUrl, requestParams);
            if (resp is null || resp.OptionChainDataResponse is null || !resp.OptionChainDataResponse.Any() || errormsg != Constants.StatusOk)
            {
                return (default, errormsg);
            }
            return (resp.OptionChainDataResponse, errormsg);
        }

      //[Throttle]
        /// <summary>
        /// 
        /// </summary>
        /// <param name="optionGreekRequest"></param>
        /// <returns></returns>
        public async virtual Task<(OptionGreekResponse?, string)> GetOptionGreekAsync(OptionGreekRequest optionGreekRequest)
        {
            var (accessTokenResult, eMsg) = await _authentication.GetAccessTokenAsync();
            if (accessTokenResult is null)
            {
                eMsg = $"Cannot access option greek. {eMsg}";
                return (default, eMsg);
            }


            var serializedOptionGreek = JsonConvert.SerializeObject(optionGreekRequest);

            string requestParams = $"jData={serializedOptionGreek}&jKey={Uri.EscapeDataString(accessTokenResult.AccessToken)}";
            return await _httpClient.PostMessageAsync<OptionGreekResponse>(EndPoints.OptionGreekUrl, requestParams);
        }

      //[Throttle]
        /// <summary>
        /// 
        /// </summary>
        /// <param name="exchange"></param>
        /// <returns></returns>
        public async virtual Task<(IEnumerable<ExchangeMessageResponse>?, string)> GetExchangeMessageAsync(Exchange exchange)
        {
            var (accessTokenResult, eMsg) = await _authentication.GetAccessTokenAsync();
            if (accessTokenResult is null)
            {
                eMsg = $"Cannot access exchange message. {eMsg}";
                return (default, eMsg);
            }

            var exchangeMessage = new ExchangeMessageRequest
            {
                UserId = accessTokenResult.ClientCode,
                Exchange = exchange
            };
            var serializedExchangeMessage = JsonConvert.SerializeObject(exchangeMessage);

            string requestParams = $"jData={serializedExchangeMessage}&jKey={Uri.EscapeDataString(accessTokenResult.AccessToken)}";
            return await _httpClient.PostMessageAsync<IEnumerable<ExchangeMessageResponse>>(EndPoints.ExchangeMessageUrl, requestParams);
        }

      //[Throttle]
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async virtual Task<(IEnumerable<BrokerMessageResponse>?, string)> GetBrokerMessageAsync()
        {
            var (accessTokenResult, eMsg) = await _authentication.GetAccessTokenAsync();
            if (accessTokenResult is null)
            {
                eMsg = $"Cannot access broker message. {eMsg}";
                return (default, eMsg);
            }

            var brokerMessageRequest = new BrokerMessageRequest
            {
                UserId = accessTokenResult.ClientCode
            };
            var serializedBrokerMessageRequest = JsonConvert.SerializeObject(brokerMessageRequest);

            string requestParams = $"jData={serializedBrokerMessageRequest}&jKey={Uri.EscapeDataString(accessTokenResult.AccessToken)}";
            return await _httpClient.PostMessageAsync<IEnumerable<BrokerMessageResponse>>(EndPoints.BrokerMessageUrl, requestParams);
        }

      //[Throttle]
        /// <summary>
        /// 
        /// </summary>
        /// <param name="positions"></param>
        /// <returns></returns>
        public async virtual Task<(IEnumerable<SpanCalculatorResponse>?, string)> SpanCalculatorAsync(IEnumerable<PositionRequest> positions)
        {
            var (accessTokenResult, eMsg) = await _authentication.GetAccessTokenAsync();
            if (accessTokenResult is null)
            {
                eMsg = $"Cannot access span calculator. {eMsg}";
                return (default, eMsg);
            }

            var spanCalculator = new SpanCalculatorRequest
            {
                AccountId = accessTokenResult.ClientCode,
                Positions = [.. positions]
            };
            var serializedSpanCalculator = JsonConvert.SerializeObject(spanCalculator);

            string requestParams = $"jData={serializedSpanCalculator}&jKey={Uri.EscapeDataString(accessTokenResult.AccessToken)}";
            return await _httpClient.PostMessageAsync<IEnumerable<SpanCalculatorResponse>>(EndPoints.SpanCalculatorUrl, requestParams);
        }
    

      //[Throttle]
        /// <summary>
        /// 
        /// </summary>
        /// <param name="positions"></param>
        /// <returns></returns>
        public async virtual Task<(IEnumerable<MarginEquityResponse>?, string)> GetMarginCalculatorEquitiesAsync()
        {
            string msg = string.Empty;
            var equities = new List<MarginEquityResponse>();
            try 
            { 
                var html = await _httpClient.GetNativeHttpClient().GetStringAsync(EndPoints.MarginCalculatorEquitiesUrl);
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                var rows = doc.DocumentNode.SelectNodes("//table//tr");
                if (rows == null || rows.Count <= 1)
                {
                    msg = $"MarginCalculatorEquitiesAsync: No table rows found on the page.";
                    _logger.LogError("{msg}", msg);
                    return (default, msg);
                }

                foreach (var row in rows.Skip(2)) // skip both header rows
                {
                    var cells = row.Elements("td").Select(td => td.InnerText.Trim()).ToList();
                    if (cells.Count < 6) continue;

                    equities.Add(new MarginEquityResponse
                    {
                        Symbol = cells[0],
                        Segment = cells[1],
                        MISMarginInPercentage = ParseDecimal(cells[2]),
                        MISLeverageX = ParseDecimal(cells[3]),
                        CNCMarginMISMarginInPercentage = ParseDecimal(cells[4]),
                        CNCLeverageX = ParseDecimal(cells[5])
                    });
                }

            }
            catch (HttpRequestException ex)
            {
                msg= $"MarginCalculatorEquitiesAsync: HTTP error: { ex.Message}";                
                _logger.LogError("NOK: {msg}", msg);
            }
            catch (TaskCanceledException)
            {
                msg = $"MarginCalculatorEquitiesAsync: Request timed out.";
                _logger.LogError("NOK: {msg}", msg);
            }
            catch (Exception ex)
            {
                msg = $"MarginCalculatorEquitiesAsync: Unexpected error: {ex.Message}";
                _logger.LogError("NOK: {msg}", msg);
            }
            return (equities, msg);
        }

        static decimal ParseDecimal(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return 0;
            input = input.Replace("%", "").Replace("X", "").Trim();
            return decimal.TryParse(input, out var result) ? result : 0;
        }
    }
}
