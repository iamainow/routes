using BenchmarkDotNet.Attributes;

namespace routes.Benchmarks.Ip4RangeSetBenchmark;

[MemoryDiagnoser]
[ExceptionDiagnoser]
[Config(typeof(NoPowerPlanConfig))]
public class Ip4RangeSetUnionExcept
{
    [Params(1_000)]
    public int Count { get; set; }

    [Params(10, 100, 1_000)]
    public int SetSize { get; set; }

    private List<Ip4RangeSet> rangeSets1 = [];

    private List<Ip4Range[]> ranges3 = [];

    [GlobalSetup]
    public async Task GlobalSetup()
    {
        Random random = new();
        List<Ip4Range> ranges = Enumerable.Range(0, (Count + 1) * SetSize).Select(_ =>
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

        ranges3 = ranges.Chunk(SetSize).ToList();

        rangeSets1 = ranges3.Take(Count).Select(chunk => new Ip4RangeSet(chunk)).ToList();
    }

    [Benchmark]
    public Ip4RangeSet Ip4RangeSet_UnionExcept_Set()
    {
        Ip4RangeSet result = new();
        for (int index = 1; index < Count; index += 2)
        {
            result.Union(rangeSets1[index - 1]);
            result.Except(rangeSets1[index]);
        }

        return result;
    }
}