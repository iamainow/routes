using routes;
using routes.core;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.Json;

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

static void RoutePrintAll()
{
    List<Ip4RouteEntry> routeTable = Ip4RouteTable.GetRouteTable();

    Console.WriteLine("{0,18} {1,18} {2,18} {3,5} {4,8} ", "DestinationIP", "NetMask", "Gateway", "IF", "Metric");
    foreach (Ip4RouteEntry entry in routeTable)
    {
        Console.WriteLine("{0,18} {1,18} {2,18} {3,5} {4,8} ", entry.DestinationIP, entry.SubnetMask, entry.GatewayIP, entry.InterfaceIndex, entry.Metric);
    }
    Console.WriteLine();
}

static void RoutePrint(int interfaceIndex)
{
    List<Ip4RouteEntry> routeTable = Ip4RouteTable.GetRouteTable();

    Console.WriteLine("{0,18} {1,18} {2,18} {3,5} {4,8} ", "DestinationIP", "NetMask", "Gateway", "IF", "Metric");
    foreach (Ip4RouteEntry entry in routeTable.Where(x => x.InterfaceIndex == interfaceIndex))
    {
        Console.WriteLine("{0,18} {1,18} {2,18} {3,5} {4,8} ", entry.DestinationIP, entry.SubnetMask, entry.GatewayIP, entry.InterfaceIndex, entry.Metric);
    }
    Console.WriteLine();
}

static Ip4RangeSet Simplify(Ip4RangeSet set, uint delta)
{
    LinkedList<Ip4Range> internalResult = new LinkedList<Ip4Range>(set);

    bool shouldIterate = false;
    do
    {
        shouldIterate = false;
        LinkedListNode<Ip4Range>? current = internalResult.First;
        while (current is not null && current.Next is not null)
        {
            var next = current.Next;
            if ((uint)current.Value.LastAddress + delta >= (uint)next.Value.FirstAddress)
            {
                current.Value = new Ip4Range(current.Value.FirstAddress, next.Value.LastAddress);
                internalResult.Remove(next);
                shouldIterate = true;
            }
            else
            {
                current = current.Next;
            }
        }

        current = internalResult.First;
        while (current is not null)
        {
            if (current.Value.Count <= delta)
            {
                var prevCurrent = current;
                current = current.Next;
                internalResult.Remove(prevCurrent);
                shouldIterate = true;
            }
            else
            {
                current = current.Next;
            }
        }
    } while (shouldIterate);

    Ip4RangeSet result = new();
    foreach (var item in internalResult)
    {
        result = result.Union(item);
    }

    return result;
}

static async Task<Ip4RangeSet> TryGetGoogleIps()
{
    using HttpClient client = new HttpClient();
    var response = await client.GetAsync("https://www.gstatic.com/ipranges/goog.json");
    if (!response.IsSuccessStatusCode)
    {
        return [];
    }
    var responseJson = await response.Content.ReadAsStringAsync();
    var googleIpRanges = JsonSerializer.Deserialize<Root>(responseJson)
        .prefixes
        .Select(x => x.ipv4Prefix)
        .Where(x => !string.IsNullOrEmpty(x))
        .ToArray();

    Ip4RangeSet result = new Ip4RangeSet();

    foreach (var ipRange in googleIpRanges)
    {
        try
        {
            var parts = ipRange.Split('/');
            Ip4Subnet subnet = new Ip4Subnet(Ip4Address.Parse(parts[0]), new Ip4Mask(int.Parse(parts[1])));
            result = result.Union(subnet.ToIp4Range());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing IP range '{ipRange}': {ex.GetBaseException().Message}");
        }
    }

    return result;
}

AdaptersPrint();

var networkInterface = NetworkInterface.GetAllNetworkInterfaces()
    .Where(x => x.Name.Contains("AmneziaVPN", StringComparison.OrdinalIgnoreCase))
    .First();

int networkInterfaceIndex = networkInterface.GetInterfaceIndex();

IPAddress gatewayIp = networkInterface.GetPrimaryGateway() ?? throw new Exception("PrimaryGateway is null");

RoutePrint(networkInterface.GetInterfaceIndex());

List<Ip4RouteEntry> routeTable = Ip4RouteTable.GetRouteTable();

int metric = 5;

Console.WriteLine("------------------------------");
Console.ReadKey();

var lines = await File.ReadAllLinesAsync("ru.txt");
List<Ip4Subnet> ruSubnets = new List<Ip4Subnet>();

foreach (string line in lines)
{
    if (!string.IsNullOrEmpty(line))
    {
        string[] stringArray = line.Split('/');
        var ip = Ip4Address.Parse(stringArray[0]);
        var mask = new Ip4Mask(int.Parse(stringArray[1]));

        var subnet = new Ip4Subnet(ip, mask);
        ruSubnets.Add(subnet);
    }
}

Ip4RangeSet ruIps = new Ip4RangeSet();
foreach (var subnet in ruSubnets)
{
    ruIps = ruIps.Union(subnet.ToIp4Range());
}

var googleIps = await TryGetGoogleIps();

var nonRuIps = new Ip4RangeSet()
    .Union(new Ip4Range(new Ip4Address(0x00000000), new Ip4Address(0xFFFFFFFF)))
    .Except(ruIps)
    .Except(googleIps)
    .Except(new Ip4Subnet(Ip4Address.Parse("10.0.0.0"), 8))
    .Except(new Ip4Subnet(Ip4Address.Parse("100.64.0.0"), 10))
    .Except(new Ip4Subnet(Ip4Address.Parse("127.0.0.0"), 8))
    .Except(new Ip4Subnet(Ip4Address.Parse("169.254.0.0"), 16))
    .Except(new Ip4Subnet(Ip4Address.Parse("172.16.0.0"), 12))
    .Except(new Ip4Subnet(Ip4Address.Parse("192.168.0.0"), 16));

foreach (var item in nonRuIps)
{
    Console.WriteLine($"{item.FirstAddress,15} - {item.LastAddress,15} {item.Count,10} => {string.Join(", ", item.ToSubnets())}");
}

Console.WriteLine("------------------------------");
Console.ReadKey();

try
{
    Ip4RouteTable.DeleteRoute(new Ip4RouteDeleteDto
    {
        DestinationIP = IPAddress.Parse("0.0.0.0"),
        SubnetMask = IPAddress.Parse("0.0.0.0"),
        InterfaceIndex = networkInterfaceIndex,
        GatewayIP = gatewayIp,
    });
    Console.WriteLine("route deleted: 0.0.0.0/0");
}
catch (Exception exception)
{
    Console.WriteLine($"error deleting route 0.0.0.0/0: {exception.GetBaseException().Message}");
}

foreach (var subnet in nonRuIps.SelectMany(x => x.ToSubnets()))
{
    try
    {
        Ip4RouteTable.CreateRoute(new Ip4RouteCreateDto
        {
            DestinationIP = new IPAddress(subnet.FirstAddress.AsByteArray()),
            SubnetMask = new IPAddress(subnet.Mask.AsByteArray()),
            InterfaceIndex = networkInterfaceIndex,
            GatewayIP = gatewayIp,
            Metric = 5,
        });
        Console.WriteLine($"route created: {subnet}");
    }
    catch (Exception exception)
    {
        Console.WriteLine($"error creating route {subnet}: {exception.GetBaseException().Message}");
    }
}

internal record Root(Prefix[] prefixes);

internal record Prefix(string ipv4Prefix);