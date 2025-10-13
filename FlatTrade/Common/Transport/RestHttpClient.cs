using FlatTrade.Common.Helpers;
using FlatTrade.Common.Types.Base;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace FlatTrade.Common.Transport
{
    public class RestHttpClient : IAsyncDisposable, IDisposable
    {
        private readonly HttpClient _httpClient;

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _httpClient.Dispose();
        }

        public ValueTask DisposeAsync()
        {
            GC.SuppressFinalize(this);
            Dispose();
            return ValueTask.CompletedTask;
        }

        public RestHttpClient(HttpClient httpClient, string baseAddress)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient)); // Ensure HttpClient is initialized
            if (_httpClient.BaseAddress != null || !string.IsNullOrEmpty(baseAddress)) // Set BaseAddress only once
            {
                _httpClient.BaseAddress = new Uri(baseAddress);
            }
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public HttpClient GetNativeHttpClient()
        {
            return _httpClient;
        }

        public void SetHeaders(string header, string value)
        {
            _httpClient.DefaultRequestHeaders.Add(header, value);
        }

        public async Task<(TReceive?, string)> GetAsync<TReceive>(Uri uri, bool isSchemaModelCheckByPassed = false)
        {
            string eMsg;
            TReceive? responseObject = default;
            try
            {
                var response = await _httpClient.GetAsync(uri);
                string responseBody = await response.Content.ReadAsStringAsync();
                
                RestHttpClientExtension.CheckJsonAgainstModel<TReceive>(responseBody, isSchemaModelCheckByPassed);
                responseObject = JsonConvert.DeserializeObject<TReceive>(responseBody)!;

                response.EnsureSuccessStatusCode(); // Throws HttpRequestException for 4xx/5xx responses            
                return (responseObject, Constants.StatusOk);
            }
            catch (HttpRequestException e)
            {
                eMsg = $"[Http Client Error]: POST Request Exception for {uri}: {e.StatusCode} - {e.Message}";
            }
            catch (JsonSerializationException e)
            {
                eMsg = $"[API Client Error]: POST JSON Serialization Exception for {uri}: {e.Message}";
            }
            catch (ArgumentOutOfRangeException e)
            {
                eMsg = $"[API Client Error]: POST JSON Serialization Exception for {uri}: {e.Message}";
            }
            catch (Exception e)
            {
                eMsg = $"[API Client Error]: Unexpected error during Get for {uri}: {e.Message}";
            }
            return (responseObject, eMsg);
        }

        public async Task<(string, string)> GetAsync(Uri uri)
        {
            string eMsg;
            string responseBody = string.Empty;
            try
            {
                var response = await _httpClient.GetAsync(uri);
                responseBody = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode(); // Throws HttpRequestException for 4xx/5xx responses    
                return (responseBody ?? string.Empty, Constants.StatusOk);
            }
            catch (HttpRequestException e)
            {
                eMsg = $"[Http Client Error]: Get Request Exception for {uri}: {e.StatusCode} - {e.Message}";
            }
            catch (Exception e)
            {
                eMsg = $"[API Client Error]: Unexpected error during Get for {uri}: {e.Message}";
            }
            return (responseBody ?? string.Empty, eMsg);
        }

        public async Task<(TReceive?, string)> PostAsync<TReceive>(Uri uri, HttpContent content)
        {
            string eMsg;
            TReceive? responseObject = default;
            try
            {
                var response = await _httpClient.PostAsync(uri, content);
                string responseBody = await response.Content.ReadAsStringAsync();
                responseObject = JsonConvert.DeserializeObject<TReceive>(responseBody)!;

                response.EnsureSuccessStatusCode(); // Throws HttpRequestException for 4xx/5xx responses            
                return (responseObject, Constants.StatusOk);
            }
            catch (HttpRequestException e)
            {
                eMsg = $"[Http Client Error]: POST Request Exception for {uri}: {e.StatusCode} - {e.Message}";
            }
            catch (JsonSerializationException e)
            {
                eMsg = $"[API Client Error]: POST JSON Serialization Exception for {uri}: {e.Message}";
            }
            catch (ArgumentOutOfRangeException e)
            {
                eMsg = $"[API Client Error]: POST JSON Serialization Exception for {uri}: {e.Message}";
            }
            catch (Exception e)
            {
                eMsg = $"[API Client Error]: Unexpected error during POST for {uri}: {e.Message}";
            }
            return (responseObject, eMsg);
        }

        public async Task<(string, string)> PostAsync(Uri uri, HttpContent content)
        {
            string eMsg;
            string responseBody = string.Empty;
            try
            {
                var response = await _httpClient.PostAsync(uri, content);
                responseBody = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode(); // Throws HttpRequestException for 4xx/5xx responses    
                return (responseBody ?? string.Empty, Constants.StatusOk);
            }
            catch (HttpRequestException e)
            {
                eMsg = $"[Http Client Error]: POST Request Exception for {uri}: {e.StatusCode} - {e.Message}";
            }
            catch (Exception e)
            {
                eMsg = $"[API Client Error]: Unexpected error during POST for {uri}: {e.Message}";
            }
            return (responseBody ?? string.Empty, eMsg);
        }

    }
}
