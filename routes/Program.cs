using routes;
using routes.core;
using routeTable;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.Versioning;
using System.Text.Json;

static void AdaptersPrint()
{
    var table = new Lazy<List<Ip4RouteEntry>>(Ip4RouteTable.GetRouteTable);

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

static void RoutePrintByName(string name)
{
    var networkInterface = NetworkInterface.GetAllNetworkInterfaces()
        .Where(x => x.Name == name)
        .First();

    int interfaceIndex = networkInterface.GetInterfaceIndex();

    RoutePrintByIndex(interfaceIndex);
}

static void RoutePrintByIndex(int interfaceIndex)
{
    List<Ip4RouteEntry> routeTable = Ip4RouteTable.GetRouteTable();

    Console.WriteLine("{0,18} {1,18} {2,18} {3,5} {4,8} ", "DestinationIP", "NetMask", "Gateway", "IF", "Metric");
    foreach (Ip4RouteEntry entry in routeTable.Where(x => x.InterfaceIndex == interfaceIndex))
    {
        Console.WriteLine("{0,18} {1,18} {2,18} {3,5} {4,8} ", entry.DestinationIP, entry.SubnetMask, entry.GatewayIP, entry.InterfaceIndex, entry.Metric);
    }
    Console.WriteLine();
}

static async Task<Ip4RangeSet> TryGetGoogleIpsAsync()
{
    using HttpClient client = new HttpClient();
    var response = await client.GetAsync("https://www.gstatic.com/ipranges/goog.json");
    if (!response.IsSuccessStatusCode)
    {
        return Ip4RangeSet.Empty;
    }
    var responseJson = await response.Content.ReadAsStringAsync();
    var googleIpRanges = JsonSerializer.Deserialize<GoogleIpsResponseRoot>(responseJson).prefixes
        .Select(x => x.ipv4Prefix)
        .Where(x => !string.IsNullOrEmpty(x))
        .ToArray();

    Ip4RangeSet result = new Ip4RangeSet();

    foreach (var ipRange in googleIpRanges)
    {
        if (Ip4Subnet.TryParse(ipRange, out Ip4Subnet subnet))
        {
            result = result.Union(subnet);
        }
    }

    return result;
}

static async Task<Ip4RangeSet> GetNonRuSubnetsAsync(string ruFilePath)
{
    var lines = await File.ReadAllLinesAsync(ruFilePath);
    List<Ip4Subnet> ruSubnets = new List<Ip4Subnet>();

    foreach (string line in lines)
    {
        if (!string.IsNullOrEmpty(line) && Ip4Subnet.TryParse(line, out var subnet))
        {
            ruSubnets.Add(subnet);
        }
    }

    Ip4RangeSet ru = new Ip4RangeSet(ruSubnets);

    var googleIps = await TryGetGoogleIpsAsync();

    return new Ip4RangeSet(Ip4Range.All)
        .Except(ru)
        .Except(googleIps)
        .Except(Ip4Subnet.Parse("10.0.0.0/8"))
        .Except(Ip4Subnet.Parse("100.64.0.0/10"))
        .Except(Ip4Subnet.Parse("127.0.0.0/8"))
        .Except(Ip4Subnet.Parse("169.254.0.0/16"))
        .Except(Ip4Subnet.Parse("172.16.0.0/12"))
        .Except(Ip4Subnet.Parse("192.168.0.0/16"));
}

[UnsupportedOSPlatform("macOS")]
[UnsupportedOSPlatform("OSX")]
static void ChangeRoutes(Ip4RangeSet nonRuIps, string interfaceName, int metric)
{
    var networkInterface = NetworkInterface.GetAllNetworkInterfaces()
        .Where(x => x.Name == interfaceName)
        .Single();

    int interfaceIndex = networkInterface.GetInterfaceIndex();

    var table = new Lazy<List<Ip4RouteEntry>>(Ip4RouteTable.GetRouteTable);

    IPAddress gatewayIp = networkInterface.GetPrimaryGateway(() => table.Value) ?? throw new Exception("PrimaryGateway is null");

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
            Console.WriteLine($"route deleted: {subnet}");
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine($"error deleting route {subnet}: {exception.GetBaseException().Message}");
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
                Metric = 5,
            });
            Console.WriteLine($"route created: {subnet}");
        }
        catch (Exception exception)
        {
            // write to standart error
            Console.Error.WriteLine($"error creating route {subnet}: {exception.GetBaseException().Message}");
        }
    }
}

static async Task SerializeToAmneziaJsonAsync(Ip4RangeSet set, string filePath)
{
    var objectToSerialize = set.ToIp4Subnets()
        .Select(x => new AmneziaItem(x.FirstAddress.ToString() + "/" + x.Mask.Cidr))
        .ToArray();

    await File.WriteAllTextAsync(filePath, JsonSerializer.Serialize(objectToSerialize));
}

Lazy<Task<Ip4RangeSet>> nonRu = new Lazy<Task<Ip4RangeSet>>(async () => await GetNonRuSubnetsAsync("ru.txt"));
const string AmneziaVPN = "AmneziaVPN";
while (true)
{
    Console.WriteLine("1 - Print Adapters");
    Console.WriteLine("2 - Print all route table");
    Console.WriteLine($"3 - Print {AmneziaVPN} route table");
    Console.WriteLine($"4 - Change {AmneziaVPN} route table");
    Console.WriteLine("5 - Create simplified json for Amnezia");
    Console.WriteLine("6 - Print simplified routes for AmneziaWG conf file");
    Console.WriteLine("(esc) - Exit");

    var key = Console.ReadKey(true);
    if (key.Key == ConsoleKey.Escape)
    {
        return;
    }

    switch (key.Key)
    {
        case ConsoleKey.D1:
            AdaptersPrint();
            break;

        case ConsoleKey.D2:
            RoutePrintAll();
            break;

        case ConsoleKey.D3:
            RoutePrintByName(AmneziaVPN);
            break;

        case ConsoleKey.D4:
            foreach (Ip4Range item in (await nonRu.Value).ToIp4Ranges())
            {
                Console.WriteLine($"{item.FirstAddress,15} - {item.LastAddress,15} {item.Count,10} => {string.Join(", ", item.ToSubnets())}");
            }
            ChangeRoutes(await nonRu.Value, AmneziaVPN, 5);
            break;

        case ConsoleKey.D5:
            Console.Write("simplifying ip range: ");
            string? input1 = Console.ReadLine();
            if (!uint.TryParse(input1, out uint delta1))
            {
                Console.Error.WriteLine("wrong input, expected a number");
                break;
            }
            Ip4RangeSet simplifiedSet1 = (await nonRu.Value).Simplify(delta1);
            await SerializeToAmneziaJsonAsync(simplifiedSet1, $"amnezia-nonru-smpl-{delta1}.json");
            break;

        case ConsoleKey.D6:
            Console.Write("simplifying ip range: ");
            string? input2 = Console.ReadLine();
            if (!uint.TryParse(input2, out uint delta2))
            {
                Console.Error.WriteLine("wrong input, expected a number");
                break;
            }
            Ip4RangeSet simplifiedSet2 = (await nonRu.Value).Simplify(delta2);
            await File.WriteAllTextAsync("AllowedIPs.txt", string.Join(", ", simplifiedSet2.ToIp4Subnets().Select(x => x.ToCidrString())));
            break;

        default:
            Console.WriteLine("Invalid option, please try again.");
            break;
    }
}

internal record GoogleIpsResponseRoot(GoogleIpsResponseItem[] prefixes);

internal record GoogleIpsResponseItem(string ipv4Prefix);

internal record AmneziaRoot(AmneziaItem[] items);

internal record AmneziaItem(string hostname);