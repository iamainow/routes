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

    private Ip4RangeSet[] rangeSets_1 = [];
    private Ip4RangeSet[] rangeSets_2 = [];
    private Ip4Range[][] rangesArray_2 = [];

    [GlobalSetup]
    public async Task GlobalSetup()
    {
        Random random = new();
        rangeSets_1 = Enumerable.Range(0, Count).Select(_ => Ip4RangeSet.Generate(SetSize, random)).ToArray();
        rangeSets_2 = Enumerable.Range(0, Count).Select(_ => Ip4RangeSet.Generate(SetSize, random)).ToArray();
        rangesArray_2 = rangeSets_2.Select(x => x.ToIp4Ranges()).ToArray();
    }

    [Benchmark]
    public int Ip4RangeSet_Union_set()
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
    public int Ip4RangeSet_Union_span()
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
    public int Ip4RangeSet_Except_set()
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
    public int Ip4RangeSet_Except_span()
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