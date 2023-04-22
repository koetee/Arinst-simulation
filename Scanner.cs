using ArinstSimulation;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestCOMportEncoder
{
    public class Scanner
    {
        private const double AMPLITUDE_ACCURACY_COEFFICIENT = 10;
        private const int BASE_AMPLITUDE_CALCULATION_LEVEL = 80;
        private const byte TERMINATION_BYTE = 0xFF;

        private int _formattedAttenuation;
        private int _index;
        private int _intermediateFrequency;
        private int _samples;
        private long _start;
        private long _step;
        private long _stop;
        private int _timeout;

        public Scanner(string config)
        {
            string[] values = config.Split(' ');
            if (values.Length != 9) {
                throw new ArgumentException("Invalid scanner configuration string.");
            }

            if (!int.TryParse(values[1], out int start) ||
                !int.TryParse(values[2], out int stop) ||
                !int.TryParse(values[3], out int step) ||
                !int.TryParse(values[4], out int timeout) ||
                !int.TryParse(values[5], out int samples) ||
                !int.TryParse(values[6], out int intermediateFrequency) ||
                !int.TryParse(values[7], out int formattedAttenuation) ||
                !int.TryParse(values[8], out int index)) {
                throw new ArgumentException("Invalid scanner configuration values.");
            }

            _start = start;
            _stop = stop;
            _step = step;
            _timeout = timeout;
            _samples = samples;
            _intermediateFrequency = intermediateFrequency;
            _formattedAttenuation = formattedAttenuation;
            _index = index;
        }

        public string Scan()
        {
            List<byte> amplitudeData = new List<byte>();
            int totalPoints = (int)Math.Ceiling((_stop - _start) / (double)_step);

            // Write header bytes <Start, Index>
            amplitudeData.Add((byte)(_index >> 16));
            amplitudeData.Add((byte)_index);
            amplitudeData.Add((byte)(totalPoints >> 16));
            amplitudeData.Add((byte)(totalPoints));

            // <encoded_data>
            // Encode the amplitude value as a 2-byte sequence
            // Add the amplitude bytes to the data buffer
            amplitudeData.AddRange(
                SignalGenerator.SignalToBytes(
                    SignalGenerator.Generate(_start / 1000000, _stop / 1000000, 50)));

            // Add termination bytes to the amplitude data buffer
            amplitudeData.Add(TERMINATION_BYTE);
            amplitudeData.Add(TERMINATION_BYTE);

            // Calculate the elapsed time for the scan
            int elapsedMilliseconds = totalPoints >> 16 * _samples * _timeout;

            // Build the response string
            StringBuilder responseBuilder = new StringBuilder();
            responseBuilder.AppendFormat("\r\nscn20 {0} {1}\r\n", _start, _index);
            responseBuilder.Append(Encoding.UTF8.GetString(amplitudeData.ToArray()));
            responseBuilder.AppendFormat("{0}\r\ncomplete\r\n", elapsedMilliseconds);

            return responseBuilder.ToString();
        }

        private static double NextGaussian(double mean, double stddev)
        {
            double u1 = 1.0 - new Random().NextDouble();
            double u2 = 1.0 - new Random().NextDouble();
            double z = (Math.Sqrt(-2.0 * Math.Log(u1))) * Math.Sin(2.0 * Math.PI * u2);
            return mean + (stddev * z);
        }
    }
}