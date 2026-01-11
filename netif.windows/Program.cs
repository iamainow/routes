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
        Lazy<Ip4RouteEntry[]> table = new(Ip4RouteTable.GetRouteTable);

        var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces()
            .Where(x => x.IsIpv4())
            .OrderBy(x => x.GetInterfaceIndex())
            .ToList();

        if (networkInterfaces.Count == 0)
        {
            Console.WriteLine("No IPv4 network interfaces found.");
            return;
        }

        const string nameHeader = "Name";
        const string indexHeader = "InterfaceIndex";
        const string gatewayHeader = "PrimaryGateway";

        // Pre-compute data to calculate column widths
        var rows = networkInterfaces.Select(ni => new
        {
            Name = ni.Name,
            Index = ni.GetInterfaceIndex().ToString(),
            Gateway = ni.GetPrimaryGateway(() => table.Value)?.ToString() ?? ""
        }).ToList();

        int nameWidth = Math.Max(nameHeader.Length, rows.Max(r => r.Name.Length));
        int indexWidth = Math.Max(indexHeader.Length, rows.Max(r => r.Index.Length));
        int gatewayWidth = Math.Max(gatewayHeader.Length, rows.Max(r => r.Gateway.Length));

        string format = $"{{0,{nameWidth}}} {{1,{indexWidth}}} {{2,{gatewayWidth}}}";

        Console.WriteLine(format, nameHeader, indexHeader, gatewayHeader);
        foreach (var row in rows)
        {
            Console.WriteLine(format, row.Name, row.Index, row.Gateway);
        }
        Console.WriteLine();
    }

    private static void PrintRouteTable(IEnumerable<Ip4RouteEntry> routeTable)
    {
        const string subnetHeader = "Subnet";
        const string gatewayHeader = "Gateway";
        const string metricHeader = "Metric";

        // Pre-compute data to calculate column widths
        var rows = routeTable.Select(entry => new
        {
            Subnet = new Ip4Subnet(entry.DestinationIP, entry.SubnetMask).ToString(),
            Gateway = entry.GatewayIP.ToString(),
            Metric = entry.Metric.ToString()
        }).ToList();

        if (rows.Count == 0)
        {
            Console.WriteLine("No routes found.");
            return;
        }

        int subnetWidth = Math.Max(subnetHeader.Length, rows.Max(r => r.Subnet.Length));
        int gatewayWidth = Math.Max(gatewayHeader.Length, rows.Max(r => r.Gateway.Length));
        int metricWidth = Math.Max(metricHeader.Length, rows.Max(r => r.Metric.Length));

        string format = $"{{0,{subnetWidth}}} {{1,{gatewayWidth}}} {{2,{metricWidth}}}";

        Console.WriteLine(format, subnetHeader, gatewayHeader, metricHeader);
        foreach (var row in rows)
        {
            Console.WriteLine(format, row.Subnet, row.Gateway, row.Metric);
        }
        Console.WriteLine();
    }

    private static void PrintAllRoutes()
    {
        PrintRouteTable(Ip4RouteTable.GetRouteTable());
    }

    private static void PrintRoutesWithInterfaceName(string name)
    {
        NetworkInterface networkInterface = NetworkInterface.GetAllNetworkInterfaces()
            .SingleOrDefault(x => x.Name == name)
            ?? throw new InvalidOperationException($"Network interface '{name}' not found");

        int interfaceIndex = networkInterface.GetInterfaceIndex();

        PrintRoutesWithInterfaceIndex(interfaceIndex);
    }

    private static void PrintRoutesWithInterfaceNameAndMetric(string name, int metric)
    {
        NetworkInterface networkInterface = NetworkInterface.GetAllNetworkInterfaces()
            .SingleOrDefault(x => x.Name == name)
            ?? throw new InvalidOperationException($"Network interface '{name}' not found");

        int interfaceIndex = networkInterface.GetInterfaceIndex();

        var routeTable = Ip4RouteTable.GetRouteTable()
            .Where(x => x.InterfaceIndex == interfaceIndex && x.Metric == metric);

        PrintRouteTable(routeTable);
    }

    private static void PrintRoutesWithInterfaceIndex(int interfaceIndex)
    {
        var routeTable = Ip4RouteTable.GetRouteTable()
            .Where(x => x.InterfaceIndex == interfaceIndex);

        PrintRouteTable(routeTable);
    }

    private static void ChangeRoutes(Ip4RangeArray targetRangeSet, string interfaceName, int metric, Action<string?> successWriteLine, Action<string?> errorWriteLine)
    {
        NetworkInterface networkInterface = NetworkInterface.GetAllNetworkInterfaces()
            .SingleOrDefault(x => x.Name == interfaceName)
            ?? throw new InvalidOperationException($"Network interface '{interfaceName}' not found");

        int interfaceIndex = networkInterface.GetInterfaceIndex();

        Lazy<Ip4RouteEntry[]> table = new(Ip4RouteTable.GetRouteTable);

        IPAddress gatewayIp = networkInterface.GetPrimaryGateway(() => table.Value) ?? throw new InvalidOperationException("PrimaryGateway is null");

        RouteWithMetricDto[] currentRoutes = table.Value
            .Where(x => x.InterfaceIndex == interfaceIndex)
            .Where(x => x.Metric == metric)
            .Select(x => new RouteWithMetricDto(new RouteWithoutMetricDto(x.DestinationIP, x.SubnetMask, x.GatewayIP), x.Metric))
            .ToArray();

        RouteWithMetricDto[] targetRoutes = targetRangeSet.ToIp4Subnets()
            .Select(x => new RouteWithMetricDto(new RouteWithoutMetricDto(x.FirstAddress, x.Mask, gatewayIp), metric))
            .ToArray();

        List<RouteWithMetricDto> routesToRemove = [];
        List<RouteWithMetricDto> routesToAdd = [];
        List<ChangeMetricRouteDto> routesToChangeMetric = [];
        List<RouteWithMetricDto> routesUnchanged = [];

        RoutesDifferenceCalculator.CalculateDifference(
            source: currentRoutes,
            target: targetRoutes,
            toAdd: routesToAdd.Add,
            toRemove: routesToRemove.Add,
            toChangeMetric: routesToChangeMetric.Add,
            toUnchanged: routesUnchanged.Add
        );

        foreach (ChangeMetricRouteDto route in routesToChangeMetric)
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

        foreach (RouteWithMetricDto route in routesToRemove)
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

        foreach (RouteWithMetricDto route in routesToAdd)
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

    private static void PrintUsage()
    {
        Console.WriteLine("""
            usage:
              ifroute set <interface-name> <metric> < some-ips.txt
              ifroute print-all-interfaces
              ifroute print-all-routes
              ifroute print-routes-with-interface-name <interface-name>
              ifroute print-routes-with-interface-name-and-metric <interface-name> <metric>
            """);
    }

    public static int Main(string[] args)
    {
        try
        {
            if (args.Length == 3 && args[0] == "set")
            {
                string interfaceName = args[1];
                if (!int.TryParse(args[2], out int metric))
                {
                    Console.Error.WriteLine($"error: invalid metric value '{args[2]}'");
                    return 1;
                }
                ITextWriterWrapper errorTextWriterWrapper = Console.IsErrorRedirected ? new TextWriterWrapper(Console.Error) : new AnsiColoredTextWriterWrapper(Console.Error, AnsiColor.Red);
                string? line;
                Ip4RangeArray ip4RangeSet = new();
                while ((line = Console.ReadLine()) != null)
                {
                    Span<Ip4Range> ranges = Ip4SubnetParser.GetRanges(line);
                    ip4RangeSet = ip4RangeSet.Union(ranges);
                }

                ChangeRoutes(ip4RangeSet, interfaceName, metric, Console.WriteLine, errorTextWriterWrapper.WriteLine);
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
            else if (args.Length == 3 && args[0] == "print-routes-with-interface-name-and-metric")
            {
                string interfaceName = args[1];
                if (!int.TryParse(args[2], out int metric))
                {
                    Console.Error.WriteLine($"error: invalid metric value '{args[2]}'");
                    return 1;
                }
                PrintRoutesWithInterfaceNameAndMetric(interfaceName, metric);
            }
            else
            {
                PrintUsage();
                return args.Length == 0 ? 0 : 1;
            }
            return 0;
        }
        catch (InvalidOperationException ex)
        {
            Console.Error.WriteLine($"error: {ex.Message}");
            return 1;
        }
    }
}