using System;
using System.Text;

namespace TestCOMportEncoder
{
    public class DeviceProcessor
    {
        private const int BASE_AMPLITUDE_CALCULATION_LEVEL = 80;
        private const int AMPLITUDE_ACCURACY_COEFFICIENT = 10;

        private int _lastPointId = -1;
        private int _pointIndex = -1;

        private readonly long _start = 1800000000;
        private readonly long _step = 1000000;
        private readonly int _intermediateFrequency = 10700000;
        private readonly double _attenuation = 0.0;

        public DeviceProcessor(string config)
        {
            string[] values = config.Split(' ');
            if (values.Length != 6) {
                throw new ArgumentException("Invalid Device configuration string.");
            }

            if (!int.TryParse(values[0], out int lastPointId) ||
                !int.TryParse(values[1], out int pointIndex) ||
                !long.TryParse(values[2], out long start) ||
                !long.TryParse(values[3], out long step) ||
                !int.TryParse(values[4], out int intermediateFrequency) ||
                !double.TryParse(values[5], out double attenuation)) {
                throw new ArgumentException("Invalid Device configuration values.");
            }

            _lastPointId = lastPointId;
            _pointIndex = pointIndex;
            _start = start;
            _step = step;
            _intermediateFrequency = intermediateFrequency;
            _attenuation = attenuation;
        }

        public void NormilizeMessage(string response, ref int index, ref byte[] encodedData, ref int _elapsedTime, ref string status)
        {
            string[] responseParts = response.Replace("\r\n", " ").Split(' ');
            string encodedDataString = "";
            index = int.Parse(responseParts[3]);
            status = responseParts[responseParts.Length - 2];

            responseParts[responseParts.Length - 2] = "";
            responseParts[responseParts.Length - 1] = "";

            for (int i = 4; i < responseParts.Length; i += 1) {
                encodedDataString += responseParts[i];
            }

            string input = encodedDataString, numberString = "";

            int iter = input.Length - 1;

            while (iter >= 0 && !char.IsDigit(input[iter])) {
                iter--;
            }

            while (iter >= 0 && char.IsDigit(input[iter])) {
                numberString = input[iter] + numberString;
                iter--;
            }

            string encodedDataWithoutNumber = input.Substring(0, iter + 1);

            encodedData = Encoding.UTF8.GetBytes(encodedDataWithoutNumber);

            Console.WriteLine($"Index: \t\t{index}");
            Console.WriteLine($"Encoded data: \t{string.Join(", ", encodedData)}" + encodedData.Length);

            if (!int.TryParse(numberString, out int elapsedTime)) {
                Console.WriteLine("Elapsed time: Could not parse.");
            }
            else {
                _elapsedTime = elapsedTime;
                Console.WriteLine($"Elapsed time: \t{elapsedTime}");
            }

            Console.WriteLine($"Status: \t{status}");
        }

        public void ProcessDeviceResponse(byte[] message)
        {
            int messageLength = message.Length;
            if (messageLength == 2 || messageLength == 6) {
                ProcessMessage(message);
            }
            else {
                string readMessage = Encoding.ASCII.GetString(message, 0, message.Length);
                byte[] messageBytes = Encoding.UTF8.GetBytes(readMessage);
                ProcessMessage(messageBytes);
            }
        }

        public double ProcessMessage(byte[] message)
        {
            int messageLength = message.Length;

            if (messageLength == 6)
                _pointIndex = (message[0] << 24) | (message[1] << 16) | (message[2] << 8) | message[3];

            int pointId = (message[messageLength - 2] & 0x000000FF) >> 3;
            int amplitudeIntValue = ((message[messageLength - 2] & 0x00000007) << 8) | (message[messageLength - 1] & 0x000000FF);

            long frequency = _start + (_pointIndex * _step) + ((_intermediateFrequency * 2) * _pointIndex);

            double amplitude = (((BASE_AMPLITUDE_CALCULATION_LEVEL * AMPLITUDE_ACCURACY_COEFFICIENT) - amplitudeIntValue) / (double)AMPLITUDE_ACCURACY_COEFFICIENT) - _attenuation;

            if (_lastPointId < pointId || pointId == 0) {
                _lastPointId = pointId;
                _pointIndex++;
            }
            ReceiveStreamData(frequency, amplitude, $"\tbytes[] = {message[0]} {message[1]}");

            return amplitude;
        }

        private void ReceiveStreamData(long frequency, double amplitude, string comm = "")
        {
            // if(amplitude > 0)
            Console.WriteLine("Received data for frequency: " + frequency + " with amplitude: " + amplitude + " " + comm);
        }
    }
}