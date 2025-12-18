using Ip4Parsers;
using routes;

namespace ipops;

internal static class Program
{
    private static Ip4RangeSet raw(IEnumerator<string> enumerator)
    {
        if (!enumerator.MoveNext())
        {
            throw new ArgumentException("missing raw argument, should use raw <ips|subnets|ip ranges>");
        }

        IEnumerable<Ip4Range> ranges = Ip4SubnetParser.GetRanges(enumerator.Current);
        return new Ip4RangeSet(ranges);
    }

    private static async Task<Ip4RangeSet> fileAsync(IEnumerator<string> enumerator)
    {
        if (!enumerator.MoveNext())
        {
            throw new ArgumentException("missing file argument, should use file <path>");
        }

        using FileStream fileStream = File.Open(enumerator.Current, FileMode.Open, FileAccess.Read, FileShare.Read);
        using StreamReader streamReader = new(fileStream);

        Ip4RangeSet result = new();
        string? line;
        while ((line = await streamReader.ReadLineAsync()) is not null)
        {
            IEnumerable<Ip4Range> ranges = Ip4SubnetParser.GetRanges(line);
            result.Union(ranges);
        }
        return result;
    }

    private static Ip4RangeSet stdin()
    {
        Ip4RangeSet result = new();
        string? line;
        while ((line = Console.ReadLine()) is not null)
        {
            IEnumerable<Ip4Range> ranges = Ip4SubnetParser.GetRanges(line);
            result.Union(ranges);
        }
        return result;
    }

    private static Ip4RangeSet bogon()
    {
        return new Ip4RangeSet(
        [
            Ip4Subnet.Parse("0.0.0.0/8"), // "This" network
            Ip4Subnet.Parse("10.0.0.0/8"), // Private-use networks
            Ip4Subnet.Parse("100.64.0.0/10"), // Carrier-grade NAT
            Ip4Subnet.Parse("127.0.0.0/8"), // Loopback
            Ip4Subnet.Parse("169.254.0.0/16"), // Link local
            Ip4Subnet.Parse("172.16.0.0/12"), // Private-use networks
            Ip4Subnet.Parse("192.0.0.0/24"), // IETF Protocol Assignments
            Ip4Subnet.Parse("192.0.2.0/24"), // TEST-NET-1
            Ip4Subnet.Parse("198.51.100.0/24"), // TEST-NET-2
            Ip4Subnet.Parse("192.88.99.0/24"), // 6to4 Relay Anycast
            Ip4Subnet.Parse("192.168.0.0/16"), // Private-use networks
            Ip4Subnet.Parse("198.18.0.0/15"), // Benchmark testing
            Ip4Subnet.Parse("203.0.113.0/24"), // TEST-NET-3
            Ip4Subnet.Parse("224.0.0.0/4"), // Multicast
            Ip4Subnet.Parse("240.0.0.0/4"), // Reserved for future use
        ]);
    }

    public static async Task Main(string[] args)
    {
        Ip4RangeSet result = new();
        RangeSetPrintFormat printFormat = RangeSetPrintFormat.Subnet;
        string printPattern = "%subnet/%cidr";

        IEnumerator<string> enumerator = args.AsEnumerable().GetEnumerator();
        if (!enumerator.MoveNext())
        {
            Console.Write("""
                usage:
                    ipops [raw <ips> | file <path> | - | bogon] [except|union [raw <ips> | file <path> | - | bogon] | simplify <number>]* [print subnet | range | amneziajson] [format <string:%subnet/%cidr | %subnet %mask | %firstaddress-%lastaddress>]
                """);
            return;
        }
        do
        {
            switch (enumerator.Current)
            {
                case "raw":
                    result = raw(enumerator);
                    break;

                case "file":
                    result = await fileAsync(enumerator);
                    break;

                case "-":
                    result = stdin();
                    break;

                case "bogon":
                    result = bogon();
                    break;

                case "except":
                    if (!enumerator.MoveNext())
                    {
                        throw new ArgumentException("missing except argument, should use except [raw ips> | file <path> | - | local]");
                    }
                    switch (enumerator.Current)
                    {
                        case "raw":
                            result.Except(raw(enumerator));
                            break;
                        case "file":
                            result.Except(await fileAsync(enumerator));
                            break;
                        case "-":
                            result.Except(stdin());
                            break;
                        case "bogon":
                            result.Except(bogon());
                            break;
                        default:
                            throw new ArgumentException($"unknown argument '{enumerator.Current}'");
                    }

                    break;

                case "union":
                    if (!enumerator.MoveNext())
                    {
                        throw new ArgumentException("missing union argument, should use union [raw <ips> | file <path> | - | local]");
                    }
                    switch (enumerator.Current)
                    {
                        case "raw":
                            result.Union(raw(enumerator));
                            break;
                        case "file":
                            result.Union(await fileAsync(enumerator));
                            break;
                        case "-":
                            result.Union(stdin());
                            break;
                        case "bogon":
                            result.Union(bogon());
                            break;
                        default:
                            throw new ArgumentException($"unknown argument '{enumerator.Current}'");
                    }

                    break;

                case "simplify":
                    if (!enumerator.MoveNext())
                    {
                        throw new ArgumentException("missing simplify argument, should use simplify <number>");
                    }
                    uint simplifyRange = uint.Parse(enumerator.Current);
                    result.MinimizeSubnets(simplifyRange);
                    break;

                case "normalize":
                    throw new ArgumentException("normalize is obsolete. result always normalized. remove normalize from arguments.");

                case "print":
                    if (!enumerator.MoveNext())
                    {
                        throw new ArgumentException("missing print argument, should use print [subnet | range | amneziajson]");
                    }
                    printFormat = enumerator.Current switch
                    {
                        "subnet" => RangeSetPrintFormat.Subnet,
                        "range" => RangeSetPrintFormat.Range,
                        "amneziajson" => RangeSetPrintFormat.AmneziaJson,
                        _ => throw new ArgumentException($"unknown argument '{enumerator.Current}'"),
                    };
                    break;

                case "format":
                    if (!enumerator.MoveNext())
                    {
                        throw new ArgumentException("missing format argument, should use format <string:%subnet/%cidr | %subnet %mask | %firstaddress-%lastaddress>");
                    }
                    printPattern = enumerator.Current;
                    break;

                default:
                    throw new ArgumentException($"unknown argument '{enumerator.Current}'");
            }
        } while (enumerator.MoveNext());

        switch (printFormat)
        {
            case RangeSetPrintFormat.Subnet:
                foreach (Ip4Subnet subnet in result.ToIp4Subnets())
                {
                    string resultString = printPattern
                        .Replace("%subnet", subnet.FirstAddress.ToString())
                        .Replace("%cidr", subnet.Mask.Cidr.ToString())
                        .Replace("%mask", subnet.Mask.ToFullString())
                        .Replace("%firstaddress", subnet.FirstAddress.ToString())
                        .Replace("%lastaddress", subnet.LastAddress.ToString())
                        .Replace("%count", subnet.Count.ToString());

                    Console.WriteLine(resultString);
                }
                break;

            case RangeSetPrintFormat.Range:
                foreach (Ip4Range subnet in result.ToIp4Ranges())
                {
                    string resultString = printPattern
                        .Replace("%firstaddress", subnet.FirstAddress.ToString())
                        .Replace("%lastaddress", subnet.LastAddress.ToString())
                        .Replace("%count", subnet.Count.ToString());

                    Console.WriteLine(resultString);
                }
                break;

            case RangeSetPrintFormat.AmneziaJson:
                {
                    string resultString = Ip4RangeSetSerializers.SerializeToAmneziaJson(result);
                    Console.WriteLine(resultString);
                }

                break;

            default:
                throw new NotImplementedException($"switch printFormat = '{printFormat}'");
        }
    }
}

internal enum RangeSetPrintFormat
{
    Subnet,
    Range,
    AmneziaJson,
}