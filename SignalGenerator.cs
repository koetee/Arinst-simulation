using System;


namespace ArinstSimulation
{
    public class SignalGenerator
    {
        private const double AMPLITUDE_FACTOR = 1.4142135623730951; // sqrt(2)
        private const double PHASE_FACTOR = 2 * Math.PI / TimeSpan.TicksPerSecond;
        private const double RANDOMFACTOR = 1.0 / (2.0 * int.MaxValue);

        private static long seed = DateTime.Now.Ticks;

        public static double Generate(double frequency)
        {
            double amplitude = AMPLITUDE_FACTOR * Math.Sqrt(frequency);
            double phase = seed * frequency * PHASE_FACTOR;

            seed = ((seed * 1103515245L) + 12345L) % 0x100000000L;

            double signal = (amplitude * Math.Sin(phase)) + (seed * RANDOMFACTOR) - 0.5;

            return signal;
        }

        public static double[] Generate(double frequencyStart, double frequencyEnd, int sampleCount)
        {
            double deltaFrequency = (frequencyEnd - frequencyStart) / sampleCount;
            double[] signal = new double[sampleCount];

            for (int i = 0; i < sampleCount; i++) {
                double frequency = frequencyStart + (i * deltaFrequency);
                signal[i] = Generate(frequency);
            }

            return signal;
        }

        public static byte[] SignalToBytes(double[] signal)
        {
            byte[] bytes = new byte[signal.Length * sizeof(double)];

            Buffer.BlockCopy(signal, 0, bytes, 0, bytes.Length);

            return bytes;
        }

        public static double[] BytesToSignal(byte[] bytes)
        {
            double[] signal = new double[bytes.Length / sizeof(double)];

            Buffer.BlockCopy(bytes, 0, signal, 0, bytes.Length);

            return signal;
        }
    }
}
