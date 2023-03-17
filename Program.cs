using System;


namespace TestCOMportEncoder
{
    class Program
    {
        public static byte[] GenerateDeviceQuery(string input)
        {
            string[] args = input.Split(' ');
            if (args.Length != 9) {
                throw new ArgumentException("Invalid input format. Expected 9 space-separated values.");
            }

            // Parse input arguments
            string deviceName = args[0];
            long start = long.Parse(args[1]);
            long end = long.Parse(args[2]);
            long step = long.Parse(args[3]);
            int dwell = int.Parse(args[4]);
            int gain = int.Parse(args[5]);
            long ifFreq = long.Parse(args[6]);
            int attenuation = int.Parse(args[7]);
            int points = int.Parse(args[8]);

            // Construct message byte array
            byte[] message = new byte[6];
            message[0] = (byte)((start >> 24) & 0xFF);
            message[1] = (byte)((start >> 16) & 0xFF);
            message[2] = (byte)((start >> 8) & 0xFF);
            message[3] = (byte)(start & 0xFF);
            message[4] = (byte)(points & 0xFF);
            message[5] = (byte)(((dwell & 0x03) << 5) | ((gain & 0x03) << 3) | (attenuation & 0x07));

            return message;
        }
        static void Main(string[] args)
        {
            DeviceProcessor device = new DeviceProcessor("-1 -1 1800000000 1000000 10700000 0");
            Scanner ComPort1 = new Scanner("scn20 1800000000 1900000000 1000000 200 20 107000000 8500 8");

            string response = ComPort1.Scan();

            Console.WriteLine("response: " + response);

            int index = 0;
            byte[] encData = null;
            int elapsedTime = 0;
            string status = "";

            device.normilizeMessage(response, ref index,ref encData, ref elapsedTime, ref status);

            for (int i = 4; i < encData[3] * 2; i += 2) {

                device.ProcessDeviceResponse(new byte[] { encData[i], encData[i + 1] });
            }



        }
    }
}
