using AnsiColoredWriters;
using Ip4Parsers;
using routes;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ipset;

internal static class Program
{
    private static Ip4RangeSet GetLocalIps()
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
    private static async Task SerializeToAmneziaJsonAsync(Ip4RangeSet set, string filePath)
    {
        var objectToSerialize = set.ToIp4Subnets()
            .Select(x => new AmneziaItem(x.ToCidrString()))
            .ToArray();

        await File.WriteAllTextAsync(filePath, JsonSerializer.Serialize(objectToSerialize, SourceGenerationContext.Default.AmneziaItemArray));
    }

    public static void Main(string[] args)
    {
        Action<string?> errorWriteLine = Console.IsErrorRedirected ? Console.Error.WriteLine : new AnsiColoredWriter(Console.Error, AnsiColor.Red).WriteLine;
        Ip4RangeSet result = new();

        var enumerator = args.AsSpan().GetEnumerator();
        if (!enumerator.MoveNext())
        {
            errorWriteLine("""
                usage:
                ipset [raw <ips|subnets|ip ranges> | file <path> | stdin | local] [ except|union [raw <ips|subnets|ip ranges> | file <path> | stdin | local] ]*
                """);
        }
        do
        {
            switch (enumerator.Current)
            {
                case "raw":
                    if (!enumerator.MoveNext())
                    {
                        throw new ArgumentException("missing raw value, should use raw <ips|subnets|ip ranges>");
                    }

                    var ranges1 = Ip4SubnetParser.GetRanges(enumerator.Current, errorWriteLine);
                    result = new Ip4RangeSet(ranges1);
                    break;

                case "file":
                    if (!enumerator.MoveNext())
                    {
                        throw new ArgumentException("missing file value, should use file <path>");
                    }

                    string[] lines2 = File.ReadAllLines(enumerator.Current);
                    var ranges2 = lines2.SelectMany(line => Ip4SubnetParser.GetRanges(line, errorWriteLine));
                    result = new Ip4RangeSet(ranges2);
                    break;

                case "stdin":
                    string? line3;
                    List<Ip4Range> ranges3 = new();
                    while ((line3 = Console.ReadLine()) is not null)
                    {
                        ranges3.AddRange(Ip4SubnetParser.GetRanges(line3, errorWriteLine));
                    }
                    result = new Ip4RangeSet(ranges3);
                    break;

                case "local":
                    result = GetLocalIps();
                    break;

                case "except":
                    if (!enumerator.MoveNext())
                    {
                        throw new ArgumentException("missing except argument, should use except [raw <ips|subnets|ip ranges> | file <path> | stdin | local]");
                    }

                    switch (enumerator.Current)
                    {
                        case "raw":
                            if (!enumerator.MoveNext())
                            {
                                throw new ArgumentException("missing raw value, should use raw <ips|subnets|ip ranges>");
                            }

                            var ranges4 = Ip4SubnetParser.GetRanges(enumerator.Current, errorWriteLine);
                            result = result.Except(new Ip4RangeSet(ranges4));
                            break;

                        case "file":
                            if (!enumerator.MoveNext())
                            {
                                throw new ArgumentException("missing file value, should use file <path>");
                            }

                            string[] lines5 = File.ReadAllLines(enumerator.Current);
                            var ranges5 = lines5.SelectMany(line => Ip4SubnetParser.GetRanges(line, errorWriteLine));
                            result = result.Except(new Ip4RangeSet(ranges5));
                            break;

                        case "stdin":
                            string? line6;
                            List<Ip4Range> ranges6 = new();
                            while ((line6 = Console.ReadLine()) is not null)
                            {
                                ranges6.AddRange(Ip4SubnetParser.GetRanges(line6, errorWriteLine));
                            }
                            result = result.Except(new Ip4RangeSet(ranges6));
                            break;

                        case "local":
                            result = result.Except(GetLocalIps());
                            break;

                        default:
                            throw new ArgumentException($"unknown argument '{enumerator.Current}'");
                    }

                    break;

                case "union":
                    if (!enumerator.MoveNext())
                    {
                        throw new ArgumentException("missing union argument, should use union [raw <ips|subnets|ip ranges> | file <path> | stdin | local]");
                    }

                    switch (enumerator.Current)
                    {
                        case "raw":
                            if (!enumerator.MoveNext())
                            {
                                throw new ArgumentException("missing raw value, should use raw <ips|subnets|ip ranges>");
                            }

                            var ranges4 = Ip4SubnetParser.GetRanges(enumerator.Current, errorWriteLine);
                            result = result.Union(new Ip4RangeSet(ranges4));
                            break;

                        case "file":
                            if (!enumerator.MoveNext())
                            {
                                throw new ArgumentException("missing file value, should use file <path>");
                            }

                            string[] lines5 = File.ReadAllLines(enumerator.Current);
                            var ranges5 = lines5.SelectMany(line => Ip4SubnetParser.GetRanges(line, errorWriteLine));
                            result = result.Union(new Ip4RangeSet(ranges5));
                            break;

                        case "stdin":
                            string? line6;
                            List<Ip4Range> ranges6 = new();
                            while ((line6 = Console.ReadLine()) is not null)
                            {
                                ranges6.AddRange(Ip4SubnetParser.GetRanges(line6, errorWriteLine));
                            }
                            result = result.Union(new Ip4RangeSet(ranges6));
                            break;

                        case "local":
                            result = result.Union(GetLocalIps());
                            break;

                        default:
                            throw new ArgumentException($"unknown argument '{enumerator.Current}'");
                    }

                    break;

                default:
                    throw new ArgumentException($"unknown argument '{enumerator.Current}'");
            }
        } while (enumerator.MoveNext());

        foreach (var subnet in result.ToIp4Subnets())
        {
            Console.WriteLine(subnet.ToCidrString());
        }
    }
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(GoogleIpsResponseRoot))]
[JsonSerializable(typeof(AmneziaItem[]))]
internal sealed partial class SourceGenerationContext : JsonSerializerContext
{
}

internal sealed record AmneziaItem(string hostname);

internal sealed record GoogleIpsResponseRoot(GoogleIpsResponseItem[] prefixes);

internal sealed record GoogleIpsResponseItem(string ipv4Prefix);