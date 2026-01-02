using NativeMethods.Windows;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace ifroute;

internal static class NetworkInterfaceExtensions
{
    /// <exception cref="NetworkInformationException"></exception>
    public static bool IsIpv4(this NetworkInterface networkInterface)
    {
        ArgumentNullException.ThrowIfNull(networkInterface);
        try
        {
            return networkInterface.GetIPProperties().GetIPv4Properties() is not null;
        }
        catch (NetworkInformationException)
        {
            return false;
        }
    }

    /// <exception cref="NetworkInformationException"></exception>
    public static int GetInterfaceIndex(this NetworkInterface networkInterface)
    {
        ArgumentNullException.ThrowIfNull(networkInterface);
        try
        {
            return networkInterface.GetIPProperties().GetIPv4Properties().Index;
        }
        catch (NetworkInformationException)
        {
            return -1;
        }
    }

    private static IPAddress? GetPrimaryGatewayViaGatewayAddresses(IPInterfaceProperties properties)
    {
        return properties.GatewayAddresses
            .Where(gatewayInfo => gatewayInfo.Address != null)
            .Where(gatewayInfo => gatewayInfo.Address.AddressFamily == AddressFamily.InterNetwork)
            .Select(x => x.Address)
            .FirstOrDefault();
    }

    private static IPAddress? GetPrimaryGatewayViaRouteTable(IEnumerable<Ip4RouteEntry> table, int interfaceIndex)
    {
        return table
            .Where(i => i.InterfaceIndex == interfaceIndex)
            .Where(i => i.GatewayIP is not null)
            .Select(x => x.GatewayIP)
            .FirstOrDefault();
    }

    private static IPAddress? GetPrimaryGatewayViaDhcpServerAddresses(IPInterfaceProperties properties)
    {
        return properties.DhcpServerAddresses.FirstOrDefault();
    }

    /// <exception cref="NetworkInformationException"></exception>
    public static IPAddress? GetPrimaryGateway(this NetworkInterface networkInterface, Func<IEnumerable<Ip4RouteEntry>> tableFunc)
    {
        ArgumentNullException.ThrowIfNull(networkInterface);
        ArgumentNullException.ThrowIfNull(tableFunc);
        return GetPrimaryGatewayViaGatewayAddresses(networkInterface.GetIPProperties())
            ?? GetPrimaryGatewayViaRouteTable(tableFunc(), networkInterface.GetInterfaceIndex())
            ?? GetPrimaryGatewayViaDhcpServerAddresses(networkInterface.GetIPProperties());
    }
}