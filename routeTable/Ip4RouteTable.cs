using System.Net;
using System.Runtime.InteropServices;

namespace NativeMethods.Windows;

public static class Ip4RouteTable
{
    private static int getSize()
    {
        int size = 0;
        _ = NativeMethods.GetIpForwardTable(nint.Zero, ref size, true);
        return size;
    }

    /// <exception cref="InvalidOperationException"></exception>
    public static Ip4RouteEntry[] GetRouteTable()
    {
        int size = getSize();
        nint fwdTable = Marshal.AllocHGlobal(size);
        try
        {
            int status = NativeMethods.GetIpForwardTable(fwdTable, ref size, true);
            if (status != 0)
            {
                throw new InvalidOperationException($"NativeMethods.GetIpForwardTable returns {status}");
            }
            NativeMethods.IPForwardTable forwardTable = NativeMethods.ReadIPForwardTable(fwdTable);
            return forwardTable.Table.Select(row => new Ip4RouteEntry
            {
                DestinationIP = new IPAddress(row.dwForwardDest),
                SubnetMask = new IPAddress(row.dwForwardMask),
                GatewayIP = new IPAddress(row.dwForwardNextHop),
                InterfaceIndex = Convert.ToInt32(row.dwForwardIfIndex),
                ForwardType = Convert.ToInt32(row.dwForwardType),
                ForwardProtocol = Convert.ToInt32(row.dwForwardProto),
                ForwardAge = Convert.ToInt32(row.dwForwardAge),
                Metric = Convert.ToInt32(row.dwForwardMetric1),
            }).ToArray();
        }
        finally
        {
            Marshal.FreeHGlobal(fwdTable);
        }
    }

    /// <exception cref="InvalidOperationException"></exception>
    public static void CreateRoute(Ip4RouteCreateDto routeEntry)
    {
        ArgumentNullException.ThrowIfNull(routeEntry);
        var route = new NativeMethods.MIB_IPFORWARDROW
        {
            dwForwardDest = BitConverter.ToUInt32(routeEntry.DestinationIP.GetAddressBytes()),
            dwForwardMask = BitConverter.ToUInt32(routeEntry.SubnetMask.GetAddressBytes()),
            dwForwardNextHop = BitConverter.ToUInt32(routeEntry.GatewayIP.GetAddressBytes()),
            dwForwardMetric1 = Convert.ToUInt32(routeEntry.Metric),
            dwForwardType = Convert.ToUInt32(3), //Default to 3
            dwForwardProto = Convert.ToUInt32(3), //Default to 3
            dwForwardAge = 0,
            dwForwardIfIndex = Convert.ToUInt32(routeEntry.InterfaceIndex)
        };
        nint ptr = Marshal.AllocHGlobal(Marshal.SizeOf<NativeMethods.MIB_IPFORWARDROW>());
        try
        {
            Marshal.StructureToPtr(route, ptr, false);

            int status = NativeMethods.CreateIpForwardEntry(ptr);
            if (status != 0)
            {
                throw new InvalidOperationException($"NativeMethods.CreateIpForwardEntry returns {status}");
            }
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
    }

    /// <exception cref="InvalidOperationException"></exception>
    public static void DeleteRoute(Ip4RouteDeleteDto routeEntry)
    {
        ArgumentNullException.ThrowIfNull(routeEntry);
        var route = new NativeMethods.MIB_IPFORWARDROW
        {
            dwForwardDest = BitConverter.ToUInt32(routeEntry.DestinationIP.GetAddressBytes()),
            dwForwardMask = BitConverter.ToUInt32(routeEntry.SubnetMask.GetAddressBytes()),
            dwForwardNextHop = BitConverter.ToUInt32(routeEntry.GatewayIP.GetAddressBytes()),
            dwForwardMetric1 = 99,
            dwForwardType = Convert.ToUInt32(3), //Default to 3
            dwForwardProto = Convert.ToUInt32(3), //Default to 3
            dwForwardAge = 0,
            dwForwardIfIndex = Convert.ToUInt32(routeEntry.InterfaceIndex)
        };
        nint ptr = Marshal.AllocHGlobal(Marshal.SizeOf<NativeMethods.MIB_IPFORWARDROW>());
        try
        {
            Marshal.StructureToPtr(route, ptr, false);

            int status = NativeMethods.DeleteIpForwardEntry(ptr);
            if (status != 0)
            {
                throw new InvalidOperationException($"NativeMethods.DeleteIpForwardEntry returns {status}");
            }
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
    }
}