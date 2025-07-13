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

static bool SimplifyLinkedListExpand(LinkedList<Ip4Range> sortedLinkedList, uint delta)
{
    bool wasListChanged = false;
    LinkedListNode<Ip4Range>? current = sortedLinkedList.First;
    while (current is not null && current.Next is not null)
    {
        var next = current.Next;
        // if gap between neighbors equals or more than delta, remove it
        if ((ulong)(uint)current.Value.LastAddress + delta + 1 >= (uint)next.Value.FirstAddress)
        {
            current.Value = new Ip4Range(current.Value.FirstAddress, next.Value.LastAddress);
            sortedLinkedList.Remove(next);
            wasListChanged = true;
        }
        else
        {
            current = current.Next;
        }
    }

    return wasListChanged;
}

static bool SimplifySetExpand(Ip4RangeSet set, uint delta)
{
    return SimplifyLinkedListExpand(new LinkedList<Ip4Range>(set.ToIp4Ranges().OrderBy(x => x.FirstAddress)), delta);
}

static bool SimplifyLinkedListShrink(LinkedList<Ip4Range> sortedLinkedList, uint delta)
{
    bool wasElementRemoved = false;
    LinkedListNode<Ip4Range>? current = sortedLinkedList.First;
    while (current is not null)
    {
        // if current range is equals or smaller than delta, remove it
        if (current.Value.Count <= delta)
        {
            var toDelete = current;
            current = current.Next;
            sortedLinkedList.Remove(toDelete);
            wasElementRemoved = true;
        }
        else
        {
            current = current.Next;
        }
    }

    return wasElementRemoved;
}

static bool SimplifySetShrink(Ip4RangeSet set, uint delta)
{
    return SimplifyLinkedListShrink(new LinkedList<Ip4Range>(set.ToIp4Ranges().OrderBy(x => x.FirstAddress)), delta);
}

[Obsolete]
static Ip4RangeSet Simplify_old(Ip4RangeSet set, uint delta)
{
    LinkedList<Ip4Range> internalResult = new LinkedList<Ip4Range>(set.ToIp4Ranges());

    bool shouldIterate = false;
    do
    {
        shouldIterate = false;
        LinkedListNode<Ip4Range>? current = null;

        current = internalResult.First;
        while (current is not null && current.Next is not null)
        {
            var next = current.Next;
            // if gap between neighbors is more than delta, remove it
            if ((uint)current.Value.LastAddress + delta + 1 >= (uint)next.Value.FirstAddress)
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
            // if current range is smaller than delta, remove it
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

[Obsolete]
static Ip4RangeSet Simplify_stupid(Ip4RangeSet set, uint delta)
{
    LinkedList<Ip4Range> internalResult = new LinkedList<Ip4Range>(set.ToIp4Ranges());

    bool shouldIterate = false;
    do
    {
        shouldIterate = false;
        shouldIterate |= SimplifyLinkedListExpand(internalResult, delta);
        shouldIterate |= SimplifyLinkedListShrink(internalResult, delta);
    } while (shouldIterate);

    Ip4RangeSet result = new();
    foreach (var item in internalResult)
    {
        result = result.Union(item);
    }

    return result;
}

static Ip4RangeSet Simplify_graduate(Ip4RangeSet set, uint delta)
{
    var result = set;

    while (true)
    {
        ulong minSize = set.ToIp4Ranges().Min(x => x.Count);
        ulong minGap = Ip4RangeSet.All.Except(set).ToIp4Ranges().Min(x => x.Count);

        if (minSize <= minGap && minSize <= delta && minSize <= uint.MaxValue)
        {
            SimplifySetShrink(set, (uint)minSize);
            continue;
        }
        else if (minGap <= minSize && minGap <= delta && minGap <= uint.MaxValue)
        {
            SimplifySetExpand(set, (uint)minGap);
            continue;
        }
        else
        {
            break;
        }
    }

    return result;
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

static void ChangeRoutes(Ip4RangeSet nonRuIps, string interfaceName, int metric)
{
    var networkInterface = NetworkInterface.GetAllNetworkInterfaces()
        .Where(x => x.Name == interfaceName)
        .Single();

    int interfaceIndex = networkInterface.GetInterfaceIndex();

    IPAddress gatewayIp = networkInterface.GetPrimaryGateway() ?? throw new Exception("PrimaryGateway is null");

    var routesToRemove = Ip4RouteTable.GetRouteTable()
        .Where(x => x.InterfaceIndex == interfaceIndex)
        .Where(x => x.Metric == metric)
        .ToArray();

    foreach (var routeToRemove in routesToRemove)
    {
        var subnet = new Ip4Subnet(Ip4Address.Parse(routeToRemove.DestinationIP.ToString()), Ip4Mask.Parse(routeToRemove.SubnetMask.ToString()));
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
                DestinationIP = new IPAddress(subnet.FirstAddress.AsByteArray()),
                SubnetMask = new IPAddress(subnet.Mask.AsByteArray()),
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
while (true)
{
    Console.WriteLine("1 - Print Adapters");
    Console.WriteLine("2 - Print all route table");
    Console.WriteLine("3 - Print AmneziaVPN route table");
    Console.WriteLine("4 - Change AmneziaVPN route table");
    Console.WriteLine("5 - Create json for Amnezia");
    Console.WriteLine("6 - Create simplyfied json for Amnezia");
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
            RoutePrintByName("AmneziaVPN");
            break;

        case ConsoleKey.D4:
            foreach (var item in (await nonRu.Value).ToIp4Ranges())
            {
                Console.WriteLine($"{item.FirstAddress,15} - {item.LastAddress,15} {item.Count,10} => {string.Join(", ", item.ToSubnets())}");
            }
            ChangeRoutes(await nonRu.Value, "AmneziaVPN", 5);
            break;

        case ConsoleKey.D5:
            await SerializeToAmneziaJsonAsync(await nonRu.Value, "amnezia-nonru.json");
            break;

        case ConsoleKey.D6:
            Console.Write("simplifiing ip range: ");
            string? input = Console.ReadLine();
            if (!uint.TryParse(input, out uint delta))
            {
                Console.Error.WriteLine("wrong imput, expected a number");
                break;
            }
            Ip4RangeSet simplifiedSet = Simplify_graduate(await nonRu.Value, delta);
            await SerializeToAmneziaJsonAsync(simplifiedSet, $"amnezia-nonru-smpl-{delta}.json");
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