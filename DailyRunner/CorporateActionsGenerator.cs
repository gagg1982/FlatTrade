using DailyRunner.Helpers;
using FlatTrade;
using FlatTrade.Common.Helpers;
using FlatTrade.Common.Transport;
using FlatTrade.Common.Types;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Concurrent;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using Utility = DailyRunner.Helpers.Utility;

namespace DailyRunner
{
    internal class CorporateActionsGenerator
    {
        private readonly Api _api;
        private readonly ILogger<CorporateActionsGenerator> _logger;
        private readonly NseApiClient _nseApiClient;
        private readonly bool _enabled;

        private readonly List<string> _excludedCAs = [];
        private readonly int _advanceDays = 90;

        private readonly DateOnly _startDate = DateOnly.ParseExact("01-01-2025", "dd-MM-yyyy", CultureInfo.InvariantCulture);

        private readonly string _storedProcedureName = "[dbo].[sp_UpsertCorporateActions]";
        private readonly string _tvpTypeName = "[dbo].[TCorporateActions]";

        private readonly string _nseCaUrl = string.Empty;
        private readonly string _bseCaUrl = string.Empty;

        private readonly ConcurrentBag<Task?> _taskList = [];
        private readonly CsvWriter? _csvWriter;
        private readonly DbWriter? _dbWriter;

        public CorporateActionsGenerator(Api api, IConfiguration config, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<CorporateActionsGenerator>();
            _api = api;
            _nseApiClient = new();
            _enabled = Convert.ToBoolean(config["CorporateActionsGenerator:Enabled"] ?? "false");
            if (!_enabled)
                return;

            _excludedCAs = config.GetSection("CorporateActionsGenerator:ExcludedCA").Get<List<string>>() ?? [];
            _advanceDays = Convert.ToInt32(config["CorporateActionsGenerator:AdvanceDays"] ?? string.Empty);
            _startDate = DateOnly.ParseExact(config["CorporateActionsGenerator:Startdate"] ?? "01-01-2025", "dd-MM-yyyy", CultureInfo.InvariantCulture);

            var writeToDbEnabled = Convert.ToBoolean(config["CorporateActionsGenerator:WriteToDb:Enabled"] ?? "false");
            if (writeToDbEnabled)
            {
                var connectionString = config["Database:ConnectionString"] ?? string.Empty;
                var dbChannelCapacity = Convert.ToInt32(config["CorporateActionsGenerator:WriteToDb:ChannelCapacity"] ?? "5");
                var writeBatchSize = Convert.ToInt32(config["CorporateActionsGenerator:WriteToDb:WriteBatchSizeInDb"] ?? "5000");
                _dbWriter = new DbWriter(connectionString, writeBatchSize, dbChannelCapacity, nameof(CorporateActionsGenerator), loggerFactory);
            }

            var writeToFileEnabled = Convert.ToBoolean(config["CorporateActionsGenerator:WriteToFile:Enabled"] ?? "false");
            if (writeToFileEnabled)
            {
                var fileChannelCapacity = Convert.ToInt32(config["CorporateActionsGenerator:WriteToFile:ChannelCapacity"] ?? "5");
                var filePath = config["CorporateActionsGenerator:WriteToFile:CsvFilePath"] ?? string.Empty;
                if (string.IsNullOrEmpty(filePath))
                    throw new InvalidDataException("CorporateActionsGenerator:WriteToFile:CsvFilePath is not configured.");

                filePath = Path.GetFullPath(filePath);
                _csvWriter = new CsvWriter(Path.GetDirectoryName(filePath!)!, fileChannelCapacity, nameof(CorporateActionsGenerator), loggerFactory);
            }

            var nseCaUrl = config["CorporateActionsGenerator:NseCaUrl"] ?? string.Empty;
            _nseCaUrl = nseCaUrl.Replace("[to_date]", DateOnly.FromDateTime(DateTime.Now.ToLocalTime().AddDays(_advanceDays)).ToString())
                                .Replace("[from_date]", _startDate.ToString());

            var bseCaUrl = config["CorporateActionsGenerator:BseCaUrl"] ?? string.Empty;
            _bseCaUrl = bseCaUrl.Replace("[to_date]", DateOnly.FromDateTime(DateTime.Now.ToLocalTime().AddDays(_advanceDays)).ToString())
                                .Replace("[from_date]", _startDate.ToString());

        }
        public async Task GenerateAndLoad()
        {
            if (!_enabled)
            {
                _logger.LogInformation("CorporateActionsGenerator is disabled in config.");
                return;
            }

            var stopWatch = new Stopwatch();
            try
            {
                stopWatch.Start();

                var res = await _nseApiClient.GetApiDataAsync(_nseCaUrl);


                List<Task> tasks = [];
                if (_nseCaUrl.IsNullOrEmpty())
                    _logger.LogWarning("NSE Corporate Actions URL is not configured.");
                else
                    tasks.Add(GenerateCorporateActionsNse());

                if (_nseCaUrl.IsNullOrEmpty())
                    _logger.LogWarning("BSE Corporate Actions URL is not configured.");
                else
                    tasks.Add(GenerateCorporateActionsBse());

                await Utility.WhenAllSafe([.. tasks]);
            }
            catch (AggregateException ae)
            {
                _logger.LogError("\n--- One or more tasks failed: ---");
                foreach (var ex in ae.Flatten().InnerExceptions)
                {
                    _logger.LogError(ex, "  Error: {ex.TypeName} - {ex.Message}", ex.GetType().Name, ex.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "\n--- An unexpected error occurred: ---");
                _logger.LogCritical("  Error: {ex.TypeName} - {ex.Message}", ex.GetType().Name, ex.Message);
            }
            finally
            {
                _taskList.Add(_csvWriter?.WriteComplete()); // Signal completion
                _taskList.Add(_dbWriter?.WriteComplete()); // Signal completion

                await Utility.WhenAllSafe([.. _taskList]);
                _logger.LogInformation("=========== [{CorporateActionsGenerator}] Done. Took : {msg}", nameof(CorporateActionsGenerator), stopWatch.StopAndLog());
            }
        }
        private async Task GenerateCorporateActionsBse()
        {
            try
            {
                var httpClient = new RestHttpClient(new HttpClient(), _bseCaUrl);
                var (resp, msg) = await httpClient.GetAsync<IEnumerable<CorporateActionsResponse>>(new Uri(_bseCaUrl));
                _logger.LogDebug("Calling API for : {msg}", msg);
                if (resp is null || !resp.Any())
                {
                    _logger.LogError("NOK: {msg}", msg);
                    return;
                }

                if (_excludedCAs.Count != 0)
                    resp = resp.Where(ca => !_excludedCAs.Contains(ca.Purpose, StringComparer.OrdinalIgnoreCase));


                var headerAndContents = GenerateHeaderAndContents(resp);
                var newFile = $"CorporateActions_BSE_{DateTime.Now:yyyyMMdd}.csv";

                if (_csvWriter is not null)
                {

                    var csvChannelObject = new CsvChannelObject
                    {
                        FileName = newFile,
                        Header = headerAndContents.Item1,
                        Records = headerAndContents.Item2
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

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while generating the CAs: {ex.Message}", ex.Message);
            }
        }
        private async Task GenerateCorporateActionsNse()
        {
            try
            {
                var httpClient = new RestHttpClient(new HttpClient(), _nseCaUrl);
                var (resp, msg) = await httpClient.GetAsync<IEnumerable<CorporateActionsResponse>>(new Uri(_nseCaUrl));
                _logger.LogDebug("Calling API for : {msg}", msg);
                if (resp is null || !resp.Any())
                {
                    _logger.LogError("NOK: {msg}", msg);
                    return;
                }

                if (_excludedCAs.Count != 0)
                    resp = resp.Where(ca => !_excludedCAs.Contains(ca.Purpose, StringComparer.OrdinalIgnoreCase));


                var headerAndContents = GenerateHeaderAndContents(resp);
                var newFile = $"CorporateActions_NSE_{DateTime.Now:yyyyMMdd}.csv";

                if (_csvWriter is not null)
                {

                    var csvChannelObject = new CsvChannelObject
                    {
                        FileName = newFile,
                        Header = headerAndContents.Item1,
                        Records = headerAndContents.Item2
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

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while generating the CAs: {ex.Message}", ex.Message);
            }
        }

        private static (string, IEnumerable<string>, IEnumerable<CorporateActionsResponse>) GenerateHeaderAndContents(IEnumerable<CorporateActionsResponse> resp)
        {
            var header = "Isin,Symbol,CompanyName,Series,Purpose,FaceValue,ExDate,RecordDate,BookClosureStartdate,BookClosureEndDate";
            var records = resp.Select(ca => $"{ca.Isin},{ca.Symbol},{ca.CompanyName},{ca.Series},{ca.Purpose},{ca.FaceValue},{ca.ExDate},{ca.RecordDate},{ca.BookClosureStartDate},{ca.BookClosureEndDate}");
            var dbRecords = resp.Select(ca => new CorporateActionsResponse
            {
                Isin = ca.Isin,
                Symbol = ca.Symbol,
                CompanyName = ca.CompanyName,
                Series = ca.Series,
                Purpose = ca.Purpose,
                FaceValue = ca.FaceValue,
                ExDate = ca.ExDate,
                RecordDate = ca.RecordDate,
                BookClosureStartDate = ca.BookClosureStartDate,
                BookClosureEndDate = ca.BookClosureEndDate
            });
            return (header, records, dbRecords);
        }

        private static DataTable ToDataTable(IEnumerable<CorporateActionsResponse> objects)
        {
            var table = new DataTable();
            table.Columns.Add("Isin", typeof(string));
            table.Columns.Add("Symbol", typeof(string));
            table.Columns.Add("CompanyName", typeof(string));
            table.Columns.Add("Series", typeof(string));
            table.Columns.Add("Purpose", typeof(string));
            table.Columns.Add("FaceValue", typeof(decimal));
            table.Columns.Add("ExDate", typeof(DateTime));
            table.Columns.Add("RecordDate", typeof(DateTime));
            table.Columns.Add("BookClosureStartDate", typeof(DateTime));
            table.Columns.Add("BookClosureEndDate", typeof(DateTime));

            foreach (dynamic obj in objects)
                table.Rows.Add(obj.token, obj.StartDateTime, obj.OpenPrice, obj.HighPrice, obj.LowPrice, obj.ClosePrice, obj.Volume);

            return table;
        }
    }
}
