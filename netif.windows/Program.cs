using AnsiColoredWriters;
using Ip4Parsers;
using Ip4RangeSetDifferenceCalculator;
using NativeMethods.Windows;
using routes;
using System.Net;
using System.Net.NetworkInformation;

namespace ifroute;

internal static class Program
{
    private static void PrintAllInterfaces()
    {
        var table = new Lazy<Ip4RouteEntry[]>(Ip4RouteTable.GetRouteTable);

        var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces()
            .Where(x => x.IsIpv4())
            .OrderBy(x => x.GetInterfaceIndex())
            .ToList();

        Console.WriteLine("{0, 30} {1, 14} {2, 18}", "Name", "InterfaceIndex", "PrimaryGateway");
        foreach (var networkInterface in networkInterfaces)
        {
            Console.WriteLine("{0, 30} {1, 14} {2, 18}", new string(networkInterface.Name.Take(30).ToArray()), networkInterface.GetInterfaceIndex(), networkInterface.GetPrimaryGateway(() => table.Value));
        }
        Console.WriteLine();
    }

    private static void PrintAllRoutes()
    {
        Ip4RouteEntry[] routeTable = Ip4RouteTable.GetRouteTable();

        Console.WriteLine("{0,18} {1,18} {2,18} {3,5} {4,6} ", "DestinationIP", "NetMask", "Gateway", "IF", "Metric");
        foreach (Ip4RouteEntry entry in routeTable)
        {
            Console.WriteLine("{0,18} {1,18} {2,18} {3,5} {4,6} ", entry.DestinationIP, entry.SubnetMask, entry.GatewayIP, entry.InterfaceIndex, entry.Metric);
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

        Console.WriteLine("{0,18} {1,18} {2,18} {3,5} {4,6} ", "DestinationIP", "NetMask", "Gateway", "IF", "Metric");
        foreach (Ip4RouteEntry entry in routeTable.Where(x => x.InterfaceIndex == interfaceIndex))
        {
            Console.WriteLine("{0,18} {1,18} {2,18} {3,5} {4,6} ", entry.DestinationIP, entry.SubnetMask, entry.GatewayIP, entry.InterfaceIndex, entry.Metric);
        }
        Console.WriteLine();
    }

    private static void ChangeRoutes(Ip4RangeSet targetRangeSet, string interfaceName, int metric, Action<string?> successWriteLine, Action<string?> errorWriteLine)
    {
        var networkInterface = NetworkInterface.GetAllNetworkInterfaces()
            .Where(x => x.Name == interfaceName)
            .Single();

        int interfaceIndex = networkInterface.GetInterfaceIndex();

        var table = new Lazy<Ip4RouteEntry[]>(Ip4RouteTable.GetRouteTable);

        IPAddress gatewayIp = networkInterface.GetPrimaryGateway(() => table.Value) ?? throw new InvalidOperationException("PrimaryGateway is null");

        var currentRoutes = Ip4RouteTable.GetRouteTable()
            .Where(x => x.InterfaceIndex == interfaceIndex)
            .Where(x => x.Metric == metric)
            .Select(x => new RouteWithMetricDto(new RouteWithoutMetricDto(x.DestinationIP, x.SubnetMask, x.GatewayIP), x.Metric))
            .ToArray();

        var targetRoutes = targetRangeSet.ToIp4Subnets()
            .Select(x => new RouteWithMetricDto(new RouteWithoutMetricDto(x.FirstAddress, x.Mask, gatewayIp), metric))
            .ToArray();

        List<RouteWithMetricDto> routesToRemove = new();
        List<RouteWithMetricDto> routesToAdd = new();
        List<ChangeMetricRouteDto> routesToChangeMetric = new();
        List<RouteWithMetricDto> routesUnchanged = new();

        RoutesDifferenceCalculator.CalculateDifference(
            source: currentRoutes,
            target: targetRoutes,
            toAdd: routesToAdd.Add,
            toRemove: routesToRemove.Add,
            toChangeMetric: routesToChangeMetric.Add,
            toUnchanged: routesUnchanged.Add
        );

        foreach (var route in routesToChangeMetric)
        {
            try
            {
                Ip4RouteTable.ChangeMetric(new Ip4RouteChangeMetricDto
                {
                    DestinationIP = route.RouteWithoutMetric.DestinationIP,
                    SubnetMask = route.RouteWithoutMetric.SubnetMask,
                    InterfaceIndex = interfaceIndex,
                    GatewayIP = route.RouteWithoutMetric.GatewayIP,
                    Metric = route.NewMetric,
                });
                successWriteLine($"route changed: {route.RouteWithoutMetric.DestinationIP} mask {route.RouteWithoutMetric.SubnetMask} gateway {route.RouteWithoutMetric.GatewayIP} metric {route.OldMetric} => {route.NewMetric}");
            }
            catch (InvalidOperationException exception)
            {
                errorWriteLine($"error changing route {route.RouteWithoutMetric.DestinationIP} mask {route.RouteWithoutMetric.SubnetMask} gateway {route.RouteWithoutMetric.GatewayIP}: {exception.GetBaseException().Message}");
            }
        }

        foreach (var route in routesToRemove)
        {
            try
            {
                Ip4RouteTable.DeleteRoute(new Ip4RouteDeleteDto
                {
                    DestinationIP = route.RouteWithoutMetric.DestinationIP,
                    SubnetMask = route.RouteWithoutMetric.SubnetMask,
                    InterfaceIndex = interfaceIndex,
                    GatewayIP = route.RouteWithoutMetric.GatewayIP,
                });
                successWriteLine($"route deleted: {route.RouteWithoutMetric.DestinationIP} mask {route.RouteWithoutMetric.SubnetMask} gateway {route.RouteWithoutMetric.GatewayIP}");
            }
            catch (InvalidOperationException exception)
            {
                errorWriteLine($"error deleting route {route.RouteWithoutMetric.DestinationIP} mask {route.RouteWithoutMetric.SubnetMask} gateway {route.RouteWithoutMetric.GatewayIP}: {exception.GetBaseException().Message}");
            }
        }

        foreach (var route in routesToAdd)
        {
            try
            {
                Ip4RouteTable.CreateRoute(new Ip4RouteCreateDto
                {
                    DestinationIP = route.RouteWithoutMetric.DestinationIP,
                    SubnetMask = route.RouteWithoutMetric.SubnetMask,
                    InterfaceIndex = interfaceIndex,
                    GatewayIP = route.RouteWithoutMetric.GatewayIP,
                    Metric = route.Metric,
                });
                successWriteLine($"route created: {route.RouteWithoutMetric.DestinationIP} mask {route.RouteWithoutMetric.SubnetMask} gateway {route.RouteWithoutMetric.GatewayIP} metric {route.Metric}");
            }
            catch (InvalidOperationException exception)
            {
                errorWriteLine($"error creating route {route.RouteWithoutMetric.DestinationIP} mask {route.RouteWithoutMetric.SubnetMask} gateway {route.RouteWithoutMetric.GatewayIP}: {exception.GetBaseException().Message}");
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
                  ifroute set [interface-name] [metric] < some-ips.txt
                  ifroute print-all-interfaces
                  ifroute print-all-routes
                  ifroute print-routes-with-interface-name [interface-name]
                """);
        }
    }
}