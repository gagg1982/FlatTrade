using FlatTrade;
using FlatTrade.Common.Helpers;
using FlatTrade.Common.Types;
using FlatTrade.Common.Types.Base;
using FlatTrade.HoldingsManager;
using FlatTrade.MarketInfoManager;
using FlatTrade.ScripManager;
using FlatTrade.TradeManager;
using Microsoft.Extensions.Logging;
using StrategyEngine.Helpers;
using StrategyEngine.Model;
using StrategyEngine.Strategy;
using System.Collections.Concurrent;

namespace StrategyEngine
{
    internal class DirectFromServer
    {
        private readonly Api _api;
        private readonly ILogger _logger;

        private event OnUpdate? OnCandles;
        private event OnUpdate? OnSecurities;
        private event OnUpdate? OnPositions;
        private event OnUpdate? OnHoldings;
        private event OnUpdate? OnTrades;
                
        public DirectFromServer(Api api, OnUpdate? onUpdate, ILogger logger)
        {
            _api = api;
            _logger = logger;

            OnCandles += onUpdate;
            OnSecurities += onUpdate;
            OnPositions += onUpdate;
            OnHoldings += onUpdate;
            OnTrades += onUpdate;
        }

        public async Task UpdateTradeDetails()
        {
            var trades = await GetTradeDetailsFromServerAsync();
            if (!trades.Any())
            {
                _logger.LogInformation("No trades found to update.");
                return;
            }

            foreach (var trade in trades!)
            {
                var details = GlobalDataSet.Data.GetOrAdd(trade.TradingSymbol, _ => new());
                details!.TradeInfo.AddOrUpdate(trade.Exchange, [trade], (_, existingValue) => 
                    {
                        lock (existingValue)
                        {
                            existingValue.Append(trade);
                            return existingValue;
                        }
                    });   //always replace            


                if(OnTrades is not null)
                    await OnTrades(new StrategyEvent
                    {
                        Exchange = trade.Exchange,
                        Token = trade.Token,
                        TradingSymbol = trade.TradingSymbol
                    });
            }
        }

        public async Task UpdateHoldingDetails()
        {
            var holdings = await GetHoldingDetailsFromServerAsync();
            if (!holdings.Any())
            {
                _logger.LogInformation("No trades found to update.");
                return;
            }

            foreach (var holding in holdings!)
            {
                foreach (var exch in holding.ExchangeSymbolResponse)
                {
                    var details = GlobalDataSet.Data.GetOrAdd(exch.TradingSymbol, _ => new());
                    details!.HoldingInfo.AddOrUpdate(exch.Exchange, holding, (key, existingValue) => holding);

                    if(OnHoldings is not null)
                        await OnHoldings(new StrategyEvent
                        {
                            Exchange = exch.Exchange,
                            Token = exch.Token,
                            TradingSymbol = exch.TradingSymbol
                        });
                }
            }
        }

        public async Task UpdatePositions()
        {
            var positions = await GetPositionsFromServerAsync();
            if (positions is null || !positions.Any())
            {
                _logger.LogInformation("No positions found to update.");
                return;
            }

            foreach (var position in positions!)
            {
                var details = GlobalDataSet.Data.GetOrAdd(position.TradingSymbol, _ => new());
                if (position.NetPositionQuantity == 0)
                {
                    details!.OpenPositions.Remove(position.ProductType, out PositionBookResponse? _);
                    details!.ClosedPositions.AddOrUpdate(position.ProductType, position, (_, _) => position); //always update to latest
                }
                else
                {
                    details!.ClosedPositions.Remove(position.ProductType, out PositionBookResponse? _);
                    details!.OpenPositions.AddOrUpdate(position.ProductType, position, (_, _) => position); //always update to latest
                }

                if (OnPositions is not null)
                    await OnPositions(new StrategyEvent
                    {
                        Exchange = position.Exchange,
                        Token = position.Token,
                        TradingSymbol = position.TradingSymbol
                    });
            }
        }

        private static ConcurrentDictionary<ChartInterval, IEnumerable<PriceCandle>> GenerateIntervalCandles(IEnumerable<ChartInterval> chartIntervals, IEnumerable<TimePriceDataResponse> oneMinutePriceData)
        {
            ConcurrentDictionary<ChartInterval, IEnumerable<PriceCandle>> resp = [];
            foreach (var timeInterval in chartIntervals)
                resp.TryAdd(timeInterval, Utility.AggregateCandles(oneMinutePriceData, (int)timeInterval));

            return resp;
        }        

        public async Task UpdateCandles(string tradingSymbol,Exchange exchange, long token, decimal price, long quantity, DateTime tradeDateTime)
        {            
            List<Task> tasks = [];           

            var details = GlobalDataSet.Data.GetOrAdd(tradingSymbol, _ => new Details());
            var intervalCandle = details.PriceCandleInfo.GetOrAdd(exchange, _ => new());
            
            foreach(var (interval,candle) in intervalCandle)
            {
                if (price == decimal.MinValue &&
                    intervalCandle.TryGetValue(interval, out SortedSet<PriceCandle>? lastPrice) &&
                    lastPrice is not null && 
                    lastPrice.Any())
                {
                    price = lastPrice.First().Close;
                }                

                var priceCandle = new PriceCandle
                {
                    Close = price,
                    Open = price,
                    High = price,
                    Low = price,
                    Volume = quantity,
                    StartTimeStamp = Utility.AlignToInterval(tradeDateTime, (int)interval).ToLocalTime()
                }; 
                intervalCandle.AddOrUpdate(interval, [priceCandle], (key, existingValue) =>
                {
                    lock (existingValue)
                    {
                        var existing = existingValue.GetViewBetween(priceCandle, priceCandle).FirstOrDefault();

                        if (existing != null)
                        {
                            // Update existing values (update fields as per your logic)
                            existing.UpdateCandle(priceCandle);
                        }
                        else
                        {
                            // No matching timestamp — add new one
                            existingValue.Add(priceCandle);
                        }
                        return existingValue;
                    }
                });
            }

            if (OnCandles is not null)
                await OnCandles(new StrategyEvent
                {
                    Exchange = exchange,
                    Token = token,
                    TradingSymbol = tradingSymbol
                });
        }

        public async Task UpdateCandles(IEnumerable<SelectedSymbol> selection, IEnumerable<ChartInterval> chartIntervals, int lastXDaysCandle = 5)
        {
            if(chartIntervals.Contains(ChartInterval.Daily))
                _logger.LogWarning("Daily interval candles cannot be fetched using TimePriceData API. Continuing for rest of the intervals...");

            List<Task> tasks = [];
            var initStartDate = DateTime.Now.Date.GetBusinessDaysAgo(lastXDaysCandle);
            var endDate = DateTime.Now.ToLocalTime();
            tasks.Add(Task.Run(async () =>
            {
                foreach (var item in selection)
                {
                    var (resp, msg) = await _api.MarketInfo.GetTimePriceDataAsync(item.Exchange, item.TradingSymbol, initStartDate, endDate, ChartInterval.One);
                    msg = $"{item.Exchange} {item.TradingSymbol} OHLCV data ({(int)ChartInterval.One} min) for start date {initStartDate} and end date {endDate}. {msg}";
                    if (resp is null || !resp.Any())
                    {
                        _logger.LogError("NOK: {msg}", msg);
                        continue;
                    }

                    var intervalCandles = GenerateIntervalCandles(chartIntervals, resp);

                    var details = GlobalDataSet.Data.GetOrAdd(item.TradingSymbol, _ => new Details());
                    var exchangeDict = details.PriceCandleInfo.GetOrAdd(item.Exchange, _ => new());

                    foreach (var (interval, candles) in intervalCandles)
                    {
                        var sortedSet = exchangeDict.GetOrAdd(interval, _ => []);

                        lock (sortedSet)
                        {
                            foreach (var candle in candles)
                            {
                                // Check if candle with same timestamp exists
                                var existingCandle = sortedSet.FirstOrDefault(c => c.StartTimeStamp == candle.StartTimeStamp);
                                if (existingCandle is null)
                                {
                                    sortedSet.Add(candle);
                                    continue;
                                }
                                existingCandle.UpdateCandle(candle);
                            }
                        }
                    }

                    if(OnCandles is not null)
                        await OnCandles(new StrategyEvent
                        {
                            Exchange = item.Exchange,
                            Token = item.Token,
                            TradingSymbol = item.TradingSymbol
                        });
                }
            }));
            await Task.WhenAll(tasks);           
        }

        public async Task UpdateSecurityInfo(IEnumerable<SelectedSymbol> selection)
        {
            var securityResponse = await GetSecurityInfoFromServerAsync(selection);

            if (securityResponse.Count() != selection.Count())
            {
                _logger.LogInformation("Unable to get the details of all securitites. Missing securities details are as follows:.");
            }

            foreach (var scrip in securityResponse)
            {
                var details = GlobalDataSet.Data.GetOrAdd(scrip.TradingSymbol, _ => new());
                details!.SecurityInfo.AddOrUpdate(scrip.Exchange, scrip, (_, _) => scrip);

                if(OnSecurities is not null)
                        await OnSecurities(new StrategyEvent
                        {
                            Exchange = scrip.Exchange,
                            Token = scrip.Token,
                            TradingSymbol = scrip.TradingSymbol
                        });
            }
        }

        private async Task<IEnumerable<PositionBookResponse>> GetPositionsFromServerAsync()
        {

            var (positionBook, mesg) = await _api.Trade.GetPositionBookAsync();
            if (positionBook is null || !positionBook.Any())
            {
                if (mesg != Constants.StatusOk)
                    _logger.LogError("Error while fetching positions : {mesg}", mesg);
                else
                    _logger.LogInformation("No positions found: {mesg}", mesg);
            }
            else
            {
                _logger.LogInformation("Fetched {positionBookCount} position from position book.", positionBook.Count());
            }
            return positionBook ?? [];
        }

        private async Task<IEnumerable<HoldingsResponse>> GetHoldingDetailsFromServerAsync()
        {

            var (holdings, mesg) = await _api.Holdings.GetHoldingsAsync();
            if (holdings is null || !holdings.Any())
            {
                if (mesg != Constants.StatusOk)
                    _logger.LogError("Error while fetching holdings details : {mesg}", mesg);
                else
                    _logger.LogInformation("No holdings found: {mesg}", mesg);
            }
            else
            {
                _logger.LogInformation("Fetched {holdingCount} holdings.", holdings.Count());
            }
            return holdings ?? [];
        }

        private async Task<IEnumerable<ScripInfo>> GetSecurityInfoFromServerAsync(IEnumerable<SelectedSymbol> selection)
        {
            var selectionProjection = selection.Select(a => new KeyValuePair<Exchange, long>(a.Exchange, a.Token));
            var quoteResponse = await GetQuoteDetailsFromServerAsync(_api, _logger, selectionProjection);
            var linkedScrips = await GetLinkedScripDetailsFromServerAsync(_api, _logger, selectionProjection);
            return ScripInfo.ConvertFrom(quoteResponse ?? [], linkedScrips ?? []);
        }

        private async Task<IEnumerable<TradeBookResponse>> GetTradeDetailsFromServerAsync()
        {

            var (tradeBook, mesg) = await _api.Trade.GetTradeBookAsync();
            if (tradeBook is null || !tradeBook.Any())
            {
                if (mesg != Constants.StatusOk)
                    _logger.LogError("Error while fetching trade book : {mesg}", mesg);
                else
                    _logger.LogInformation("No trades found: {mesg}", mesg);
            }
            else
            {
                _logger.LogInformation("Fetched {tradeBookCount} trades from the trade book.", tradeBook.Count());
            }
            return tradeBook ?? [];
        }

        private static async Task<IEnumerable<QuotesResponse>> GetQuoteDetailsFromServerAsync(Api api, ILogger logger, IEnumerable<KeyValuePair<Exchange, long>> exchangeToken)
        {
            List<Task<(QuotesResponse?, string)>> quoteTasks = [];
            List<QuotesResponse> quotes = [];

            foreach (var exchToken in exchangeToken)
                quoteTasks.Add(api.Scrips.GetQuotesAsync(exchToken.Key, exchToken.Value));

            var quotesResults = await Task.WhenAll(quoteTasks);

            foreach (var (quote, mesg) in quotesResults)
            {
                if (quote is null)
                {
                    if (mesg != Constants.StatusOk)
                        logger.LogError("Error while fetching quote : {mesg}", mesg);
                    else
                        logger.LogInformation("No quotes found : {mesg}", mesg);
                }
                else
                {
                    quotes.Add(quote);
                }
            }
            return quotes;
        }
        
        private static async Task<IEnumerable<LinkedScrip>> GetLinkedScripDetailsFromServerAsync(Api api, ILogger logger, IEnumerable<KeyValuePair<Exchange, long>> exchangeToken)
        {
            List<Task<(LinkedScripsResponse?, string)>> linkedScripTasks = [];

            foreach (var exchToken in exchangeToken)
                linkedScripTasks.Add(api.Scrips.GetLinkedScripsAsync(exchToken.Key, exchToken.Value));

            var linkedScripResults = await Task.WhenAll(linkedScripTasks);

            ConcurrentDictionary<KeyValuePair<Exchange, long>, LinkedScrip> linkedScrips = [];
            foreach (var (linkedScrip, mesg) in linkedScripResults)
            {
                if (linkedScrip is null)
                {
                    if (mesg != Constants.StatusOk)
                        logger.LogError("Error while fetching linked scrips : {mesg}", mesg);
                    else
                        logger.LogInformation("No linked scrips found : {mesg}", mesg);
                }
                else
                {
                    foreach (var equity in linkedScrip.LinkedEquities)
                    {
                        var key = new KeyValuePair<Exchange, long>(equity.Exchange, equity.Token);
                        var value = new LinkedScrip
                        {
                            TradingSymbol = equity.TradingSymbol,
                            Exchange = equity.Exchange,
                            Token = equity.Token,
                            IsFutureAllowed = linkedScrip.LinkedFutures.Count !=0,
                            IsOptionAllowed = linkedScrip.LinkedOptions.Count != 0,
                        };
                        linkedScrips.AddOrUpdate(key, value, (_, _) => value);
                    }
                }
            }
            return linkedScrips.Values;
        }

    }
}
