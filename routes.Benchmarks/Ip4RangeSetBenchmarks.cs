#pragma warning disable CA1002
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
    private string _subnets = "";
    private Ip4Range[] _bogon = [];
    private List<Ip4Range> ranges = new();
    private (Ip4RangeSet2, Ip4RangeSet2)[][] sets = new (Ip4RangeSet2, Ip4RangeSet2)[16][];

    [GlobalSetup]
    public async Task GlobalSetup()
    {
        Random random = new();
        _subnets = await FetchAndParseRuAggregatedZoneAsync();
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
        ranges = Enumerable.Range(0, 1 << 20).Select(_ =>
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

        IEnumerable<Ip4Range> getAndMoveNext(List<Ip4Range>.Enumerator enumerator, int size)
        {
            var result = enumerator.Current;
            for (int i = 0; i < size; i++)
            {
                var temp = enumerator.Current;
                enumerator.MoveNext();
                yield return temp;
            }
        }
        for (int index = 0; index < sets.Length; index++)
        {
            var enumerator = ranges.GetEnumerator();
            int size = 1 << index;
            int retries = 1 << (20 - 1 - index);
            sets[index] = Enumerable.Range(0, retries).Select(_ => (new Ip4RangeSet2(getAndMoveNext(enumerator, size)), new Ip4RangeSet2(getAndMoveNext(enumerator, size)))).ToArray();
        }
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
    public List<Ip4RangeSet2> Union1x1()
    {
        List<Ip4RangeSet2> result = [];

        var set = sets[1].Take(100);
        foreach (var (t1, t2) in set)
        {
            t1.Union(t2);
        }

        return result;
    }

    [Benchmark]
    public List<Ip4RangeSet2> Union2x2()
    {
        List<Ip4RangeSet2> result = [];

        var set = sets[2].Take(100);
        foreach (var (t1, t2) in set)
        {
            t1.Union(t2);
        }

        return result;
    }

    [Benchmark]
    public List<Ip4RangeSet2> Union3x3()
    {
        List<Ip4RangeSet2> result = [];

        var set = sets[3].Take(100);
        foreach (var (t1, t2) in set)
        {
            t1.Union(t2);
        }

        return result;
    }

    [Benchmark]
    public List<Ip4RangeSet2> Union4x4()
    {
        List<Ip4RangeSet2> result = [];

        var set = sets[4].Take(100);
        foreach (var (t1, t2) in set)
        {
            t1.Union(t2);
        }

        return result;
    }

    [Benchmark]
    public List<Ip4RangeSet2> Union5x5()
    {
        List<Ip4RangeSet2> result = [];

        var set = sets[5].Take(100);
        foreach (var (t1, t2) in set)
        {
            t1.Union(t2);
        }

        return result;
    }

    [Benchmark]
    public List<Ip4RangeSet2> Union6x6()
    {
        List<Ip4RangeSet2> result = [];

        var set = sets[6].Take(100);
        foreach (var (t1, t2) in set)
        {
            t1.Union(t2);
        }

        return result;
    }

    [Benchmark]
    public List<Ip4RangeSet2> Union7x7()
    {
        List<Ip4RangeSet2> result = [];

        var set = sets[7].Take(100);
        foreach (var (t1, t2) in set)
        {
            t1.Union(t2);
        }

        return result;
    }

    [Benchmark]
    public List<Ip4RangeSet2> Union8x8()
    {
        List<Ip4RangeSet2> result = [];

        var set = sets[8].Take(100);
        foreach (var (t1, t2) in set)
        {
            t1.Union(t2);
        }

        return result;
    }

    [Benchmark]
    public List<Ip4RangeSet2> Union9x9()
    {
        List<Ip4RangeSet2> result = [];

        var set = sets[9].Take(100);
        foreach (var (t1, t2) in set)
        {
            t1.Union(t2);
        }

        return result;
    }

    [Benchmark]
    public List<Ip4RangeSet2> Union10x10()
    {
        List<Ip4RangeSet2> result = [];

        var set = sets[10].Take(100);
        foreach (var (t1, t2) in set)
        {
            t1.Union(t2);
        }

        return result;
    }

    [Benchmark]
    public List<Ip4RangeSet2> Union11x11()
    {
        List<Ip4RangeSet2> result = [];

        var set = sets[11].Take(100);
        foreach (var (t1, t2) in set)
        {
            t1.Union(t2);
        }

        return result;
    }

    [Benchmark]
    public List<Ip4RangeSet2> Union12x12()
    {
        List<Ip4RangeSet2> result = [];

        var set = sets[12].Take(100);
        foreach (var (t1, t2) in set)
        {
            t1.Union(t2);
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

        AddJob(Job.Default
            .DontEnforcePowerPlan()
            .WithRuntime(NativeAotRuntime.Net10_0));
    }
}