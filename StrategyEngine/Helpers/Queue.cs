using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading.Channels;

namespace StrategyEngine.Helpers
{
    internal class Queue<T> :IDisposable
    {
        private bool _disposed = false;

        public delegate Task OnEnqueue(T Object);
        private readonly ILogger<Queue<T>> _logger;
        private readonly Channel<T> _channel;
        private readonly Task _task;
        private readonly string _queueFriendlyName;
        private readonly OnEnqueue? _onQueue = null;

        public Queue(int capacity, string queueFriendlyName, OnEnqueue onEnQueue, ILoggerFactory loggerFactory)
        {
            _onQueue = onEnQueue ?? throw new ArgumentNullException(nameof(onEnQueue));
            _logger = loggerFactory.CreateLogger<Queue<T>>();
            _queueFriendlyName = queueFriendlyName;
            _channel = Utility.CreateBoundedChannel<T>(capacity);
            
            _task = WriteAsync();
        }

        public async Task WriteComplete()
        {
            _channel.Writer.Complete();
            await _task;
        }

        public async Task WriteAsync(T obj) => await _channel.Writer.WriteAsync(obj!);

        private async Task WriteAsync()
        {
            _logger.LogInformation("====== [{_queueFriendlyName}] Starting to process ======", _queueFriendlyName);
            int objectsProcessed = 0;
            int objectsfailed = 0;

            await foreach (var reader in _channel.Reader.ReadAllAsync())
            {
                ++objectsProcessed;

                try
                {                    
                    await _onQueue!(reader);
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "[{_queueFriendlyName}] Error {ex.Message}. Queue Message : {message}", _queueFriendlyName, ex.Message, JsonConvert.SerializeObject(reader));
                    ++objectsfailed;
                }               
                
                if ((objectsProcessed + objectsfailed) % 500 == 0)
                    _logger.LogInformation("[{_queueFriendlyName}] Total: {objectsProcessed}, Success: {objectsProcessed}{objectsfailed}", _queueFriendlyName, objectsProcessed + objectsfailed, objectsProcessed, objectsfailed > 0 ? ", Failed: " + objectsfailed : string.Empty);
            }
            _logger.LogInformation("====== [{_queueFriendlyName}] Finished processing. Total: {total}, Success: {processed}, Failed: {failed} ======",
                _queueFriendlyName, objectsProcessed + objectsfailed, objectsProcessed, objectsfailed);
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this); // Prevent finalizer from running again
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources here
                    WriteComplete().GetAwaiter().GetResult();
                }
                // Dispose unmanaged resources here if any
                _disposed = true;
            }
        }

        // Finalizer (only if you have unmanaged resources)
        ~Queue()
        {
            Dispose(false);
        }

    }
}
