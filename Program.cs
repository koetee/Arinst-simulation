using ArinstSimulation;
using System;


namespace TestCOMportEncoder
{
    class Program
    {
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

            device.normilizeMessage(response, ref index, ref encData, ref elapsedTime, ref status);

            for (int i = 4; i < encData[3] * 2; i += 2) 
                device.ProcessDeviceResponse(new byte[] { encData[i], encData[i + 1] });
            
        }
    }
}
