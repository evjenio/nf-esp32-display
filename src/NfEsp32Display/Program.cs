using System.Diagnostics;
using System.Threading;

#nullable enable

namespace NfEsp32Display
{
    public class Program
    {
        const string Ssid = "NanoFrameworkDevice";
        const string Password = "11111111";

        public static void Main()
        {
            var network = Wifi.SearchFor(Ssid);
            var status = Wifi.ConnectTo(network, Password);

            Debug.WriteLine($"Connection status: {status}");
            Debug.WriteLine($"IP: {Wifi.GetIp()}");

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
