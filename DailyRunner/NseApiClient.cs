using FlatTrade.Common.Types.Base;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace DailyRunner
{
    public class NseApiClient : IAsyncDisposable, IDisposable
    {
        private readonly HttpClient _client;
        private readonly HttpClientHandler _handler;
        private bool _initialized = false;
        private readonly JsonSerializerOptions _jsonOptions;

        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
        public int MaxRetries { get; set; } = 3;
        public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(2);

        public NseApiClient()
        {
            _handler = new HttpClientHandler()
            {
                UseCookies = true,
                CookieContainer = new CookieContainer(),
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            _client = new HttpClient(_handler)
            {
                Timeout = Timeout
            };

            // Headers to mimic Chrome
            _client.DefaultRequestHeaders.UserAgent.ParseAdd(
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36");
            _client.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
            _client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip, deflate, br, zstd");
            _client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en-IN,en-GB;q=0.9,en-US;q=0.8,en;q=0.7");
            _client.DefaultRequestHeaders.Referrer = new Uri("https://www.nseindia.com/");
            _client.DefaultRequestHeaders.Connection.ParseAdd("keep-alive");

            _client.DefaultRequestHeaders.Add("sec-ch-ua", "\"Not_A Brand\";v=\"8\", \"Chromium\";v=\"120\", \"Google Chrome\";v=\"120\"");
            _client.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
            _client.DefaultRequestHeaders.Add("sec-ch-ua-platform", "\"Windows\"");
            _client.DefaultRequestHeaders.Add("sec-fetch-dest", "empty");
            _client.DefaultRequestHeaders.Add("sec-fetch-mode", "cors");
            _client.DefaultRequestHeaders.Add("sec-fetch-site", "same-origin");

            // JSON serializer config
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            };
        }

        public CookieCollection GetCookies(string url)
        {
            return _handler.CookieContainer.GetCookies(new Uri(url));
        }

        /// <summary>
        /// Ensure NSE cookies/session are initialized
        /// </summary>
        private async Task EnsureInitializedAsync()
        {
            if (_initialized) return;

            Console.WriteLine("[NSE] Initializing session...");
            var resp = await _client.GetAsync("https://www.nseindia.com/companies-listing/corporate-filings-actions");
            resp.EnsureSuccessStatusCode();
            _initialized = true;
        }

        /// <summary>
        /// Fetch raw JSON string from an NSE API URL with retries
        /// </summary>
        public async Task<string> GetApiDataAsync(string apiUrl, CancellationToken cancellationToken = default)
        {
            await EnsureInitializedAsync();

            var cookies = GetCookies("https://www.nseindia.com/companies-listing/corporate-filings-actions");
            Console.WriteLine("Cookies being sent:");
            foreach (Cookie cookie in cookies)
                Console.WriteLine($"{cookie.Name}={cookie.Value}");

            for (int attempt = 1; attempt <= MaxRetries; attempt++)
            {
                try
                {
                    using var resp = await _client.GetAsync(apiUrl, cancellationToken);
                    resp.EnsureSuccessStatusCode();
                    await foreach(var res in resp.Content.ReadFromJsonAsAsyncEnumerable<CorporateActionsResponse>(cancellationToken))
                    {
                        Console.WriteLine($"[NSE] Fetched {res!.Isin} records from {apiUrl}");
                    }

                    return await resp.Content.ReadAsStringAsync(cancellationToken);
                }
                catch (Exception ex) when (attempt < MaxRetries)
                {
                    Console.WriteLine($"[NSE] Attempt {attempt} failed: {ex.Message}. Retrying in {RetryDelay.TotalSeconds}s...");
                    await Task.Delay(RetryDelay, cancellationToken);
                }
            }

            throw new HttpRequestException($"[NSE] Failed after {MaxRetries} retries for URL: {apiUrl}");
        }

        /// <summary>
        /// Fetch JSON and deserialize into strongly-typed object
        /// </summary>
        public async Task<(T?, string)> GetApiDataAsync<T>(string apiUrl, CancellationToken cancellationToken = default)
        {
            string msg = Constants.StatusOk;
            try
            {
                string json = await GetApiDataAsync(apiUrl, cancellationToken);
                return (JsonSerializer.Deserialize<T>(json, _jsonOptions), msg);

            }
            catch (JsonException jex)
            {
                msg = "Invalid JSON format from NSE API " + jex;
            }
            catch (Exception ex)
            {
                msg = "Error fetching data from NSE API " + ex;
            }
            return (default, msg);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _client.Dispose();
            _handler.Dispose();
        }

        public ValueTask DisposeAsync()
        {
            GC.SuppressFinalize(this);
            Dispose();
            return ValueTask.CompletedTask;
        }
    }
}