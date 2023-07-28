
using Iot.Device.DhcpServer;
using nanoFramework.Runtime.Native;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;

namespace WifiAP
{
    public class Program
    {
        // Start Simple WebServer
        static WebServerSimple server = new WebServerSimple();

        // Connected Station count
        static int connectedCount = 0;

        // GPIO pin used to put device into AP set-up mode
        const int SETUP_PIN = 5;

        static IPAddress localIP = IPAddress.Parse(WirelessAP.SoftApIP);

        public static void Main()
        {
            
            Wireless80211.Disable();
            
            if (WirelessAP.Setup() == false)
            {
                // Reboot device to Activate Access Point on restart
                Debug.WriteLine($"Setup Soft AP, Rebooting device");
                Power.RebootDevice();
            }
          
            var dhcpserver = new DhcpServer
            {
                CaptivePortalUrl = $"http://{WirelessAP.SoftApIP}"
            };
            var dhcpInitResult = dhcpserver.Start(localIP, new IPAddress(new byte[] { 255, 255, 255, 0 }));
            if (!dhcpInitResult)
            {
                Debug.WriteLine($"Error initializing DHCP server.");
            }

            Debug.WriteLine($"Running Soft AP, waiting for client to connect");
           
        
            NetworkChange.NetworkAPStationChanged += NetworkChange_NetworkAPStationChanged;
          
            server.Start(localIP);

            Thread.Sleep(Timeout.Infinite);
        }

      

     
        private static void NetworkChange_NetworkAPStationChanged(int NetworkIndex, NetworkAPStationEventArgs e)
        {
           
            try
            {
                Debug.WriteLine($"NetworkAPStationChanged event Index:{NetworkIndex} Connected:{e.IsConnected} Station:{e.StationIndex} ");
                Debug.WriteLine($"Soft AP IP address :{WirelessAP.GetIP()}");

                if (e.IsConnected)
                {
                    WirelessAPConfiguration wapconf = WirelessAPConfiguration.GetAllWirelessAPConfigurations()[0];
                    WirelessAPStation station = wapconf.GetConnectedStations(e.StationIndex);

                    string macString = BitConverter.ToString(station.MacAddress);
                    Debug.WriteLine($"Station mac {macString} Rssi:{station.Rssi} PhyMode:{station.PhyModes} ");

                }
              
            }
            catch (Exception ex)
            {

                Debug.WriteLine(ex.ToString());
            }


        }
    }
}
