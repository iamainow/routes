#pragma warning disable CA1822
#pragma warning disable CA1515
#pragma warning disable CA5394
#pragma warning disable CA2014
#pragma warning disable CA1707

using BenchmarkDotNet.Attributes;

namespace routes.Benchmarks.Ip4RangeSetBenchmark;

[MemoryDiagnoser]
[ExceptionDiagnoser]
[Config(typeof(NoPowerPlanConfig))]
public class Ip4RangeSetExcept
{
    [Params(1_000, 10_000, 100_000)]
    public int Count { get; set; }

    private List<Ip4RangeSet> rangeSets1 = [];
    private List<Ip4RangeSet> rangeSets2 = [];

    [GlobalSetup]
    public async Task GlobalSetup()
    {
        Random random = new();
        const int pack = 10;
        List<Ip4Range> ranges = Enumerable.Range(0, Count * pack * 2).Select(_ =>
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

        rangeSets1 = ranges.Chunk(pack).Select(chunk => new Ip4RangeSet(chunk)).ToList();
        rangeSets2 = ranges.Skip(Count * pack).Chunk(pack).Select(chunk => new Ip4RangeSet(chunk)).ToList();
    }

    [Benchmark]
    public int Ip4RangeSet_Except()
    {
        int result = 0;
        for (int index = 0; index < Count; index++)
        {
            var set1 = rangeSets1[index];
            var set2 = rangeSets2[index];
            set1.Except(set2);
            result += set1.RangesCount;
        }

        return result;
    }
}