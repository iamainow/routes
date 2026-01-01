#pragma warning disable CA1822
#pragma warning disable CA1515
#pragma warning disable CA5394

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using routes;

namespace Ip4Parsers.Benchmarks;

[MemoryDiagnoser]
[Config(typeof(NoPowerPlanConfig))]
public class Ip4ParsersBenchmarks
{
    private string _subnetsText = "";
    private List<Ip4Range> ranges = [];
    private string rangesString = "";
    private List<Ip4Subnet> subnets = [];
    private string subnetsString = "";
    private List<Ip4Address> addresses = [];
    private string addressesString = "";

    [GlobalSetup]
    public async Task GlobalSetup()
    {
        Random random = new();
        _subnetsText = await FetchAndParseRuAggregatedZoneAsync();
        ranges = Enumerable.Range(0, 1_000_000).Select(_ =>
        {
            var address1 = new Ip4Address((uint)random.NextInt64(0, uint.MaxValue));
            var address2 = new Ip4Address((uint)random.NextInt64(0, uint.MaxValue));
            if (address1 <= address2)
            {
                return new Ip4Range(address1, address2);
            }
            else
            {
                return new Ip4Range(address2, address1);
            }
        }).ToList();
        rangesString = string.Join("\n", ranges.Select(r => r.ToString()));

        subnets = ranges.SelectMany(x => x.ToSubnets()).Take(1_000_000).ToList();
        subnetsString = string.Join("\n", subnets.Select(r => r.ToCidrString()));

        addresses = ranges.SelectMany(x => new[] { x.FirstAddress, x.LastAddress }).Take(1_000_000).ToList();
        addressesString = string.Join("\n", addresses.Select(r => r.ToString()));
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
    public Ip4Range[] RealisticGetRanges()
    {
        return Ip4SubnetParser.GetRanges(_subnetsText).ToArray();
    }

    [Benchmark]
    public Ip4Range[] ParseAddressesByGetRanges()
    {
        return Ip4SubnetParser.GetRanges(addressesString).ToArray();
    }

    [Benchmark]
    public Ip4Range[] ParseRangesByGetRanges()
    {
        return Ip4SubnetParser.GetRanges(rangesString).ToArray();
    }

    [Benchmark]
    public Ip4Range[] ParseSubnetsByGetRanges()
    {
        return Ip4SubnetParser.GetRanges(subnetsString).ToArray();
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