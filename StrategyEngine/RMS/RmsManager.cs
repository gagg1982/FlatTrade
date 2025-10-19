using HtmlAgilityPack;
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

        public bool IsValidationSucceeded(StrategySignal signal)
        {
            if (!Enabled)
                return !Enabled;

            bool validated = true;
            foreach (var rule in Rules)
            {
                if (!rule.Key.IsValidationSucceeded(signal))
                {
                    validated = false;
                    break;
                }
            }

            return validated;
        }
    }
}
