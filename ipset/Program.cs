using AnsiColoredWriters;
using Ip4Parsers;
using routes;

namespace ipops;

internal static class Program
{
    private static Ip4RangeSet raw(IEnumerator<string> enumerator, Action<string?> errorWriteLine)
    {
        if (!enumerator.MoveNext())
        {
            throw new ArgumentException("missing raw argument, should use raw <ips|subnets|ip ranges>");
        }

        var ranges = Ip4SubnetParser.GetRanges(enumerator.Current, errorWriteLine);
        return new Ip4RangeSet(ranges);
    }

    private static async Task<Ip4RangeSet> fileAsync(IEnumerator<string> enumerator, Action<string?> errorWriteLine)
    {
        if (!enumerator.MoveNext())
        {
            throw new ArgumentException("missing file argument, should use file <path>");
        }

        using FileStream fileStream = File.Open(enumerator.Current, FileMode.Open, FileAccess.Read, FileShare.Read);
        using StreamReader streamReader = new StreamReader(fileStream);

        Ip4RangeSet result = new Ip4RangeSet();
        string? line;
        while ((line = await streamReader.ReadLineAsync()) is not null)
        {
            var ranges = Ip4SubnetParser.GetRanges(line, errorWriteLine);
            foreach (var range in ranges)
            {
                result = result.Union(range);
            }
        }
        return result;
    }

    private static Ip4RangeSet stdin(Action<string?> errorWriteLine)
    {
        Ip4RangeSet result = new Ip4RangeSet();
        string? line;
        while ((line = Console.ReadLine()) is not null)
        {
            var ranges = Ip4SubnetParser.GetRanges(line, errorWriteLine);
            foreach (var range in ranges)
            {
                result = result.Union(range);
            }
        }
        return result;
    }

    private static Ip4RangeSet bogon()
    {
        return new Ip4RangeSet([
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
        Action<string?> errorWriteLine = Console.IsErrorRedirected ? Console.Error.WriteLine : new AnsiColoredWriter(Console.Error, AnsiColor.Red).WriteLine;
        Ip4RangeSet result = new();
        RangeSetPrintFormat printFormat = RangeSetPrintFormat.Subnet;
        string printPattern = "%subnet/%cidr";

        var enumerator = args.AsEnumerable().GetEnumerator();
        if (!enumerator.MoveNext())
        {
            Console.Write("""
                usage:
                    ipops [raw <ips> | file <path> | - | bogon] [except|union [raw <ips> | file <path> | - | bogon] | simplify <number> | normalize]* [print subnet | range | amneziajson] [format <string:%subnet/%cidr | %subnet %mask | %firstaddress-%lastaddress>]
                """);
            return;
        }
        do
        {
            switch (enumerator.Current)
            {
                case "raw":
                    result = raw(enumerator, errorWriteLine);
                    break;

                case "file":
                    result = await fileAsync(enumerator, errorWriteLine);
                    break;

                case "-":
                    result = stdin(errorWriteLine);
                    break;

                case "bogon":
                    result = bogon();
                    break;

                case "except":
                    if (!enumerator.MoveNext())
                    {
                        throw new ArgumentException("missing except argument, should use except [raw ips> | file <path> | - | local]");
                    }
                    result = enumerator.Current switch
                    {
                        "raw" => result.Except(raw(enumerator, errorWriteLine)),
                        "file" => result.Except(await fileAsync(enumerator, errorWriteLine)),
                        "-" => result.Except(stdin(errorWriteLine)),
                        "bogon" => result.Except(bogon()),
                        _ => throw new ArgumentException($"unknown argument '{enumerator.Current}'"),
                    };
                    break;

                case "union":
                    if (!enumerator.MoveNext())
                    {
                        throw new ArgumentException("missing union argument, should use union [raw <ips> | file <path> | - | local]");
                    }
                    result = enumerator.Current switch
                    {
                        "raw" => result.Union(raw(enumerator, errorWriteLine)),
                        "file" => result.Union(await fileAsync(enumerator, errorWriteLine)),
                        "-" => result.Union(stdin(errorWriteLine)),
                        "bogon" => result.Union(bogon()),
                        _ => throw new ArgumentException($"unknown argument '{enumerator.Current}'"),
                    };
                    break;

                case "simplify":
                    if (!enumerator.MoveNext())
                    {
                        throw new ArgumentException("missing simplify argument, should use simplify <number>");
                    }
                    uint simplifyRange = uint.Parse(enumerator.Current);
                    result = result.Simplify(simplifyRange);
                    break;

                case "normalize":
                    result = result.Normalize();
                    break;

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
                foreach (var subnet in result.ToIp4Subnets())
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
                foreach (var subnet in result.ToIp4Ranges())
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