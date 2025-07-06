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
List<Ip4Subnet> ruSubnets = new List<Ip4Subnet>();
Ip4RangeSet ruRangeSet = new Ip4RangeSet();
foreach (string line in lines)
{
    if (!string.IsNullOrEmpty(line))
    {
        string[] stringArray = line.Split('/');
        var ip = Ip4Address.Parse(stringArray[0]);
        var mask = new Ip4Mask(int.Parse(stringArray[1]));

        var subnet = new Ip4Subnet(ip, mask);
        ruSubnets.Add(subnet);
        ruRangeSet = ruRangeSet.Union(subnet.ToIp4Range());

        //Console.WriteLine($"{line,18} => {subnet.FirstAddress,15} {subnet.Mask.ToFullString(),15} or {subnet.FirstAddress,15}-{subnet.LastAddress,15}");
    }
}

Console.WriteLine("------------------------------");
Console.ReadKey();

var allIpRangeSet = new Ip4RangeSet().Union(new Ip4Range(new Ip4Address(0x00000000), new Ip4Address(0xFFFFFFFF)));
var nonRuIpRangeSet = allIpRangeSet.Except(ruRangeSet);

//foreach (var item in nonRuIpRangeSet)
//{
//    Console.WriteLine($"{item.FirstAddress,15} - {item.LastAddress,15} {item.Count}");
//}

//Console.WriteLine(nonRuIpRangeSet);
Console.WriteLine("------------------------------");
Console.ReadKey();

const uint delta = 10000;

LinkedList<Ip4Range> ips = new LinkedList<Ip4Range>(nonRuIpRangeSet);

bool shouldShrink = false;
do
{
    shouldShrink = false;
    LinkedListNode<Ip4Range>? current = ips.First;
    while (current is not null && current.Next is not null)
    {
        var next = current.Next;
        if ((uint)current.Value.LastAddress + delta >= (uint)next.Value.FirstAddress)
        {
            current.Value = new Ip4Range(current.Value.FirstAddress, next.Value.LastAddress);
            ips.Remove(next);
            shouldShrink = true;
        }
        else
        {
            current = current.Next;
        }
    }

    current = ips.First;
    while (current is not null)
    {
        if (current.Value.Count <= delta)
        {
            var prevCurrent = current;
            current = current.Next;
            ips.Remove(prevCurrent);
            shouldShrink = true;
        }
        else
        {
            current = current.Next;
        }
    }
} while (shouldShrink);

var lessNonRuIpRangeSet = new Ip4RangeSet();
foreach (var item in ips)
{
    lessNonRuIpRangeSet = lessNonRuIpRangeSet.Union(item);
}

foreach (var item in lessNonRuIpRangeSet)
{
    Console.WriteLine($"{item.FirstAddress,15} - {item.LastAddress,15} {item.Count}");
}

foreach (var item in lessNonRuIpRangeSet)
{
    foreach (var item2 in item.ToSubnets())
    {
        Console.WriteLine(item2);
    }
}