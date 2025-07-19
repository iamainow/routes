using routes;
using routes.core;
using routeTable;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.Versioning;
using System.Text.Json;
using System.Text.RegularExpressions;

static void AdaptersPrint()
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

static void RoutePrintAll()
{
    Ip4RouteEntry[] routeTable = Ip4RouteTable.GetRouteTable();

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
    Ip4RouteEntry[] routeTable = Ip4RouteTable.GetRouteTable();

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
    List<Ip4Subnet> ruSubnets = [];

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
        .Except(GetLocalIps());
}
// routes -ip4all | routes -except -file "ru.txt" | routes -except -google | routes -except -local | interface -set-routes -name "AmneziaVPN" -metric 5 -gateway default

static Ip4RangeSet GetLocalIps()
{
    return new Ip4RangeSet([
        Ip4Subnet.Parse("10.0.0.0/8"),
        Ip4Subnet.Parse("100.64.0.0/10"),
        Ip4Subnet.Parse("127.0.0.0/8"),
        Ip4Subnet.Parse("169.254.0.0/16"),
        Ip4Subnet.Parse("172.16.0.0/12"),
        Ip4Subnet.Parse("192.168.0.0/16"),
    ]);
}

[UnsupportedOSPlatform("macOS")]
[UnsupportedOSPlatform("OSX")]
static void ChangeRoutes(Ip4RangeSet nonRuIps, string interfaceName, int metric)
{
    var networkInterface = NetworkInterface.GetAllNetworkInterfaces()
        .Where(x => x.Name == interfaceName)
        .Single();

    int interfaceIndex = networkInterface.GetInterfaceIndex();

    var table = new Lazy<Ip4RouteEntry[]>(Ip4RouteTable.GetRouteTable);

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
        .Select(x => new AmneziaItem(x.ToCidrString()))
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
    Console.WriteLine("5 - Simplify & save Amnezia routes json");
    Console.WriteLine("6 - Simplify & save AmneziaWG conf file");
    Console.WriteLine("7 - Minimize subnets & save AmneziaWG conf file");
    Console.WriteLine("8 - Save as route add X.X.X.X mask Y.Y.Y.Y Z.Z.Z.Z");
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
            Ip4RangeSet simplifiedSet2 = (await nonRu.Value).Simplify(delta2).Except(GetLocalIps());
            await File.WriteAllTextAsync($"AllowedIPs-smpl-{delta2}.txt", string.Join(", ", simplifiedSet2.ToIp4Subnets().Select(x => x.ToCidrString())));
            break;

        case ConsoleKey.D7:
            Console.Write("simplifying ip range: ");
            string? input3 = Console.ReadLine();
            if (!uint.TryParse(input3, out uint delta3))
            {
                Console.Error.WriteLine("wrong input, expected a number");
                break;
            }
            Ip4RangeSet simplifiedSet3 = (await nonRu.Value).MinimizeSubnets(delta3).Except(GetLocalIps());
            await File.WriteAllTextAsync($"AllowedIPs-mnmz-{delta3}.txt", string.Join(", ", simplifiedSet3.ToIp4Subnets().Select(x => x.ToCidrString())));
            break;

        case ConsoleKey.D8:
            Console.Write("simplifying ip range: ");
            string? input4 = Console.ReadLine();
            Console.WriteLine();
            if (!uint.TryParse(input4, out uint delta4))
            {
                Console.Error.WriteLine("wrong input, expected a number");
                break;
            }

            Console.Write("gateway: ");
            string? input44 = Console.ReadLine();
            Console.WriteLine();

            Ip4RangeSet simplifiedSet4 = (await nonRu.Value).MinimizeSubnets(delta4).Except(GetLocalIps());

            string[] lines4 = simplifiedSet4.ToIp4Subnets().Select(x => $"route add {x.FirstAddress} mask {x.Mask.ToFullString()} {input44}").ToArray();
            int part = 1;
            foreach (var lines44 in lines4.Chunk(1024))
            {
                await File.WriteAllTextAsync($"route-mnmz-{delta4}-part{part++}.txt", string.Join("\r\n", lines44));
            }

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

public class Parameters
{
    public required int Quality { get; set; }
    public required string Source { get; set; }
    public required string Destination { get; set; }
    public required string SourceExtension { get; set; }
    public required string ConverterExe { get; set; }
}

internal class InternalParameters
{
    public int? Quality { get; set; }
    public string? Source { get; set; }
    public string? Destination { get; set; }
    public string? SourceExtension { get; set; }
    public string? ConverterExe { get; set; }
    public Parameters ToParameters()
    {
        ArgumentNullException.ThrowIfNull(Quality);
        ArgumentNullException.ThrowIfNull(Source);
        ArgumentNullException.ThrowIfNull(Destination);
        ArgumentNullException.ThrowIfNull(SourceExtension);
        ArgumentNullException.ThrowIfNull(ConverterExe);

        return new Parameters
        {
            Quality = Quality.Value,
            Source = Source,
            Destination = Destination,
            SourceExtension = SourceExtension,
            ConverterExe = ConverterExe,
        };
    }
}

public static partial class Ip4AddressParser
{
    [GeneratedRegex(@"(?<ip>((25[0-5]|(2[0-4]|1\d|[1-9]|)\d)\.?\b){4})")]
    public static partial Regex RegEx();

    public static IEnumerable<Ip4Address> GetAddresses(string text)
    {
        foreach (Match match in RegEx().Matches(text))
        {
            if (match.Success && match.Groups["ip"].Success)
            {
                if (Ip4Address.TryParse(match.Groups["ip"].Value, out Ip4Address address))
                {
                    yield return address;
                }
            }
        }
    }
}

public static partial class Ip4SubnetParser
{
    [GeneratedRegex(@"\b(?<ip>((25[0-5]|(2[0-4]|1\d|[1-9]|)\d)\.?\b){4})\/(?<mask>3[0-2]|([1-2]|)\d)\b")]
    public static partial Regex CidrRegEx();

    [GeneratedRegex(@"\b(?<ip>((25[0-5]|(2[0-4]|1\d|[1-9]|)\d)\.?\b){4}) (?<mask>((25[0-5]|(2[0-4]|1\d|[1-9]|)\d)\.?\b){4})\b")]
    public static partial Regex FullRegEx();

    public static IEnumerable<Ip4Subnet> GetSubnets(string text)
    {
        var matches = Enumerable.Concat(CidrRegEx().Matches(text), FullRegEx().Matches(text)).OrderBy(x => x.Index);
        foreach (Match match in matches)
        {
            if (match.Success && match.Groups["ip"].Success && match.Groups["mask"].Success)
            {
                if (Ip4Address.TryParse(match.Groups["ip"].Value, out Ip4Address address) && Ip4Mask.TryParse(match.Groups["mask"].Value, out Ip4Mask mask))
                {
                    yield return new Ip4Subnet(address, mask);
                }
            }
        }
    }
}

// -get-interface [-name "AmneziaVPN"] [-index 15] [-gateway "128.0.0.1"]                                        [-out "%name %index %gateway"]
// -get-routes [-subnet "127.0.0.1"] [-mask ""] [-cidrmask ""] [-metric ""] [-name ""] [-index ""] [-gateway ""] [-out "%subnet %mask %cidrmask %metric %name %index %gateway"] e.g. "route add %subnet mask %mask %gateway", (default) "%subnet/%cidrmask"
// -union-routes -file "" -std -file "" -inline "127.0.0.1/8"                                                    [-out "%subnet %firstip %mask %cidrmask %lastip"] e.g. "%subnet-%lastip", (default) "%subnet/%cidrmask", "%subnet %mask"
// -except-routes [-std] [-file ""] [-inline "127.0.0.1/8"] (1 except 2 except 3 etc.)                           [-out "%subnet %firstip %mask %cidrmask %lastip"] e.g. "%subnet-%lastip", (default) "%subnet/%cidrmask", "%subnet %mask"
// -intersect [-file ""] [-std] [-file ""] [-inline "127.0.0.1/8"]                                               [-out "%subnet %firstip %mask %cidrmask %lastip"] e.g. "%subnet-%lastip", (default) "%subnet/%cidrmask", "%subnet %mask"
public static partial class ParametersBuilder
{
    public static Parameters? Parse(ReadOnlySpan<string> args)
    {
        var enumerator = args.GetEnumerator();
        if (!enumerator.MoveNext())
        {
            Console.Write("""
                    -quality x : where x = [0, 10] - quality of converted audio files
                    -source-directory <path> : source directory
                    -dest-directory <path> : destination directory
                    -source-ext <ext> : source extension e.g. .flac, .wav
                    -ffmpeg <path> : path to ffmpeg executable
                    """);

            return null;
        }

        InternalParameters result = new();
        do
        {
            switch (enumerator.Current)
            {
                case "-quality":
                    if (!enumerator.MoveNext())
                    {
                        throw new ArgumentException("missing -quality value, should use -quality [0, 10]");
                    }

                    if (!int.TryParse(enumerator.Current, out int quality))
                    {
                        throw new ArgumentException("-quality should be a number [0, 10]");
                    }

                    result.Quality = quality;
                    break;

                case "-source-directory":
                    if (!enumerator.MoveNext())
                    {
                        throw new ArgumentException("missing -source-directory value, should use -source-directory <path>");
                    }

                    if (string.IsNullOrEmpty(enumerator.Current))
                    {
                        throw new ArgumentException("missing -source-directory value, should use -source-directory <path>");
                    }

                    result.Source = enumerator.Current;
                    break;

                case "-dest-directory":
                    if (!enumerator.MoveNext())
                    {
                        throw new ArgumentException("missing -dest-directory value, should use -dest-directory <path>");
                    }

                    if (string.IsNullOrEmpty(enumerator.Current))
                    {
                        throw new ArgumentException("missing -dest-directory value, should use -dest-directory <path>");
                    }

                    result.Destination = enumerator.Current;
                    break;

                case "-source-ext":
                    if (!enumerator.MoveNext())
                    {
                        throw new ArgumentException("missing -source-ext value, should use -source-ext <ext>");
                    }

                    if (string.IsNullOrEmpty(enumerator.Current))
                    {
                        throw new ArgumentException("missing -source-ext value, should use -source-ext <ext>");
                    }

                    if (!enumerator.Current.StartsWith("."))
                    {
                        throw new ArgumentException("-source-ext value should start with '.'");
                    }

                    result.SourceExtension = enumerator.Current;
                    break;

                case "-ffmpeg":
                    if (!enumerator.MoveNext())
                    {
                        throw new ArgumentException("missing -ffmpeg value, should use -ffmpeg <path>");
                    }

                    if (string.IsNullOrEmpty(enumerator.Current))
                    {
                        throw new ArgumentException("missing -ffmpeg value, should use -ffmpeg <path>");
                    }

                    result.ConverterExe = enumerator.Current;
                    break;

                default:
                    throw new ArgumentException($"unknown argument '{enumerator.Current}'");
            }
        } while (enumerator.MoveNext());

        return result.ToParameters();
    }
}