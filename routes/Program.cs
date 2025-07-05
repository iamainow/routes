using routes;
using routes.core;
using System.Net;
using System.Net.NetworkInformation;

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

AdaptersPrint();

RoutePrint();

var networkInterface = NetworkInterface.GetAllNetworkInterfaces()
    .Where(x => x.Name.Contains("AmneziaVPN", StringComparison.OrdinalIgnoreCase))
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
try
{
    Ip4RouteTable.CreateRoute(exampleRoute);
    Console.WriteLine("route created");
}
catch (Exception exception)
{
    Console.WriteLine(exception.ToString());
}



Console.WriteLine("------------------------------");
Console.ReadKey();

var lines = await File.ReadAllLinesAsync("ru.txt");
foreach (string line in lines)
{
    if (!string.IsNullOrEmpty(line))
    {
        string[] stringArray = line.Split('/');
        var ip = Ip4Address.Parse(stringArray[0]);
        var mask = new Ip4Mask(int.Parse(stringArray[1]));

        var subnet = new Ip4Subnet(ip, mask);

        Console.WriteLine($"{line,18} => {subnet.FirstAddress,15} {subnet.Mask.ToFullString(),15} or {subnet.FirstAddress,15}-{subnet.LastAddress,15}");
    }
}

Console.WriteLine("------------------------------");
Console.ReadKey();