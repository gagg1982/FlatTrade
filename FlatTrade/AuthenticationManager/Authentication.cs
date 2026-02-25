using Common.Helpers;
using Common.Transport;
using Common.Types;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;
using System.Security.Cryptography;
using System.Text;

using Microsoft.Playwright;
using OtpNet;

namespace FlatTrade.AuthenticationManager
{
    public class Authentication(string apiKey, string redirectUri, string secret, string accessTokenFilePath, string uid, string password, string qrCode, RestHttpClient httpClient, ILoggerFactory loggerFactory)
    {
        private readonly ILogger<Authentication> _logger = loggerFactory.CreateLogger<Authentication>();
        private static readonly string TOKEN_FILE_NAME = "FlatTrade.AccessToken.Token";
        private readonly string _redirectUri = redirectUri;
        private readonly string _apiSecret = secret;
        private readonly string _accessTokenFilePath = string.IsNullOrEmpty(accessTokenFilePath) ?
                                                            Path.GetFullPath(TOKEN_FILE_NAME) : Path.GetFullPath(accessTokenFilePath);

        private AccessTokenInfo? _accessToken;
        private readonly string _apiKey = apiKey;
        private readonly string _uid = uid;
        private readonly string _password = password;
        private readonly string _qrCode = qrCode;

        private readonly RestHttpClient _httpClient = httpClient;

        //[Throttle]
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<(AccessTokenInfo?, string)> GetAccessTokenAsync()
        {
            if (_accessToken != null && IsTokenValid(_accessToken, _logger))
                return (_accessToken, Constants.StatusOk);

            _accessToken = await LoadAccessTokenAsync(_accessTokenFilePath, _logger);

            if (_accessToken != null && IsTokenValid(_accessToken, _logger))
            {
                _logger.LogInformation("Using stored valid access token.");
                return (_accessToken, Constants.StatusOk);
            }

            _logger.LogWarning("Stored access token is invalid or expired. Initiating new authorization flow...");
            return await PerformAuthorizationFlowAsync();
        }

        private static bool IsTokenValid(AccessTokenInfo tokenInfo, ILogger logger)
        {
            // Check if the token has actually expired
            if (tokenInfo.ExpiresAtUtc.ToLocalTime() <= DateTime.Now.ToLocalTime())
            {
                logger.LogWarning("Token is technically expired based on expires_at.");
                return false;
            }

            // Check the 5-6 AM rule
            DateTime nowLocalTime = DateTime.Now.ToLocalTime();

            // Get today's 5 AM and 6 AM (local time, assuming the rule is local time based)
            DateTime fiveAMToday = nowLocalTime.Date.AddHours(5);
            DateTime sixAMToday = nowLocalTime.Date.AddHours(6);

            // Adjust fiveAMToday and sixAMToday to local time if they were derived from localtime.
            // or ensure consistent timezone handling for the 5-6 AM rule.
            // For simplicity, assuming the rule refers to local time of the *machine running the code*.
            // A more robust solution might require specifying a timezone for the 5-6 AM rule.
            if (nowLocalTime >= fiveAMToday && nowLocalTime < sixAMToday)
            {
                logger.LogWarning("Current time is between 5 AM and 6 AM local time. Token might be cleared. Re-authorizing.");
                return false; // Force re-auth during the clear window
            }
            return true;
        }

        private async Task<(AccessTokenInfo?, string)> PerformAuthorizationFlowAsync()
        {
            if(string.IsNullOrEmpty(_qrCode) || string.IsNullOrEmpty(_uid) || string.IsNullOrEmpty(_password))
                return await PerformAuthorizationFlowInteractiveAsync();
            return await PerformAuthorizationFlowNonInteractiveAsync();            
        }

        private async Task<(AccessTokenInfo?, string)> PerformAuthorizationFlowNonInteractiveAsync()
        {
            string authUrl = EndPoints.GetAuthorizationUrl(_apiKey);

            using var httpListener = new HttpListener();
            httpListener.Prefixes.Add(_redirectUri);
            httpListener.Start();

            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = false // set true once stable
            });

            var context = await browser.NewContextAsync();
            var page = await context.NewPageAsync();

            // Navigate to authorization page
            await page.GotoAsync(authUrl);

            // ===== STEP 2: GENERATE TOTP =====
            var totp = new Totp(Base32Encoding.ToBytes(_qrCode));
            string otpCode = totp.ComputeTotp();

            // ===== STEP 2: LOGIN =====
            await page.FillAsync("input[id='input-20']", _uid);
            await page.FillAsync("input[id='input-23']", _password);
            await page.FillAsync("input[id='input-27']", otpCode);
            await page.GetByRole(AriaRole.Button, new() { Name = "Log In" }).ClickAsync();
            //await page.ClickAsync("button[id='sbmt']");


            // ===== STEP 4: WAIT FOR REDIRECT =====
            HttpListenerContext contextListener = await httpListener.GetContextAsync();
            var request = contextListener.Request;
            var response = contextListener.Response;

            string? receivedCode = request.QueryString["code"];

            string responseString = "<html><body>Authentication successful. You may close this window.</body></html>";
            byte[] buffer = Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            await response.OutputStream.WriteAsync(buffer);
            response.OutputStream.Close();

            httpListener.Stop();
            await browser.CloseAsync();

            if (string.IsNullOrEmpty(receivedCode))
            {
                return (null, "Authorization code not received");
            }

            return await ExchangeCodeForTokenAsync(receivedCode);
        }

        private async Task<(AccessTokenInfo?, string)> PerformAuthorizationFlowInteractiveAsync()
        {
            // Construct the authorization URL
            string authUrl = $"{EndPoints.GetAuthorizationUrl(_apiKey)}";

            _logger.LogInformation("\nOpening browser for authorization. Please login: {authUrl}", authUrl);

            // Use a lightweight HttpListener to capture the redirect
            using var httpListener = new HttpListener();

            httpListener.Prefixes.Add(_redirectUri); // HttpListener prefixes must end with a slash
            httpListener.Start();
            _logger.LogInformation("Listening for redirect on {_redirectUri}/", _redirectUri);

            // Open the URL in the default browser
            BrowserHelper.OpenUrlInBrowser(authUrl);

            // Wait for the incoming request (the redirect from Groww)
            HttpListenerContext context = await httpListener.GetContextAsync();
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            // Extract the authorization code and state from the redirect URL
            string? receivedCode = request.QueryString["code"];
            //string? receivedState = request.QueryString["client"];

            // Send a simple response to the browser to prevent it from hanging
            string responseString = "<HTML><BODY>Authentication successful! You can close this tab.</BODY></HTML>";
            byte[] buffer = Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            response.OutputStream.Close();

            // Stop listening as soon as we get the response
            httpListener.Stop();
            _logger.LogInformation("HttpListener stopped.");

            if (string.IsNullOrEmpty(receivedCode))
            {
                string eMsg = "Error: Authorization code not received.";
                return (null, eMsg);
            }

            _logger.LogInformation("Authorization code received: {receivedCode}...", receivedCode[0..10]); // Log snippet
            return await ExchangeCodeForTokenAsync(receivedCode);

        }

        private static string CalculateSha256Hash(string input)
        {
            // Use SHA256.Create() for a default implementation (FIPS compliant on Windows)
            // or new SHA256Managed() if targeting older .NET Framework and need specific impl.
            using SHA256 sha256Hash = SHA256.Create();

            // Convert the input string to a byte array and compute the hash.
            // UTF8 is a common and safe encoding for hashing text.
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Convert the byte array to a hexadecimal string.
            StringBuilder builder = new();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2")); // "x2" formats as two lowercase hexadecimal digits
            }
            return builder.ToString();

        }

        private async Task<(AccessTokenInfo?, string)> ExchangeCodeForTokenAsync(string code)
        {
            _logger.LogInformation("Exchanging authorization code for access token...");

            string sha256Hash = CalculateSha256Hash(_apiKey + code + _apiSecret);
            _logger.LogInformation("  SHA-256 Hash (Hexadecimal): {sha256Hash}", sha256Hash);

            var content = new Dictionary<string, string>
            {
                { "api_key", _apiKey },
                { "request_code", code },
                { "api_secret", sha256Hash }
            };

            var serializedContent = JsonConvert.SerializeObject(content);
            var (tokenResponse, eMsg) = await _httpClient.PostMessageAsync<TokenResponse, BaseErrorMessageResponse>(EndPoints.TokenAuthenticationUrl, serializedContent);

            if (tokenResponse != null && !string.IsNullOrEmpty(tokenResponse.AccessToken))
            {
                if (tokenResponse.Status != "Ok")
                {
                    var errorMsg = $"Error in token response: {tokenResponse.Status}: {tokenResponse.ErrorMsg}";
                    return (null, errorMsg);
                }
                _logger.LogInformation("Access token successfully received!");


                var nowLocalTime = DateTime.UtcNow.ToLocalTime();
                DateTime tokenExpiryTime;
                if (nowLocalTime < nowLocalTime.Date.AddHours(5))
                {
                    tokenExpiryTime = nowLocalTime.Date.AddHours(5); // If before 5 AM, set expiry to today at 5 AM
                }
                else
                {
                    tokenExpiryTime = nowLocalTime.Date.AddDays(1).AddHours(5); // Otherwise, set expiry to next day at 5 AM
                }

                _accessToken = new AccessTokenInfo
                {
                    AccessToken = tokenResponse.AccessToken,
                    ClientCode = tokenResponse.Client,
                    ExpiresAtUtc = tokenExpiryTime,
                    LastGeneratedUtc = DateTime.UtcNow
                };

                await SaveAccessTokenAsync(_accessToken, _accessTokenFilePath, _logger);
                return (_accessToken, Constants.StatusOk);
            }
            else
            {
                eMsg = "Failed to receive access token from API. " + eMsg;
                return (null, eMsg);
            }
        }

        private static async Task<AccessTokenInfo?> LoadAccessTokenAsync(string tokenFilePath, ILogger logger)
        {
            if (!File.Exists(tokenFilePath))
            {
                logger.LogWarning("No stored access token file {tokenFilePath} found.", tokenFilePath);
                return null;
            }

            try
            {
                string json = await File.ReadAllTextAsync(tokenFilePath);
                return JsonConvert.DeserializeObject<AccessTokenInfo>(json);

                //return CipherHelper.DecryptObject<AccessTokenInfo?>(jsonEncrypted!, _iv, _encryptionKey);
            }

            catch (JsonSerializationException ex)
            {
                logger.LogError(ex, "  JSON Serialization Exception: {ex.Message}", ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error loading access token from file: {ex.Message}", ex.Message);
            }
            return null;
        }

        private static async Task SaveAccessTokenAsync(AccessTokenInfo tokenInfo, string tokenFilePath, ILogger logger)
        {
            try
            {
                // Use instance fields for encryption key and IV
                //(byte[] encryptedBytes, _iv) = CipherHelper.EncryptObject(tokenInfo, _encryptionKey);

                if (!FileHelper.CreateDirectory(tokenFilePath))
                {
                    logger.LogCritical("Failed to create directory for the file: {tokenFilePath}", tokenFilePath);
                    return;
                }

                string json = JsonConvert.SerializeObject(tokenInfo, Formatting.Indented);
                await File.WriteAllTextAsync(tokenFilePath, json);
                logger.LogInformation("Access token info saved to '{tokenFilePath}'.", tokenFilePath);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error saving access token info to file: {ex.Message}", ex.Message);
            }
        }
    }
}