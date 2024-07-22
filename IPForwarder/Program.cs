
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Open.Nat;

namespace IPForwarder
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                // Fetch public IP from an external service
                var publicIp = await GetPublicIpAddressAsync();
                Console.WriteLine($"Your public IP address is: {publicIp}");

                // Port forwarding setup
                var discoverer = new NatDiscoverer();
                var cts = new System.Threading.CancellationTokenSource(10000);


                Console.WriteLine("Discovering devices...");
                var device = await discoverer.DiscoverDeviceAsync(PortMapper.Upnp, cts);
                


                var ip = await device.GetExternalIPAsync();


                Console.WriteLine($"Your external IP address is: {ip}");
              
                
                var mapping = new Mapping(Protocol.Tcp, 81, 80, "Port forwarding example");

                Console.WriteLine("Creating mapping...");
                await device.CreatePortMapAsync(mapping);

                Console.WriteLine("Port forwarded: " + mapping);
            }
            catch (NatDeviceNotFoundException)
            {
                Console.WriteLine("NAT device not found. Please ensure that UPnP is enabled on your router.");
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("Discovery timed out. Try increasing the timeout value.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to forward port: " + ex.Message);
            }

            Console.ReadLine();
        }

        static async Task<string> GetPublicIpAddressAsync()
        {
            using (var httpClient = new HttpClient())
            {
                try
                {
                    // Use a public IP service to get your external IP
                    var response = await httpClient.GetStringAsync("https://api.ipify.org?format=text");
                    return response.Trim();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to get public IP address: " + ex.Message);
                    return "Unknown IP";
                }
            }
        }
    }

}
