using AnsiColoredWriters;
using Ip4Parsers;
using NativeMethods.Windows;
using routes;
using System.Net;
using System.Net.NetworkInformation;

namespace netif.windows;

internal static class Program
{
    private static void PrintAllInterfaces()
    {
        var table = new Lazy<Ip4RouteEntry[]>(Ip4RouteTable.GetRouteTable);

        var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces()
            .Where(x => x.IsIpv4())
            .OrderBy(x => x.GetInterfaceIndex())
            .ToList();

        Console.WriteLine("{0, 40} {1, 20} {2, 20}", "Name", "InterfaceIndex", "PrimaryGateway");
        foreach (var networkInterface in networkInterfaces)
        {
            Console.WriteLine("{0, 40} {1, 20} {2, 20}", networkInterface.Name, networkInterface.GetInterfaceIndex(), networkInterface.GetPrimaryGateway(() => table.Value));
        }
        Console.WriteLine();
    }

    private static void PrintAllRoutes()
    {
        Ip4RouteEntry[] routeTable = Ip4RouteTable.GetRouteTable();

        Console.WriteLine("{0,18} {1,18} {2,18} {3,5} {4,8} ", "DestinationIP", "NetMask", "Gateway", "IF", "Metric");
        foreach (Ip4RouteEntry entry in routeTable)
        {
            Console.WriteLine("{0,18} {1,18} {2,18} {3,5} {4,8} ", entry.DestinationIP, entry.SubnetMask, entry.GatewayIP, entry.InterfaceIndex, entry.Metric);
        }
        Console.WriteLine();
    }

    private static void PrintRoutesWithInterfaceName(string name)
    {
        var networkInterface = NetworkInterface.GetAllNetworkInterfaces()
            .Where(x => x.Name == name)
            .First();

        int interfaceIndex = networkInterface.GetInterfaceIndex();

        PrintRoutesWithInterfaceIndex(interfaceIndex);
    }

    private static void PrintRoutesWithInterfaceIndex(int interfaceIndex)
    {
        Ip4RouteEntry[] routeTable = Ip4RouteTable.GetRouteTable();

        Console.WriteLine("{0,18} {1,18} {2,18} {3,5} {4,8} ", "DestinationIP", "NetMask", "Gateway", "IF", "Metric");
        foreach (Ip4RouteEntry entry in routeTable.Where(x => x.InterfaceIndex == interfaceIndex))
        {
            Console.WriteLine("{0,18} {1,18} {2,18} {3,5} {4,8} ", entry.DestinationIP, entry.SubnetMask, entry.GatewayIP, entry.InterfaceIndex, entry.Metric);
        }
        Console.WriteLine();
    }

    private static void ChangeRoutes(Ip4RangeSet nonRuIps, string interfaceName, int metric, Action<string?> successWriteLine, Action<string?> errorWriteLine)
    {
        var networkInterface = NetworkInterface.GetAllNetworkInterfaces()
            .Where(x => x.Name == interfaceName)
            .Single();

        int interfaceIndex = networkInterface.GetInterfaceIndex();

        var table = new Lazy<Ip4RouteEntry[]>(Ip4RouteTable.GetRouteTable);

        IPAddress gatewayIp = networkInterface.GetPrimaryGateway(() => table.Value) ?? throw new InvalidOperationException("PrimaryGateway is null");

        var routesToRemove = Ip4RouteTable.GetRouteTable()
            .Where(x => x.InterfaceIndex == interfaceIndex)
            .Where(x => x.Metric == metric)
            .ToArray();

        foreach (var routeToRemove in routesToRemove)
        {
            var subnet = new Ip4Subnet(routeToRemove.DestinationIP, routeToRemove.SubnetMask);
            try
            {
                Ip4RouteTable.DeleteRoute(new Ip4RouteDeleteDto
                {
                    DestinationIP = routeToRemove.DestinationIP,
                    SubnetMask = routeToRemove.SubnetMask,
                    InterfaceIndex = routeToRemove.InterfaceIndex,
                    GatewayIP = routeToRemove.GatewayIP,
                });
                successWriteLine($"route deleted: {subnet}");
            }
            catch (InvalidOperationException exception)
            {
                errorWriteLine($"error deleting route {subnet}: {exception.GetBaseException().Message}");
            }
        }

        foreach (var subnet in nonRuIps.ToIp4Subnets())
        {
            try
            {
                Ip4RouteTable.CreateRoute(new Ip4RouteCreateDto
                {
                    DestinationIP = subnet.FirstAddress,
                    SubnetMask = subnet.Mask,
                    InterfaceIndex = interfaceIndex,
                    GatewayIP = gatewayIp,
                    Metric = metric,
                });
                successWriteLine($"route created: {subnet}");
            }
            catch (InvalidOperationException exception)
            {
                errorWriteLine($"error creating route {subnet}: {exception.GetBaseException().Message}");
            }
        }
    }

    public static void Main(string[] args)
    {
        if (args.Length == 3 && args[0] == "set")
        {
            string interfaceName = args[1];
            int metric = int.Parse(args[2]);
            Action<string?> errorWriteLine = Console.IsErrorRedirected ? Console.Error.WriteLine : new AnsiColoredWriter(Console.Error, AnsiColor.Red).WriteLine;
            string? line;
            Ip4RangeSet ip4RangeSet = new();
            while ((line = Console.ReadLine()) != null)
            {
                var ranges = Ip4SubnetParser.GetRanges(line, errorWriteLine);
                Ip4RangeSet rangesSet = new(ranges);

                ip4RangeSet = ip4RangeSet.Union(rangesSet);
            }

            ChangeRoutes(ip4RangeSet, interfaceName, metric, Console.WriteLine, errorWriteLine);
        }
        else if (args.Length == 1 && args[0] == "print-all-interfaces")
        {
            PrintAllInterfaces();
        }
        else if (args.Length == 1 && args[0] == "print-all-routes")
        {
            PrintAllRoutes();
        }
        else if (args.Length == 2 && args[0] == "print-routes-with-interface-name")
        {
            string interfaceName = args[1];
            PrintRoutesWithInterfaceName(interfaceName);
        }
        else
        {
            Console.Write("""
                usage:
                  netif.windows set [interface-name] [metric] < some-ips.txt
                  netif.windows print-all-interfaces
                  netif.windows print-all-routes
                  netif.windows print-routes-with-interface-name [interface-name]
                """);
        }
    }
}