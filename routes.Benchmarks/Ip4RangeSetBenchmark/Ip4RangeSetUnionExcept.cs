using BenchmarkDotNet.Attributes;

namespace routes.Benchmarks.Ip4RangeSetBenchmark;

[MemoryDiagnoser]
[ExceptionDiagnoser(false)]
[Config(typeof(NoPowerPlanConfig))]
public class Ip4RangeSetUnionExcept
{
    [Params(1_000)]
    public int Count { get; set; }

    [Params(10, 100, 1_000, 10_000)]
    public int SetSize { get; set; }

    private List<Ip4RangeSet> rangeSets1 = [];

    private List<Ip4Range[]> rangesArray1 = [];

    [GlobalSetup]
    public async Task GlobalSetup()
    {
        Random random = new();
        rangeSets1 = Enumerable.Range(0, Count).Select(_ => Ip4RangeSet.Generate(SetSize, random)).ToList();
        rangesArray1 = rangeSets1.Select(x => x.ToIp4Ranges()).ToList();
    }

    [Benchmark]
    public int Ip4RangeSet_UnionExcept_Set()
    {
        Ip4RangeSet result = new();
        for (int index = 0; index < Count - 1; index += 2)
        {
            result.Union(rangeSets1[index]);
            result.Except(rangeSets1[index + 1]);
        }

        return result.RangesCount;
    }

    [Benchmark]
    public int Ip4RangeSet_UnionExcept_Ip4RangeArray()
    {
        Ip4RangeSet result = new();
        for (int index = 0; index < Count - 1; index += 2)
        {
            result.Union(rangesArray1[index]);
            result.Except(rangeSets1[index + 1]);
        }

        return result.RangesCount;
    }
}