using BenchmarkDotNet.Attributes;
using Ip4Parsers;
using System.Runtime.InteropServices;

namespace routes.Benchmarks.Ip4RangeSetStackAllocBenchmark;

[Config(typeof(BenchmarkManualConfig))]
public class Ip4RangeSetStackAllocRealistic
{
    private string _subnetsText = "";
    private Ip4Subnet[] _subnets = [];
    private Ip4Range[] _bogon = [];

    [GlobalSetup]
    public async Task GlobalSetup()
    {
        Random random = new();
        _subnetsText = await FetchAndParseRuAggregatedZoneAsync();
        _subnets = Ip4SubnetParser.GetSubnets(_subnetsText).ToArray();
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
    public int Ip4RangeSetStackAlloc_Realistic()
    {
        var all = Ip4SubnetParser.GetRanges("0.0.0.0/0");
        var ip = Ip4SubnetParser.GetRanges("1.2.3.4");

        var subnets = Ip4SubnetParser.GetRanges(_subnetsText);

        var result = new Ip4RangeSetStackAlloc(stackalloc Ip4Range[20000], all);
        result.ExceptModifySpan(ip);
        result.ExceptModifySpan(_bogon);
        result.ExceptModifySpan(subnets);

        return result.RangesCount;
    }

    [Benchmark]
    public int Ip4RangeSetStackAlloc_RealisticWithoutParser()
    {
        var all = Ip4Range.All;
        var ip = new Ip4Address(1, 2, 3, 4).ToIp4Range();

        ListStackAlloc<Ip4Range> subnetList = new(stackalloc Ip4Range[_subnets.Length]);
        foreach (var subnet in _subnets)
        {
            subnetList.Add(subnet.ToIp4Range());
        }

        var result = new Ip4RangeSetStackAlloc(stackalloc Ip4Range[20000], MemoryMarshal.CreateSpan(ref all, 1));
        result.ExceptModifySpan(MemoryMarshal.CreateSpan(ref ip, 1));
        result.ExceptModifySpan(_bogon);
        result.ExceptModifySpan(subnetList.AsSpan());

        return result.RangesCount;
    }
}
