using Microsoft.Extensions.Logging;
using System.Net.WebSockets;
using System.Text;

namespace FlatTrade.Common.Transport
{
    public delegate Task AsyncEventHandler<TEventArgs>(object sender, TEventArgs e);

    public class CustomWebSocket : IAsyncDisposable
    {
        private readonly ILogger<CustomWebSocket> _logger;
        private readonly string _webSocketUri;

        private ClientWebSocket? _clientWebSocket;
        private CancellationTokenSource? _cts;
        private Task? _runLoopTask;
        private bool _disposed;

        private event AsyncEventHandler<string>? _onMessageReceived;

        public CustomWebSocket(Uri uri, AsyncEventHandler<string>? handler, ILoggerFactory loggerFactory)
        {
            if (uri == null || !uri.IsAbsoluteUri ||
                (uri.Scheme != Uri.UriSchemeWs && uri.Scheme != Uri.UriSchemeWss))
            {
                throw new ArgumentException("Invalid WebSocket URI provided.", nameof(uri));
            }

            _webSocketUri = uri.ToString();
            _logger = loggerFactory.CreateLogger<CustomWebSocket>();
            _onMessageReceived = handler;
        }

        // ---------------------------------------------------------------------
        // Public API
        // ---------------------------------------------------------------------

        public void Start()
        {
            // Fire-and-forget non-blocking loop
            _runLoopTask = Task.Run(RunForeverAsync);
        }

        public async Task<bool> SendMessageAsync(string message)
        {
            int cnt = 0;
            while (true)
            {
                ++cnt;
                if (_clientWebSocket == null || _clientWebSocket.State != WebSocketState.Open)
                {
                    _logger.LogWarning("SendMessageAsync: socket not open. Retrying in 500ms.");
                    await Task.Delay(500);
                    continue;
                }
                if (cnt > 5)
                {
                    _logger.LogWarning("SendMessageAsync: socket not open.");
                    return false;
                }
                break;
            }

            try
            {
                var bytes = Encoding.UTF8.GetBytes(message);
                await _clientWebSocket.SendAsync(bytes, WebSocketMessageType.Text, true, _cts!.Token);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message");
                return false;
            }
        }

        // ---------------------------------------------------------------------
        // Core Loop
        // ---------------------------------------------------------------------

        private async Task RunForeverAsync()
        {
            int retry = 0;

            while (!_disposed)
            {
                try
                {
                    await StartAndRunAsync();
                    retry = 0; // reset after successful run
                }
                catch (Exception ex)
                {
                    retry++;
                    var delay = TimeSpan.FromMilliseconds(Math.Min(1000, 100 * retry));
                    _logger.LogWarning(ex, "WebSocket disconnected, retrying in {Delay}s", delay.TotalSeconds);
                    await Task.Delay(delay);
                }
            }
        }

        private async Task StartAndRunAsync()
        {
            _cts = new CancellationTokenSource();
            _clientWebSocket = new ClientWebSocket();

            await _clientWebSocket.ConnectAsync(new Uri(_webSocketUri), _cts.Token);
            _logger.LogInformation("Connected to {Uri}", _webSocketUri);

            var receive = ReceiveLoopAsync(_cts.Token);
            var keepAlive = KeepAliveLoopAsync(_cts.Token);

            await Task.WhenAny(receive, keepAlive);

            _logger.LogWarning("WebSocket loop ended.");
        }

        // ---------------------------------------------------------------------
        // Message handling
        // ---------------------------------------------------------------------

        private async Task ReceiveLoopAsync(CancellationToken token)
        {
            var buffer = new byte[8192];

            try
            {
                while (!token.IsCancellationRequested &&
                       _clientWebSocket?.State == WebSocketState.Open)
                {
                    var result = await _clientWebSocket.ReceiveAsync(buffer, token);

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var msg = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        if (_onMessageReceived != null)
                            await _onMessageReceived(this, msg);
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        _logger.LogWarning("Server requested close: {Status} {Desc}",
                            result.CloseStatus, result.CloseStatusDescription);
                        break;
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (WebSocketException ex)
            {
                _logger.LogWarning(ex, "Receive loop error");
            }
        }

        private async Task KeepAliveLoopAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested &&
                       _clientWebSocket?.State == WebSocketState.Open)
                {
                    await Task.Delay(TimeSpan.FromSeconds(15), token);
                    var ping = Encoding.UTF8.GetBytes("ping");
                    await _clientWebSocket.SendAsync(ping, WebSocketMessageType.Text, true, token);
                    _logger.LogDebug("Sent keep-alive ping");
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Keep-alive failed");
            }
        }

        // ---------------------------------------------------------------------
        // Disposal
        // ---------------------------------------------------------------------

        public void Dispose()
        {
            DisposeAsyncCore().AsTask().GetAwaiter().GetResult(); // Safe sync fallback
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore();
            GC.SuppressFinalize(this);
        }

        protected async ValueTask DisposeAsyncCore()
        {
            if (_disposed) return;
            _disposed = true;

            try
            {
                _cts?.Cancel();

                if (_clientWebSocket != null &&
                    (_clientWebSocket.State == WebSocketState.Open ||
                     _clientWebSocket.State == WebSocketState.CloseReceived))
                {
                    await _clientWebSocket.CloseAsync(
                        WebSocketCloseStatus.NormalClosure,
                        "Application shutting down",
                        CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error during DisposeAsync");
            }
            finally
            {
                _cts?.Dispose();
                _clientWebSocket?.Dispose();

                if (_runLoopTask != null)
                    await Task.WhenAny(_runLoopTask, Task.Delay(1000)); // wait briefly
            }

            _logger.LogInformation("{0}: Disposed gracefully", GetType().Name);
        }
    }
}
