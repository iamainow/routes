using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.Versioning;

namespace routes
{
    public static class NetworkInterfaceExtensions
    {
        /// <exception cref="NetworkInformationException"></exception>
        public static bool IsIpv4(this NetworkInterface networkInterface)
        {
            return networkInterface.GetIPProperties().GetIPv4Properties() is not null;
        }

        /// <exception cref="NetworkInformationException"></exception>
        public static int GetInterfaceIndex(this NetworkInterface networkInterface)
        {
            return networkInterface.GetIPProperties().GetIPv4Properties().Index;
        }

        private static IPAddress? GetPrimaryGatewayViaGatewayAddresses(IPInterfaceProperties properties)
        {
            return properties.GatewayAddresses
                .Where(gatewayInfo => gatewayInfo.Address != null)
                .Where(gatewayInfo => gatewayInfo.Address.AddressFamily == AddressFamily.InterNetwork)
                .Select(x => x.Address)
                .FirstOrDefault();
        }

        private static IPAddress? GetPrimaryGatewayViaRouteTable(int interfaceIndex)
        {
            return Ip4RouteTable.GetRouteTable()
                .Where(i => i.InterfaceIndex == interfaceIndex)
                .Select(x => x.GatewayIP)
                .FirstOrDefault();
        }

        [UnsupportedOSPlatform("macOS")]
        [UnsupportedOSPlatform("OSX")]
        private static IPAddress? GetPrimaryGatewayViaDhcpServerAddresses(IPInterfaceProperties properties)
        {
            return properties.DhcpServerAddresses.FirstOrDefault();
        }

        /// <exception cref="NetworkInformationException"></exception>
        [UnsupportedOSPlatform("macOS")]
        [UnsupportedOSPlatform("OSX")]
        public static IPAddress? GetPrimaryGateway(this NetworkInterface networkInterface)
        {
            return GetPrimaryGatewayViaGatewayAddresses(networkInterface.GetIPProperties())
                ?? GetPrimaryGatewayViaRouteTable(networkInterface.GetInterfaceIndex())
                ?? GetPrimaryGatewayViaDhcpServerAddresses(networkInterface.GetIPProperties());
        }
    }
}
