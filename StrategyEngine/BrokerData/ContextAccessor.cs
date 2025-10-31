using FlatTrade;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StrategyEngine.Strategies;

namespace StrategyEngine.BrokerData
{
    internal sealed class ContextAccessor : IAsyncDisposable, IDisposable
    {
        private bool _disposed = false;
        private readonly IConfiguration _config;
        private readonly Api _api;
        private readonly ILogger _logger;

        public Holding Holding { get;}
        public Security Security { get;}
        public Trade Trade { get; }
        public Position Position { get; }
        public Order Order { get; }
        public Candle Candle { get; }
        public Quote Quote { get; }
        
        public TouchLine TouchLine { get; }

        public ContextAccessor(IConfiguration config, Api api, OnUpdate? onUpdate, ILoggerFactory loggerFactory)
        {
            _api = api;
            _config = config;

            _logger = loggerFactory.CreateLogger<ContextAccessor>();
            Holding = new(config, this, api, onUpdate, loggerFactory);
            Security = new(config, this, api, onUpdate, loggerFactory);
            Trade = new(config, this, api, onUpdate, loggerFactory);
            Position = new(config, this, api, onUpdate, loggerFactory);
            Candle = new(config, this, api, onUpdate, loggerFactory);
            Order = new(config, this, api, onUpdate, loggerFactory);
            Quote = new(config, this, api, onUpdate, loggerFactory);
            TouchLine = new(config, this, api, onUpdate, loggerFactory);
        }

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

        private async ValueTask DisposeAsyncCore()
        {
            if (_disposed)
                return;

            _disposed = true;

            await Quote.DisposeAsync();
            await Order.DisposeAsync();
            await TouchLine.DisposeAsync();
            await Candle.DisposeAsync();

            _logger.LogInformation("{0}: Disposed gracefully", GetType().Name);
            // Dispose other sync-only resources here (e.g., timers, files)
        }
    }
}
