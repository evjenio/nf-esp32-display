using System;
using System.Diagnostics;
using System.Device.WiFi;
using System.Threading;
using nanoFramework.Networking;
using System.Net.NetworkInformation;

#nullable enable

namespace NfEsp32Display
{
    public static class Wifi
    {
        static readonly AutoResetEvent signal = new(false);
        static WiFiAvailableNetwork[]? availableNetworks;

        public static WiFiAvailableNetwork[]? Scan()
        {
            var wifiAdapter = WiFiAdapter.FindAllAdapters()[0];

            try
            {
                wifiAdapter.AvailableNetworksChanged += AvailableNetworksChanged;

                Debug.WriteLine("starting WiFi scan");
                wifiAdapter.ScanAsync();

                var success = signal.WaitOne(30000, true);
                if (success)
                {
                    Debug.WriteLine("scan complete");
                    return availableNetworks;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("message:" + ex.Message);
                Debug.WriteLine("stack:" + ex.StackTrace);
            }
            finally
            {
                wifiAdapter.AvailableNetworksChanged -= AvailableNetworksChanged;
                signal.Reset();
            }
            return null;
        }

        public static WiFiAvailableNetwork SearchFor(string ssid)
        {
            WiFiAvailableNetwork? network = null;

            while (network == null)
            {
                var availableNetworks = Scan();
                if (availableNetworks == null)
                {
                    Thread.Sleep(TimeSpan.FromMinutes(1));
                    continue;
                }

                foreach (var availableNetwork in availableNetworks)
                {
                    if (string.Equals(availableNetwork.Ssid, ssid))
                    {
                        network = availableNetwork;
                        return network;
                    }
                }
            }

            throw new InvalidOperationException();
        }

        public static bool ConnectTo(WiFiAvailableNetwork network, string password)
        {
            var result = WiFiNetworkHelper.ConnectDhcp(network.Ssid, password, token: new CancellationTokenSource(10000).Token);
            Wireless80211Configuration wirelessConfiguration = GetConfiguration();
            return result;
        }

        public static string GetIp() => GetInterface().IPv4Address;
        

        private static Wireless80211Configuration GetConfiguration()
        {
            NetworkInterface networkInterface = GetInterface();
            return Wireless80211Configuration.GetAllWireless80211Configurations()[networkInterface.SpecificConfigId];
        }

        private static NetworkInterface GetInterface()
        {
            NetworkInterface[] Interfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface ni in Interfaces)
            {
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                {
                    return ni;
                }
            }
            return null;
        }

        private static void AvailableNetworksChanged(WiFiAdapter wifiAdapter, object args)
        {
            Debug.WriteLine("WiFi networks:");

            var report = wifiAdapter.NetworkReport;
            availableNetworks = report.AvailableNetworks;

            Debug.WriteLine(Table.Row("SSID", "BSSID", "RSSI", "SIGNAL"));


            foreach (var network in report.AvailableNetworks)
            {
                Debug.WriteLine(Table.Row(network.Ssid, network.Bsid, $"{network.NetworkRssiInDecibelMilliwatts}", $"{network.SignalBars}/4"));
            }
            signal.Set();
        }
    }
}
