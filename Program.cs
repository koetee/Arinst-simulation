using System.Collections.Generic;

namespace TestCOMportEncoder
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            DeviceProcessor device = new DeviceProcessor("-1 -1 1800000000 1000000 10700000 0");
            Scanner ComPort1 = new Scanner("scn20 1800000000 1900000000 1000000 200 20 107000000 8500 8");
            List<double> dat = new List<double>();

            string response;

            int index = 0;
            byte[] encData = null;
            int elapsedTime = 0;
            string status = "";

            for (int j = 0; j < 1; j++) {
                response = ComPort1.Scan();
                device.normilizeMessage(response, ref index, ref encData, ref elapsedTime, ref status);

                for (int i = 4; i < encData[3] * 2; i += 2)
                    dat.Add(device.ProcessMessage(new byte[] { encData[i], encData[i + 1] }));
            }
        }
    }
}