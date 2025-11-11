using FlatTrade;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StrategyEngine.Model;
using StrategyEngine.OrderProcessors;
using System.Collections.Concurrent;

namespace StrategyEngine.RMS
{
    internal class RmsManager(IConfiguration config, Api api, IOrderProcessor orderProcessor, ILoggerFactory loggerFactory)
        : AbstractRms<RmsManager>(config, api, orderProcessor, loggerFactory), IAsyncDisposable, IDisposable
    {
        private bool Enabled { get; }
        private bool _disposed = false;

        private ConcurrentDictionary<IRms, bool> Rules { get; } = [];

        protected override string Name => $"{GetType().Name}_RmsExecutor";
        
        public virtual void Dispose()
        {
            DisposeAsyncCore().AsTask().GetAwaiter().GetResult(); // Safe sync fallback
            GC.SuppressFinalize(this);
        }

        public virtual async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore();
            GC.SuppressFinalize(this);
        }

        private async ValueTask DisposeAsyncCore()
        {
            if (_disposed)
                return;

            _disposed = true;

            // Dispose async resources             

            _logger.LogInformation("{0}: Disposed gracefully", GetType().Name);
            await Task.FromResult<ValueTask>(default);
        }

        public IRms Register(IRms rule) { Rules.AddOrUpdate(rule, true, (_, _) => true); return this; }

        public IRms UnRegister(IRms rule) { Rules.Remove(rule, out bool _); return this; }

        public int Count() => Rules.Count;

        public IRms Clear() { Rules.Clear(); return this;}

        public override async Task<bool> IsValidationSucceeded(StrategySignal signal)
        {
            if (!Enabled)
                return !Enabled;

            List<Task<bool>> validated = [];
            foreach (var rule in Rules)
                validated.Add(rule.Key.IsValidationSucceeded(signal));
            
            return await WaitForAllOrAnyFalseAsync(validated);
        }

        private static async Task<bool> WaitForAllOrAnyFalseAsync(IEnumerable<Task<bool>> tasks)
        {
            var taskList = tasks.ToList();
            var remaining = new List<Task<bool>>(taskList);

            while (remaining.Count > 0)
            {
                var finished = await Task.WhenAny(remaining);
                remaining.Remove(finished);

                if (!await finished) // if any returns false, exit early
                    return false;
            }

            // all completed successfully (all true)
            return true;
        }
    }
}
