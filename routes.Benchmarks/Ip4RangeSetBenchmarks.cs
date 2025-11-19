#pragma warning disable CA1822
#pragma warning disable CA1515
#pragma warning disable CA5394

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Ip4Parsers;

namespace routes.Benchmarks;

[SimpleJob(RuntimeMoniker.Net10_0, baseline: true)]
[SimpleJob(RuntimeMoniker.Net90)]
[SimpleJob(RuntimeMoniker.Net80)]
[MemoryDiagnoser]
public class Ip4RangeSetBenchmarks
{
    private string _subnets = "";
    private Ip4Range[] _bogon = [];
    private List<Ip4Range> ranges = new();

    [GlobalSetup]
    public async Task GlobalSetup()
    {
        Random random = new(42);
        _subnets = await FetchAndParseRuAggregatedZoneAsync();
        _bogon = new Ip4Range[]
        {
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
        };
        ranges = Enumerable.Range(0, 1_000_000).Select(_ =>
        {
            var address1 = new Ip4Address((uint)random.NextInt64(0, uint.MaxValue));
            var address2 = new Ip4Address((uint)random.NextInt64(0, uint.MaxValue));
            if (address1 < address2)
            {
                return new Ip4Range(address1, address2);
            }
            else
            {
                return new Ip4Range(address2, address1);
            }
        }).ToList();
    }

    private static async Task<string> FetchAndParseRuAggregatedZoneAsync()
    {
        try
        {
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(new Uri("https://www.ipdeny.com/ipblocks/data/aggregated/ru-aggregated.zone"));
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return content;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching from internet: {ex.Message}");
            throw;
        }
    }

    [Benchmark]
    public Ip4RangeSet2 Realistic()
    {
        var all = new Ip4RangeSet2(Ip4SubnetParser.GetRanges("0.0.0.0/0"));
        var ip = new Ip4RangeSet2(Ip4SubnetParser.GetRanges("1.2.3.4"));
        var bogon = new Ip4RangeSet2(_bogon);

        var subnets = new Ip4RangeSet2(Ip4SubnetParser.GetRanges(_subnets));

        var result = all;
        result.Except(ip);
        result.Except(bogon);
        result.Except(subnets);
        result.Normalize();

        return result;
    }


    [Benchmark]
    public Ip4RangeSet2 o10k()
    {
        Ip4RangeSet2 result = new();
        for (int index = 0; index < 10_000; index++)
        {
            if (index % 2 == 0)
            {
                result.Union(ranges[index]);
            }
            else
            {
                result.Except(ranges[index]);
            }
        }

        return result;
    }

    [Benchmark]
    public Ip4RangeSet2 o100k()
    {
        Ip4RangeSet2 result = new();
        for (int index = 0; index < 100_000; index++)
        {
            if (index % 2 == 0)
            {
                result.Union(ranges[index]);
            }
            else
            {
                result.Except(ranges[index]);
            }
        }

        return result;
    }

    [Benchmark]
    public Ip4RangeSet2 o1000k()
    {
        Ip4RangeSet2 result = new();
        for (int index = 0; index < 1_000_000; index++)
        {
            if (index % 2 == 0)
            {
                result.Union(ranges[index]);
            }
            else
            {
                result.Except(ranges[index]);
            }
        }

        return result;
    }
}