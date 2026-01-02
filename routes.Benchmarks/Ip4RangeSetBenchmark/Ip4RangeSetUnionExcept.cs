using BenchmarkDotNet.Attributes;
using routes.Extensions;

namespace routes.Benchmarks.Ip4RangeSetBenchmark;

[Config(typeof(BenchmarkManualConfig))]
public class Ip4RangeSetUnionExcept
{
    [Params(1_000)]
    public int Count { get; set; }

    [Params(10, 100, 1_000)]
    public int SetSize { get; set; }

    private List<Ip4RangeSet> rangeSets_1 = [];
    private List<Ip4RangeSet> rangeSets_2 = [];
    private List<Ip4Range[]> rangesArray_2 = [];

    [GlobalSetup]
    public async Task GlobalSetup()
    {
        Random random = new();
        rangeSets_1 = Enumerable.Range(0, Count).Select(_ => Ip4RangeSet.Generate(SetSize, random)).ToList();
        rangeSets_2 = Enumerable.Range(0, Count).Select(_ => Ip4RangeSet.Generate(SetSize, random)).ToList();
        rangesArray_2 = rangeSets_2.Select(x => x.ToIp4Ranges()).ToList();
    }

    [Benchmark]
    public int Ip4RangeSet_Union_Ip4RangeSet()
    {
        int result = 0;
        for (int index = 0; index < Count; ++index)
        {
            rangeSets_1[index].Union(rangeSets_2[index]);
            result += rangeSets_1[index].RangesCount;
        }

        return result;
    }

    [Benchmark]
    public int Ip4RangeSet_Union_Ip4RangeArray()
    {
        int result = 0;
        for (int index = 0; index < Count; ++index)
        {
            rangeSets_1[index].Union(rangesArray_2[index]);
            result += rangeSets_1[index].RangesCount;
        }

        return result;
    }

    [Benchmark]
    public int Ip4RangeSet_Except_Ip4RangeSet()
    {
        int result = 0;
        for (int index = 0; index < Count; ++index)
        {
            rangeSets_1[index].Except(rangeSets_2[index]);
            result += rangeSets_1[index].RangesCount;
        }

        return result;
    }

    [Benchmark]
    public int Ip4RangeSet_Except_Ip4RangeArray()
    {
        int result = 0;
        for (int index = 0; index < Count; ++index)
        {
            var temp = new Ip4RangeSet(rangesArray_2[index]);
            rangeSets_1[index].Except(temp);
            result += rangeSets_1[index].RangesCount;
        }

        return result;
    }
}