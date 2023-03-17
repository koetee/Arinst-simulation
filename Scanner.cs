using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;


namespace TestCOMportEncoder
{
    public class Scanner
    {
        private const int BASE_AMPLITUDE_CALCULATION_LEVEL = 80;
        private const double AMPLITUDE_ACCURACY_COEFFICIENT = 10;

        private long _start;
        private long _stop;
        private long _step;
        private int _timeout;
        private int _samples;
        private int _intermediateFrequency;
        private int _formattedAttenuation;
        private int _index;

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
                Console.ReadLine();
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
            long frequency = _start;
            List<byte> amplitudeData = new List<byte>();
            int totalPoints = (int)Math.Ceiling((_stop - _start) / (double)_step);
          
            // Write header bytes
            amplitudeData.Add((byte)(_index >> 16));
            amplitudeData.Add((byte)((byte)_index));
            amplitudeData.Add((byte)(totalPoints >> 16));
            amplitudeData.Add((byte)(totalPoints));

            for (int i = 0; i < totalPoints; i++) {
                // Generate a random amplitude value
                int amplitude = new Random().Next(0, (int)(BASE_AMPLITUDE_CALCULATION_LEVEL * AMPLITUDE_ACCURACY_COEFFICIENT));

                // Encode the amplitude value as a 2-byte sequence
                // Add the amplitude bytes to the data buffer
                amplitudeData.AddRange(BitConverter.GetBytes((short)((i << 11) | (amplitude & 0x7FF))));
                amplitudeData.AddRange(BitConverter.GetBytes((short)((frequency & 0x7FF))));
                
                // Update the frequency for the next point
                frequency += _step;
            }

            // Add termination bytes to the amplitude data buffer
            amplitudeData.Add(0xFF);
            amplitudeData.Add(0xFF);

            // Calculate the elapsed time for the scan
            int elapsedMilliseconds = totalPoints >> 16 * _samples * _timeout;

            // Build the response string
            StringBuilder responseBuilder = new StringBuilder();
            responseBuilder.AppendFormat("\r\nscn20 {0} {1}\r\n", _start, _index);
            responseBuilder.Append(Encoding.UTF8.GetString(amplitudeData.ToArray()));
            responseBuilder.AppendFormat("{0}\r\ncomplete\r\n", elapsedMilliseconds);
 
            return responseBuilder.ToString();
        }
      
    }
}