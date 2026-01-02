using BenchmarkDotNet.Attributes;
using routes;

namespace Ip4Parsers.Benchmarks;

[Config(typeof(BenchmarkManualConfig))]
public class Ip4ParsersBenchmarks
{
    [Params(1_000_000)]
    public int Count { get; set; }

    private string subnetsText = "";
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
        subnetsText = await FetchAndParseRuAggregatedZoneAsync();
        ranges = Enumerable.Range(0, Count).Select(_ =>
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

        subnets = ranges.ToArray().SelectMany(x => x.ToSubnets().ToArray()).Take(Count).ToList();
        subnetsString = string.Join("\n", subnets.Select(r => r.ToCidrString()));

        addresses = ranges.SelectMany(x => new[] { x.FirstAddress, x.LastAddress }).Take(Count).ToList();
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
    public int RealisticGetRanges()
    {
        return Ip4SubnetParser.GetRanges(subnetsText).Length;
    }

    [Benchmark]
    public int ParseAddressesByGetRanges()
    {
        return Ip4SubnetParser.GetRanges(addressesString).Length;
    }

    [Benchmark]
    public int ParseRangesByGetRanges()
    {
        return Ip4SubnetParser.GetRanges(rangesString).Length;
    }

    [Benchmark]
    public int ParseSubnetsByGetRanges()
    {
        return Ip4SubnetParser.GetRanges(subnetsString).Length;
    }
}
