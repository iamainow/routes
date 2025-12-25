#pragma warning disable CA1822
#pragma warning disable CA1515
#pragma warning disable CA5394
#pragma warning disable CA2014
#pragma warning disable CA1707

using BenchmarkDotNet.Attributes;

namespace routes.Benchmarks.Ip4RangeSetStackAllocBenchmark;

[MemoryDiagnoser]
[ExceptionDiagnoser]
[Config(typeof(NoPowerPlanConfig))]
public class Ip4RangeSetStackAllocUnionExcept
{
    [Params(1_000, 10_000, 100_000)]
    public int Count { get; set; }

    private List<Ip4Range[]> rangeSets = [];

    [GlobalSetup]
    public async Task GlobalSetup()
    {
        Random random = new();
        List<Ip4Range> ranges = Enumerable.Range(0, Count * 10).Select(_ =>
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

        rangeSets = ranges.Chunk(10).ToList();
    }

    [Benchmark]
    public int Ip4RangeSetStackAlloc_SmartUnionUnorderedExceptUnsorted()
    {
        Ip4RangeSetStackAlloc result = new(stackalloc Ip4Range[1000]);
        for (int index = 1; index < Count; index += 2)
        {
            result.SmartUnionUnordered(rangeSets[index - 1]);
            result.ExceptUnsorted(rangeSets[index]);
        }

        return result.RangesCount;
    }
}