using DailyRunner.Helpers;
using FlatTrade;
using FlatTrade.Common.Helpers;
using FlatTrade.Common.Types.Base;
using FlatTrade.ScripManager;
using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Diagnostics;

namespace DailyRunner
{
    internal class StocksGenerator
    {
        private readonly ILogger<StocksGenerator> _logger;
        private readonly Api _api;
        private readonly bool _enabled;

        private readonly CsvWriter? _csvWriter;
        private readonly DbWriter? _dbWriter;

        private readonly List<Exchange> _exchanges = [];
        private readonly List<InstrumentName> _instrumentName = [];

        private readonly string _storedProcedureName = "[dbo].[sp_UpsertStockInstruments]";
        private readonly string _tvpTypeName = "[dbo].[TStockInstruments]";
        private readonly ConcurrentBag<Task?> _taskList = [];

        internal class LinkScripSymbol
        {
            public string TradingSymbol { get; set; } = string.Empty;
            public Exchange Exchange { get; set; }
            public long Token { get; set; }
            public bool IsFutureAllowed { get; set; }
            public bool IsOptionAllowed { get; set; }
        }

        public StocksGenerator(Api api, IConfiguration config, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<StocksGenerator>();
            _api = api;
            _enabled = Convert.ToBoolean(config["StockInstrumentGenerator:Enabled"] ?? "false");
            if (!_enabled)
                return;

            var writeToDbEnabled = Convert.ToBoolean(config["StockInstrumentGenerator:WriteToDb:Enabled"] ?? "false");
            if (writeToDbEnabled)
            {
                var connectionString = config["Database:ConnectionString"] ?? string.Empty;
                var dbChannelCapacity = Convert.ToInt32(config["StockInstrumentGenerator:WriteToDb:ChannelCapacity"] ?? "5000");
                var writeBatchSize = Convert.ToInt32(config["StockInstrumentGenerator:WriteToDb:WriteBatchSizeInDb"] ?? "5000");
                _dbWriter = new DbWriter(connectionString, writeBatchSize, dbChannelCapacity, nameof(StocksGenerator), loggerFactory);
            }

            var writeToFileEnabled = Convert.ToBoolean(config["StockInstrumentGenerator:WriteToFile:Enabled"] ?? "false");
            if (writeToFileEnabled)
            {
                var fileChannelCapacity = Convert.ToInt32(config["StockInstrumentGenerator:WriteToFile:ChannelCapacity"] ?? "5000");
                var filePath = config["StockInstrumentGenerator:WriteToFile:CsvFilePath"] ?? string.Empty;
                if (string.IsNullOrEmpty(filePath))
                    throw new ArgumentNullException("StockInstrumentGenerator:WriteToFile:CsvFilePath is not configured.");

                filePath = Path.GetFullPath(filePath);
                _csvWriter = new CsvWriter(Path.GetDirectoryName(filePath)!, fileChannelCapacity, nameof(StocksGenerator), loggerFactory);
            }

            _instrumentName = config.GetSection("StockInstrumentGenerator:InterestedInstrumentNamesInExchanges").Get<List<InstrumentName>>() ?? [];
            if (_instrumentName is null || _instrumentName.Count == 0)
            {
                _instrumentName = [.. Enum.GetValues(typeof(InstrumentName)).Cast<InstrumentName>()];
            }

            _exchanges = config.GetSection("StockInstrumentGenerator:InterestedExchanges").Get<List<Exchange>>() ?? [];
            if (_exchanges is null || _exchanges.Count == 0)
            {
                _exchanges = [.. Enum.GetValues(typeof(Exchange)).Cast<Exchange>()];
            }
        }
        public async Task<IEnumerable<(Exchange, long, string)>?> GenerateAndLoad()
        {
            if (!_enabled)
            {
                _logger.LogInformation("StocksGenerator is disabled in config.");
                return default;
            }

            var stopWatch = new Stopwatch();
            try
            {
                stopWatch.Start();

                var (quoteList, infoList, intradayList) = await GenerateStockInstruments();
                if (quoteList is null || !quoteList.Any() || infoList is null || !infoList.Any())
                {
                    _logger.LogCritical("NOK: No symbol extracted from api.");
                    return default;
                }

                var headerAndContents = GenerateHeaderAndContents(quoteList, infoList, intradayList);               
                if (_csvWriter is not null)
                {
                    var csvChannelObject = new CsvChannelObject
                    {
                        FileName = $"StockInstruments_{DateTime.Now:yyyyMMdd}.csv",
                        Header = headerAndContents.Item1,
                        Records = headerAndContents.Item2.Select(t => { return (t.Item4); })
                    };
                    await _csvWriter.WriteCsvAsync(csvChannelObject);
                }
                    
                if (_dbWriter is not null)
                {
                    var dbChannelObject = new DbChannelObject
                    {
                        TvpName = _tvpTypeName,
                        StoredProcedureName = _storedProcedureName,
                        Records = ToDataTable(headerAndContents.Item3)
                    };
                    await _dbWriter.WriteDbAsync(dbChannelObject);
                }
                    
                return headerAndContents.Item2.Select(t => { return (t.Item2, t.Item3, t.Item1); });
            }

            catch (AggregateException ae)
            {
                _logger.LogError("\n--- One or more tasks failed: ---");
                foreach (var ex in ae.Flatten().InnerExceptions)
                {
                    _logger.LogError(ex, "  Error: {ex.GetType().Name} - {ex.Message}", ex.GetType().Name, ex.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "\n--- An unexpected error occurred: ---");
                _logger.LogCritical("  Error: {ex.GetType().Name} - {ex.Message}", ex.GetType().Name, ex.Message);
            }
            finally
            {
                await Utility.WhenAllSafe([.._taskList]);

                _taskList.Clear();
                _taskList.Add(_csvWriter?.WriteComplete()); // Signal completion
                _taskList.Add(_dbWriter?.WriteComplete()); // Signal completion

                await Utility.WhenAllSafe([.._taskList]);
                _logger.LogInformation("===========  Stock instruments uploaded. Took =========== : {msg}", stopWatch.StopAndLog());
            }
            return default;
        }

        private async Task<(IEnumerable<QuotesResponse>, 
                            IEnumerable<SecurityInfoResponse>,
                            IEnumerable<LinkScripSymbol>)> GenerateStockInstruments()
        {
            ConcurrentBag<ScripDetails> scripList = [];

            List<Task?> tasks = [];
            foreach (var exchange in _exchanges)
            {
                tasks.Add(Task.Run(async () =>
                {
                    var (scripResp, msg) = await _api.Scrips.GetAllScripAsync(exchange);
                    
                    if (scripResp is null || !scripResp.Any())
                    {
                        msg = $"{exchange} stocks information from scrips(GetAllScripAsync). {msg}";
                        _logger.LogError("NOK: {msg}", msg);
                        //continue;
                        return;
                    }

                    foreach (var scrip in scripResp)
                        scripList.Add(scrip);
                }));
            }
            await Utility.WhenAllSafe([..tasks]);
            tasks.Clear();

            scripList = [.. scripList.DistinctBy(scrip => new { scrip.Token, scrip.Exchange })];
            _logger.LogInformation("Total Scrips fetched from API: {scripList.Count}. Step 1/4...", scripList.Count);
            ConcurrentBag<SecurityInfoResponse> securityInfoList = [];

            int cnt = 0;
            foreach (var scripBatch in scripList.Chunk(500))
            {
                tasks.Add(Task.Run(async()=>
                {
                    foreach (var scrip in scripBatch)
                    {
                        if (Interlocked.Increment(ref cnt) % 1000 == 0)
                            _logger.LogDebug("GetSecurityInfoAsync api called {cnt}/{total}", cnt, scripList.Count);
                        var (securityResp, msg) = await _api.Scrips.GetSecurityInfoAsync(scrip.Exchange, scrip.Token);

                        if (securityResp is null || securityResp.Status != Constants.StatusOk)
                        {
                            msg = $"{scrip.Exchange}/{scrip.TradingSymbol} stock information from scrips(GetSecurityInfoAsync). {msg}";
                            _logger.LogError("NOK: {msg}", msg);
                            continue;
                        }
                        securityInfoList.Add(securityResp);
                        var currentCount = securityInfoList.Count;
                        if (currentCount % 1000 == 0)
                            _logger.LogInformation("GetSecurityInfoAsync api response {securityInfoList}/{total} ", securityInfoList.Count, scripList.Count);
                    }
                }));
            }

            await Utility.WhenAllSafe([.. tasks]);
            tasks.Clear();

            _logger.LogInformation("Total Security Info fetched from API: {securityInfoList.Count}. Step 2/4...", securityInfoList.Count);
            securityInfoList = [.. securityInfoList.Where(sec => _instrumentName.Contains(sec.InstrumentName))];
            _logger.LogInformation("Filtered Security Info eligible to fetch quotes for: {securityInfoList.Count}. Step 2/4...", securityInfoList.Count);
            ConcurrentBag<QuotesResponse> quoteList = [];

            cnt = 0;
            foreach (var scrip in securityInfoList)
            {
                if (Interlocked.Increment(ref cnt) % 500 == 0)
                {
                    _logger.LogDebug("GetQuotesAsync api called {cnt}/{total}", cnt, securityInfoList.Count);
                    //Thread.Sleep(1000);
                }
                var (quotesResp, msg) = await _api.Scrips.GetQuotesAsync(scrip.Exchange, scrip.Token);

                if (quotesResp is null || quotesResp.Status != Constants.StatusOk)
                {
                    msg = $"{scrip.Exchange} stocks information from scrips(GetQuotesAsync). {msg}";
                    _logger.LogError("NOK: {msg}", msg);
                    continue;
                }
                quoteList.Add(quotesResp);
                if (quoteList.Count % 500 == 0)
                {
                    _logger.LogInformation("GetQuotesAsync api response {quoteList}/{total}", quoteList.Count, securityInfoList.Count);
                    //Thread.Sleep(1000);
                }
            }
            //await Utility.WhenAllSafe([..tasks]);
            //tasks.Clear();

            _logger.LogInformation("Total Quotes Info fetched from API: {quoteList.Count}. Step 3/4...", quoteList.Count);

            ConcurrentBag<LinkScripSymbol> linkedScrips = [];
            cnt = 0;
            foreach (var securityInfoBatch in securityInfoList.Chunk(1000))
            {
                tasks.Add(Task.Run(async () =>
                {
                    foreach (var scrip in securityInfoBatch)
                    {
                        if (Interlocked.Increment(ref cnt) % 500 == 0)
                            _logger.LogDebug("GetLinkedScripsAsync api called {cnt}/{total}", cnt, securityInfoList.Count);

                        var (linkedScripResp, msg) = await _api.Scrips.GetLinkedScripsAsync(scrip.Exchange, scrip.Token);

                        if (linkedScripResp is null || linkedScripResp.Status != Constants.StatusOk)
                        {
                            msg = $"{scrip.TradingSymbol} stocks information from scrips(GetLinkedScripsAsync). {msg}";
                            _logger.LogError("NOK: {msg}", msg);
                            continue;
                        }
                        linkedScrips.Add(new LinkScripSymbol
                        {
                             Token = scrip.Token,
                             Exchange = scrip.Exchange,
                             TradingSymbol = scrip.TradingSymbol,
                             IsFutureAllowed = linkedScripResp.LinkedFutures is not null && linkedScripResp.LinkedFutures.Count !=0,
                             IsOptionAllowed = linkedScripResp.LinkedOptions is not null && linkedScripResp.LinkedOptions.Count !=0
                        });
                        if (linkedScrips.Count % 500 == 0)
                        {
                            _logger.LogInformation("GetLinkedScripsAsync api response {securityInfoList}/{total}", linkedScrips.Count, securityInfoList.Count);
                            //Thread.Sleep(1000);
                        }
                    }
                }));
            }
            await Utility.WhenAllSafe([.. tasks]);
            tasks.Clear();

            _logger.LogInformation("Total linked scrips fetched from API: {securityInfoList.Count}. Step 4/4...", securityInfoList.Count);
            if (scripList.IsEmpty || quoteList.IsEmpty || securityInfoList.IsEmpty)
                return default;

            return (quoteList, securityInfoList, linkedScrips);
        }

        private static (string, IEnumerable<(string, Exchange, long, string)>, IEnumerable<object>) GenerateHeaderAndContents(IEnumerable<QuotesResponse> quoteList, 
                                                                                                                              IEnumerable<SecurityInfoResponse> securityInfoList,
                                                                                                                              IEnumerable<LinkScripSymbol> linkedScripSymbolsList)
        {
            var joinedDataHeader = $"Token,SymbolName,TradingSymbol,CompanyName," +
                                   $"Exchange,Segment,InstrumentName," +
                                   $"Isin,TickSize,PricePrecision," +
                                   $"LotSize,UpperCircuit,LowerCircuit," +
                                   $"LastTradeDateTime,LastUpdateTime,LastTradePrice,AverageTradePrice," +
                                   $"Wk52High,Wk52Low,IssueCapital,IssueDate,ListingDate,FreezeQuantity,IsFutureAllowed,IsOptionAllowed";
            var joinedData = from a in quoteList
                             join b in securityInfoList on new { a.Token, a.Exchange } // 👈 multiple fields
                                                                equals new { b.Token, b.Exchange }
                             join c in linkedScripSymbolsList on new { b.Token, b.Exchange } // 👈 multiple fields
                                                                equals new { c.Token, c.Exchange }
                             select
                             (
                                a.TradingSymbol,
                                a.Exchange,
                                a.Token,
                                CsvStr: string.Format($"{a.Token},{a.SymbolName},{a.TradingSymbol},{b.CompanyName}," +
                                                      $"{a.Exchange},{a.Segment},{a.InstrumentName}," +
                                                      $"{a.Isin},{a.TickSize},{a.PricePrecision}," +
                                                      $"{a.LotSize},{a.UpperCircuitLimit},{a.LowerCircuitLimit}," +
                                                      $"{a.LastTradeDateTime},{a.LastUpdateTime},{a.LastTradePrice},{a.AverageTradePrice}," +
                                                      $"{a.Wk52High},{a.Wk52Low},{a.IssueCapital},{b.IssueDate},{b.ListingDate}," +
                                                      $"{b.FreezeQuantity},{c.IsFutureAllowed},{c.IsOptionAllowed}")
                             );

            var joinedObject = from a in quoteList
                               join b in securityInfoList on a.Token equals b.Token
                               join c in linkedScripSymbolsList on a.Token equals c.Token
                               select
                               (new
                               {
                                   a.Token,
                                   a.SymbolName,
                                   a.TradingSymbol,
                                   b.CompanyName,
                                   a.Exchange,
                                   a.Segment,
                                   a.InstrumentName,
                                   a.Isin,
                                   a.TickSize,
                                   a.PricePrecision,
                                   a.LotSize,
                                   UpperCircuit = a.UpperCircuitLimit,
                                   LowerCircuit = a.LowerCircuitLimit,
                                   a.LastTradeDateTime,
                                   a.LastUpdateTime,
                                   a.LastTradePrice,
                                   a.AverageTradePrice,
                                   a.Wk52High,
                                   a.Wk52Low,
                                   a.IssueCapital,
                                   IssueDate = b.IssueDate.ToDateTime(TimeOnly.MinValue),
                                   ListingDate = b.ListingDate.ToDateTime(TimeOnly.MinValue),
                                   b.FreezeQuantity,
                                   c.IsFutureAllowed,
                                   c.IsOptionAllowed
                               }
                               );

            return (joinedDataHeader, joinedData, joinedObject);
        }

        private static DataTable ToDataTable(IEnumerable<object> objects)
        {
            var table = new DataTable();
            table.Columns.Add("Token", typeof(int));
            table.Columns.Add("SymbolName", typeof(string));
            table.Columns.Add("TradingSymbol", typeof(string));
            table.Columns.Add("CompanyName", typeof(string));
            table.Columns.Add("ExchangeCode", typeof(string));
            table.Columns.Add("Segment", typeof(string));
            table.Columns.Add("InstrumentName", typeof(string));
            table.Columns.Add("Isin", typeof(string));
            table.Columns.Add("TickSize", typeof(decimal));
            table.Columns.Add("PricePrecision", typeof(short));
            table.Columns.Add("LotSize", typeof(decimal));
            table.Columns.Add("UpperCircuit", typeof(decimal));
            table.Columns.Add("LowerCircuit", typeof(decimal));
            table.Columns.Add("LastTradeDateTime", typeof(DateTime));
            table.Columns.Add("LastUpdateTime", typeof(DateTime));
            table.Columns.Add("LastTradePrice", typeof(decimal));
            table.Columns.Add("AverageTradePrice", typeof(decimal));
            table.Columns.Add("Wk52High", typeof(decimal));
            table.Columns.Add("Wk52Low", typeof(decimal));
            table.Columns.Add("IssueCapital", typeof(decimal));
            table.Columns.Add("IssueDate", typeof(DateTime));
            table.Columns.Add("ListingDate", typeof(DateTime));
            table.Columns.Add("FreezeQuantity", typeof(int));
            table.Columns.Add("IsFutureAllowed", typeof(int));
            table.Columns.Add("IsOptionAllowed", typeof(int));

            foreach (dynamic obj in objects)
                table.Rows.Add(obj.Token, obj.SymbolName, obj.TradingSymbol, obj.CompanyName, obj.Exchange.ToString(), obj.Segment, obj.InstrumentName, obj.Isin,
                                obj.TickSize, obj.PricePrecision, obj.LotSize, obj.UpperCircuit, obj.LowerCircuit,
                                obj.LastTradeDateTime, obj.LastUpdateTime, obj.LastTradePrice,
                                obj.AverageTradePrice, obj.Wk52High, obj.Wk52Low, obj.IssueCapital, obj.IssueDate, obj.ListingDate,
                                obj.FreezeQuantity, obj.IsFutureAllowed, obj.IsOptionAllowed);

            return table;
        }
    }
}
