using TicTacTec.TA.Library;

namespace StrategyEngine.Strategies.Indicators
{
    internal class Technicals
    {
        public static double[] Ema(double[] close, int period)
        {
            int outBegIdx, outNbElement;
            var output = new double[close.Length];
            Core.Ema(0, close.Length - 1, close, period, out outBegIdx, out outNbElement, output);

            // Shift result to align with input
            return output.Skip(outBegIdx).Take(outNbElement).ToArray();
        }

        public static (double[] macd, double[] signal, double[] hist) Macd(double[] close, int fast = 12, int slow = 26, int signalPeriod = 9)
        {
            int outBegIdx, outNbElement;
            var macd = new double[close.Length];
            var signal = new double[close.Length];
            var hist = new double[close.Length];

            Core.Macd(0, close.Length - 1, close, fast, slow, signalPeriod,
                        out outBegIdx, out outNbElement, macd, signal, hist);

            return (macd.Skip(outBegIdx).ToArray(),
                    signal.Skip(outBegIdx).ToArray(),
                    hist.Skip(outBegIdx).ToArray());
        }
    }        
}
