using StrategyEngine.Strategy;
using System.Collections.Concurrent;

namespace StrategyEngine.RMS
{
    public class RmsManager : IRMS
    {
        private bool Enabled { get; }
        private ConcurrentDictionary<IRMS, bool> Rules { get; } = [];

        public IRMS Register(IRMS rule) { Rules.AddOrUpdate(rule, true, (_, _) => true); return this; }

        public IRMS UnRegister(IRMS rule) { Rules.Remove(rule, out bool _); return this; }

        public int Count() => Rules.Count;

        public IRMS Clear() { Rules.Clear(); return this;}

        public async Task<bool> IsValidationSucceeded(StrategySignal signal)
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
