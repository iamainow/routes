using BenchmarkDotNet.Attributes;
using Ip4Parsers;
using System.Runtime.InteropServices;

namespace routes.Benchmarks.Ip4RangeReadonlySpanBenchmark;

[Config(typeof(BenchmarkManualConfig))]
public class Ip4RangeReadonlySpanRealistic
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
    public int Ip4RangeReadonlySpan_Realistic_WithParser_WithoutBuilder()
    {
        var all = Ip4SubnetParser.GetRanges("0.0.0.0/0");
        var ip = Ip4SubnetParser.GetRanges("1.2.3.4");

        var subnets = Ip4SubnetParser.GetRanges(_subnetsText);

        var result = new Ip4RangeReadonlySpan();

        Span<Ip4Range> s0 = stackalloc Ip4Range[result.CalcUnionBuffer(all.Length)];
        int l0 = result.Union(all, s0);
        var result0 = new Ip4RangeReadonlySpan(s0[..l0]);

        Span<Ip4Range> s1 = stackalloc Ip4Range[result0.CalcExceptBuffer(ip.Length)];
        int l1 = result0.Except(ip, s1);
        var result1 = new Ip4RangeReadonlySpan(s1[..l1]);

        Span<Ip4Range> s2 = stackalloc Ip4Range[result1.CalcExceptBuffer(_bogon.Length)];
        int l2 = result1.Except(_bogon, s2);
        var result2 = new Ip4RangeReadonlySpan(s2[..l2]);

        Span<Ip4Range> s3 = stackalloc Ip4Range[result1.CalcExceptBuffer(subnets.Length)];
        int l3 = result2.Except(subnets, s3);
        var result3 = new Ip4RangeReadonlySpan(s3[..l3]);

        return result3.RangesCount;
    }

    [Benchmark]
    public int Ip4RangeReadonlySpan_Realistic_WithoutParser_WithoutBuilder()
    {
        var all0 = Ip4Range.All;
        var all = MemoryMarshal.CreateSpan(ref all0, 1);

        var ip0 = new Ip4Address(1, 2, 3, 4).ToIp4Range();
        var ip = MemoryMarshal.CreateSpan(ref ip0, 1);

        ListStackAlloc<Ip4Range> subnetList = new(stackalloc Ip4Range[_subnets.Length]);
        foreach (var subnet in _subnets)
        {
            subnetList.Add(subnet.ToIp4Range());
        }

        Span<Ip4Range> subnets = subnetList.AsSpan();

        var result = new Ip4RangeReadonlySpan();

        Span<Ip4Range> s0 = stackalloc Ip4Range[result.CalcUnionBuffer(all.Length)];
        int l0 = result.Union(all, s0);
        var result0 = new Ip4RangeReadonlySpan(s0[..l0]);

        Span<Ip4Range> s1 = stackalloc Ip4Range[result0.CalcExceptBuffer(ip.Length)];
        int l1 = result0.Except(ip, s1);
        var result1 = new Ip4RangeReadonlySpan(s1[..l1]);

        Span<Ip4Range> s2 = stackalloc Ip4Range[result1.CalcExceptBuffer(_bogon.Length)];
        int l2 = result1.Except(_bogon, s2);
        var result2 = new Ip4RangeReadonlySpan(s2[..l2]);

        Span<Ip4Range> s3 = stackalloc Ip4Range[result1.CalcExceptBuffer(subnets.Length)];
        int l3 = result2.Except(subnets, s3);
        var result3 = new Ip4RangeReadonlySpan(s3[..l3]);

        return result3.RangesCount;
    }

    [Benchmark]
    public int Ip4RangeReadonlySpan_Realistic_WithoutParser_WithBuilder()
    {
        var all0 = Ip4Range.All;
        var all = MemoryMarshal.CreateSpan(ref all0, 1);

        var ip0 = new Ip4Address(1, 2, 3, 4).ToIp4Range();
        var ip = MemoryMarshal.CreateSpan(ref ip0, 1);

        ListStackAlloc<Ip4Range> subnetList = new(stackalloc Ip4Range[_subnets.Length]);
        foreach (var subnet in _subnets)
        {
            subnetList.Add(subnet.ToIp4Range());
        }

        Span<Ip4Range> subnets = subnetList.AsSpan();

        // Estimate buffers: operations: 3 (union, except, except), ranges: all + ip + bogon + subnets
        Span<Ip4RangeReadonlySpanBuilder.OperationData> ops = stackalloc Ip4RangeReadonlySpanBuilder.OperationData[16];
        Span<Ip4Range> rangeBuffer = stackalloc Ip4Range[all.Length + ip.Length + _bogon.Length + subnets.Length];
        var builder = new Ip4RangeReadonlySpanBuilder(ReadOnlySpan<Ip4Range>.Empty, rangeBuffer, ops)
            .Union(all)
            .Except(ip)
            .Except(_bogon)
            .Except(subnets);

        int totalBuffer = builder.CalcTotalBuffer();
        Span<Ip4Range> resultBuffer = stackalloc Ip4Range[totalBuffer];
        var result = builder.Execute(resultBuffer);

        return result.RangesCount;
    }
}