#pragma warning disable CA1822
#pragma warning disable CA1515
#pragma warning disable CA5394

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using Ip4Parsers;

namespace routes.Benchmarks;

[MemoryDiagnoser]
[Config(typeof(NoPowerPlanConfig))]
public class Ip4RangeSetBenchmarks
{
    private string _subnetsText = "";
    private Ip4Subnet[] _subnets = [];
    private Ip4Range[] _bogon = [];
    private List<Ip4RangeSet2> rangeSetsBy10 = [];

    [GlobalSetup]
    public async Task GlobalSetup()
    {
        Random random = new();
        _subnetsText = await FetchAndParseRuAggregatedZoneAsync();
        _subnets = Ip4SubnetParser.GetSubnets(_subnetsText, null).ToArray();
        _bogon =
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
        ];
        List<Ip4Range> ranges = Enumerable.Range(0, 1_000_000).Select(_ =>
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

        rangeSetsBy10 = ranges.Chunk(10).Select(chunk => new Ip4RangeSet2(chunk)).ToList();
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

        var subnets = new Ip4RangeSet2(Ip4SubnetParser.GetRanges(_subnetsText));

        var result = all;
        result.Except(ip);
        result.Except(bogon);
        result.Except(subnets);

        return result;
    }

    [Benchmark]
    public Ip4RangeSet2 RealisticWithoutParser()
    {
        var all = Ip4RangeSet2.All;
        var ip = new Ip4RangeSet2(new Ip4Address(1, 2, 3, 4));
        var bogon = new Ip4RangeSet2(_bogon);

        var subnets = new Ip4RangeSet2(_subnets);

        var result = all;
        result.Except(ip);
        result.Except(bogon);
        result.Except(subnets);

        return result;
    }

    [Benchmark]
    public Ip4RangeSet2 Union3Except4()
    {
        Random random = new();
        Ip4RangeSet2 result = new();
        for (int index = 0; index < 100_000; index++)
        {
            if (random.NextDouble() < 0.5d)
            {
                result.Union3(rangeSetsBy10[index]);
            }
            else
            {
                result.Except4(rangeSetsBy10[index]);
            }
        }

        return result;
    }

    [Benchmark]
    public Ip4RangeSet2 Union4Except4()
    {
        Random random = new();
        Ip4RangeSet2 result = new();
        for (int index = 0; index < 100_000; index++)
        {
            if (random.NextDouble() < 0.5d)
            {
                result.Union4(rangeSetsBy10[index]);
            }
            else
            {
                result.Except4(rangeSetsBy10[index]);
            }
        }

        return result;
    }
}

public class NoPowerPlanConfig : ManualConfig
{
    public NoPowerPlanConfig()
    {
        // Explicitly use the user's current power plan to prevent BenchmarkDotNet
        // from changing the Windows power plan during benchmark execution
        AddJob(Job.Default
            .DontEnforcePowerPlan()
            .WithRuntime(CoreRuntime.Core10_0));

        //AddJob(Job.Default
        //    .DontEnforcePowerPlan()
        //    .WithRuntime(NativeAotRuntime.Net10_0));
    }
}