using routes;
using System.Net;
using System.Net.NetworkInformation;

AdaptersPrint();

RoutePrint();

var networkInterface = NetworkInterface.GetAllNetworkInterfaces()
    .Where(x => x.Name.Contains("ForcePoint", StringComparison.OrdinalIgnoreCase))
    .First();

List<Ip4RouteEntry> routeTable = Ip4RouteTable.GetRouteTable();

var exampleRoute = new Ip4RouteCreateDto
{
    //string destination, string mask, int interfaceIndex, int metric
    DestinationIP = IPAddress.Parse("10.158.8.10"),
    SubnetMask = IPAddress.Parse("255.255.255.255"),
    Metric = 2,
    InterfaceIndex = networkInterface.GetInterfaceIndex(),
    GatewayIP = networkInterface.GetPrimaryGateway() ?? throw new Exception("PrimaryGateway is null"),
};

var route = routeTable
    .Where(x => x.DestinationIP.Equals(exampleRoute.DestinationIP))
    .Where(x => x.SubnetMask.Equals(exampleRoute.SubnetMask))
    .Where(x => x.InterfaceIndex.Equals(exampleRoute.InterfaceIndex))
    .Where(x => x.GatewayIP.Equals(exampleRoute.GatewayIP))
    .FirstOrDefault();

if (route is not null)
{
    Ip4RouteTable.DeleteRoute(new Ip4RouteDeleteDto
    {
        DestinationIP = exampleRoute.DestinationIP,
        SubnetMask = exampleRoute.SubnetMask,
        InterfaceIndex = exampleRoute.InterfaceIndex,
        GatewayIP = exampleRoute.GatewayIP,
    });
    Console.WriteLine("route deleted");
}

Ip4RouteTable.CreateRoute(exampleRoute);
Console.WriteLine("route created");

static void AdaptersPrint()
{
    var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces()
        .Where(x => x.IsIpv4())
        .OrderBy(x => x.GetInterfaceIndex())
        .ToList();

    Console.WriteLine("{0, 40} {1, 20} {2, 20}", "Name", "InterfaceIndex", "PrimaryGateway");
    foreach (var networkInterface in networkInterfaces)
    {
        Console.WriteLine("{0, 40} {1, 20} {2, 20}", networkInterface.Name, networkInterface.GetInterfaceIndex(), networkInterface.GetPrimaryGateway());
    }
    Console.WriteLine();
}

static void RoutePrint()
{
    List<Ip4RouteEntry> routeTable = Ip4RouteTable.GetRouteTable();

    Console.WriteLine("{0,18} {1,18} {2,18} {3,5} {4,8} ", "DestinationIP", "NetMask", "Gateway", "IF", "Metric");
    foreach (Ip4RouteEntry entry in routeTable)
    {
        Console.WriteLine("{0,18} {1,18} {2,18} {3,5} {4,8} ", entry.DestinationIP, entry.SubnetMask, entry.GatewayIP, entry.InterfaceIndex, entry.Metric);
    }
    Console.WriteLine();
}

Console.ReadKey();