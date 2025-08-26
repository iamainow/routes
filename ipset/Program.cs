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

    private static Ip4RangeSet local()
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

    public static async Task Main(string[] args)
    {
        Action<string?> errorWriteLine = Console.IsErrorRedirected ? Console.Error.WriteLine : new AnsiColoredWriter(Console.Error, AnsiColor.Red).WriteLine;
        Ip4RangeSet result = new();

        var enumerator = args.AsEnumerable().GetEnumerator();
        if (!enumerator.MoveNext())
        {
            Console.WriteLine("""
                usage:
                ipset [raw <ips|subnets|ip ranges> | file <path> | - | local] [ except|union [raw <ips|subnets|ip ranges> | file <path> | - | local] ]*
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

                case "local":
                    result = local();
                    break;

                case "except":
                    if (!enumerator.MoveNext())
                    {
                        throw new ArgumentException("missing except argument, should use except [raw <ips|subnets|ip ranges> | file <path> | - | local]");
                    }

                    switch (enumerator.Current)
                    {
                        case "raw":
                            result = result.Except(raw(enumerator, errorWriteLine));
                            break;

                        case "file":
                            result = result.Except(await fileAsync(enumerator, errorWriteLine));
                            break;

                        case "-":
                            result = result.Except(stdin(errorWriteLine));
                            break;

                        case "local":
                            result = result.Except(local());
                            break;

                        default:
                            throw new ArgumentException($"unknown argument '{enumerator.Current}'");
                    }

                    break;

                case "union":
                    if (!enumerator.MoveNext())
                    {
                        throw new ArgumentException("missing union argument, should use union [raw <ips|subnets|ip ranges> | file <path> | - | local]");
                    }

                    switch (enumerator.Current)
                    {
                        case "raw":
                            result = result.Union(raw(enumerator, errorWriteLine));
                            break;

                        case "file":
                            result = result.Union(await fileAsync(enumerator, errorWriteLine));
                            break;

                        case "-":
                            result = result.Union(stdin(errorWriteLine));
                            break;

                        case "local":
                            result = result.Union(local());
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