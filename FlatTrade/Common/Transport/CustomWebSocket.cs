using Microsoft.Extensions.Logging;
using System.Net.WebSockets;
using System.Text;

namespace FlatTrade.Common.Transport
{
    public delegate Task AsyncEventHandler<TEventArgs>(object sender, TEventArgs e);
    public class CustomWebSocket
    {
        private readonly ILogger<CustomWebSocket> _logger;
        private readonly string _webSocketUri = string.Empty;

        private ClientWebSocket? _clientWebSocket; // The WebSocket client instance
        private CancellationTokenSource? _cancellationTokenSource; // For managing task cancellation

        public event AsyncEventHandler<string>? OnMessageReceived;

        private readonly Thread _keepAliveThread;
        private bool _running = false;

        public CustomWebSocket(Uri uri, ILoggerFactory loggerFactory)
        {
            if (uri == null || !uri.IsAbsoluteUri || uri.Scheme != Uri.UriSchemeWs && uri.Scheme != Uri.UriSchemeWss)
            {
                throw new ArgumentException("Invalid WebSocket URI provided.", nameof(uri));
            }
            _webSocketUri = uri.ToString(); // Store the valid WebSocket URI
            _logger = loggerFactory.CreateLogger<CustomWebSocket>();
            _cancellationTokenSource = new CancellationTokenSource(); // Initialize cancellation token source

            _running = true;
            _keepAliveThread = new Thread(async () => await KeepAliveLoop())
            {
                IsBackground = true
            };

        }
        private async Task KeepAliveLoop()
        {
            while (_running && _clientWebSocket!.State == WebSocketState.Open)
            {
                try
                {
                    // Send ping (here just a text "ping", but could be custom protocol-level ping)
                    var buffer = Encoding.UTF8.GetBytes("ping");
                    await _clientWebSocket.SendAsync(new ArraySegment<byte>(buffer),
                                        WebSocketMessageType.Text,
                                        true,
                                        _cancellationTokenSource!.Token);

                    _logger.LogDebug("Sent keep-alive ping");

                    // Wait 15s before next ping
                    await Task.Delay(TimeSpan.FromSeconds(15), _cancellationTokenSource!.Token);
                }
                catch (Exception ex)
                {
                    _logger.LogDebug("Keep-alive failed : {ex.Message}", ex.Message);
                    break;
                }
            }
        }

        public async Task<bool> StartAsync()
        {
            _logger.LogInformation("Attempting to connect to: {_webSocketUri}", _webSocketUri);

            try
            {
                _clientWebSocket = new ClientWebSocket();

                // 1. Connect to the WebSocket server
                // The CancellationToken can be used to abort the connection attempt if it takes too long.
                await _clientWebSocket.ConnectAsync(new Uri(_webSocketUri), _cancellationTokenSource!.Token);
                _logger.LogInformation("WebSocket connected successfully!");

                _ = Task.Run(async () => await ReceiveLoopAsync(_cancellationTokenSource.Token));
                _logger.LogInformation("Started background message receiving loop.");

                _keepAliveThread.Start();
            }
            catch (WebSocketException wse)
            {
                _logger.LogError("WebSocket connection error: {wseMessage}", wse.Message);
                if (wse.InnerException != null)
                    _logger.LogError("  Inner Exception: {wseInnerExceptionMessage}", wse.InnerException.Message);

                _clientWebSocket?.Dispose();
                _clientWebSocket = null;
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
                throw;
            }
            catch (UriFormatException ufe)
            {
                _logger.LogCritical("Invalid WebSocket URI format: {ufeMessage}", ufe.Message);
                return false;
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("WebSocket connection attempt was cancelled.");
                _clientWebSocket?.Dispose();
                _clientWebSocket = null;
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
                throw; // Re-throw
            }
            catch (Exception ex)
            {
                _logger.LogCritical("An unexpected error occurred during StartAsync: {exMessage}", ex.Message);
                _clientWebSocket?.Dispose();
                _clientWebSocket = null;
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
                throw; // Re-throw
            }

            return true;
        }

        /// <summary>
        /// Handles sending messages to the WebSocket server.
        /// </summary>
        public async Task<bool> SendMessageAsync(string message)
        {
            // Check if the WebSocket is connected and ready to send
            if (_clientWebSocket == null || _clientWebSocket.State != WebSocketState.Open)
            {
                _logger.LogError("SendMessageAsync: WebSocket is not open. Message not sent.");
                return false;
            }

            // Convert string message to bytes using UTF8 encoding
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            var segment = new ArraySegment<byte>(buffer);

            try
            {
                // Send the message as text. 'true' for 'endOfMessage' indicates this is a complete message.
                await _clientWebSocket.SendAsync(segment, WebSocketMessageType.Text, true, _cancellationTokenSource!.Token);
                _logger.LogDebug("  Sent: '{message}'", message);
                return true;
            }
            catch (WebSocketException wse)
            {
                _logger.LogError("Error sending message: {wse.Message}", wse.Message);
                // If sending fails, it indicates a problem, so initiate shutdown.
                _cancellationTokenSource?.Cancel();
            }
            catch (OperationCanceledException)
            {
                _logger.LogError("Sending was cancelled.");
            }
            catch (Exception ex)
            {
                _logger.LogCritical("Unexpected error sending message: {ex.Message}", ex.Message);
                _cancellationTokenSource?.Cancel();
            }
            return false;
        }

        /// <summary>
        /// Handles receiving messages from the WebSocket server.
        /// </summary>
        private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
        {
            if (_clientWebSocket == null || _clientWebSocket.State != WebSocketState.Open)
            {
                _logger.LogInformation("ReceiveLoop: WebSocket is not open, cannot start receive loop.");
                return;
            }

            // Use a larger buffer for receiving to handle larger messages efficiently.
            // WebSocket messages can be fragmented, but ClientWebSocket handles reassembly for full messages.
            byte[] buffer = new byte[1024 * 4]; // 4KB buffer
            var segment = new ArraySegment<byte>(buffer);

            try
            {
                // Continue receiving as long as the WebSocket is open and cancellation has not been requested
                while (_clientWebSocket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
                {
                    // Receive a message. This call blocks until a message is received or the connection is closed/aborted.
                    WebSocketReceiveResult result = await _clientWebSocket.ReceiveAsync(segment, cancellationToken);

                    // Process the received message based on its type
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        string receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        _logger.LogDebug("  Recv: '{receivedMessage}'", receivedMessage);
                        OnMessageReceived?.Invoke(this, receivedMessage);
                    }
                    else if (result.MessageType == WebSocketMessageType.Binary)
                    {
                        _logger.LogInformation("  Received (Binary): {resultCount} bytes. (Not currently processed)", result.Count);
                        // If you expect binary data, process the 'buffer' here.
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        _logger.LogWarning("  Received Close message from server. Status: {resultCloseStatus} ({resultCloseStatusDescription})", result.CloseStatus, result.CloseStatusDescription);
                        // Server requested to close. Acknowledge the close.
                        // Use CancellationToken.None to ensure the close operation itself isn't cancelled.
                        await _clientWebSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Client closing", CancellationToken.None);
                        // Signal to stop the loop and client
                        //_cancellationTokenSource?.Cancel();
                        await StartAsync();
                        break;
                    }
                }
            }
            catch (WebSocketException wse)
            {
                _logger.LogError("Error in ReceiveLoop: {wseMessage}", wse.Message);
                // Initiate cancellation if an error occurs to stop other tasks and cleanup.
                //_cancellationTokenSource?.Cancel();
                await StartAsync();
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Receive loop was cancelled.");
            }
            catch (Exception ex)
            {
                _logger.LogCritical("An unexpected error occurred in ReceiveLoop: {exMessage}", ex.Message);
                _cancellationTokenSource?.Cancel();
            }
        }

        /// <summary>
        /// Stops the WebSocket client, closing the connection gracefully.
        /// </summary>
        public async Task StopAsync()
        {
            if (_clientWebSocket == null) return;

            // Signal cancellation to any ongoing operations (like ReceiveLoopAsync)
            _cancellationTokenSource?.Cancel();

            // Attempt to close the WebSocket gracefully if it's still open
            if (_clientWebSocket.State == WebSocketState.Open || _clientWebSocket.State == WebSocketState.CloseReceived)
            {
                _logger.LogInformation("Closing WebSocket connection...");
                try
                {
                    // Close the output side of the connection gracefully.
                    // The server should respond with its own close frame.
                    await _clientWebSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Client initiated closure", CancellationToken.None);

                    // Optionally, wait for the server's close acknowledgment.
                    // This will block until the server sends its close frame or timeout.
                    // This can be omitted if you don't need to wait for server acknowledgement.
                    // await _clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client finished", CancellationToken.None);
                }
                catch (WebSocketException wse)
                {
                    _logger.LogError("Error during WebSocket close: {wse.Message}", wse.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "An unexpected error occurred during WebSocket close: {ex.Message}", ex.Message);
                }
            }

            // Dispose the ClientWebSocket and CancellationTokenSource to release resources.
            _clientWebSocket.Dispose();
            _clientWebSocket = null;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
            _running = false;

            _logger.LogInformation("WebSocket client resources released.");
        }
    }
}
