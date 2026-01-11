using BenchmarkDotNet.Attributes;
using routes.Extensions;

namespace routes.Benchmarks.Ip4RangeSetStackAllocBenchmark;

[Config(typeof(BenchmarkManualConfig))]
public class Ip4RangeArrayUnionExcept
{
    [Params(1_000)]
    public int Count { get; set; }

    [Params(10, 100, 1_000)]
    public int SetSize { get; set; }

    private Ip4Range[][] rangesArray_1 = [];
    private Ip4Range[][] rangesArray_2 = [];

    [GlobalSetup]
    public async Task GlobalSetup()
    {
        Random random = new();
        rangesArray_1 = Enumerable.Range(0, Count).Select(_ => Ip4RangeSet.Generate(SetSize, random)).Select(x => x.ToIp4Ranges()).ToArray();
        rangesArray_2 = Enumerable.Range(0, Count).Select(_ => Ip4RangeSet.Generate(SetSize, random)).Select(x => x.ToIp4Ranges()).ToArray();
    }

    [Benchmark]
    public int Ip4RangeArray_Create_Union_span()
    {
        int result = 0;
        for (int index = 0; index < Count; ++index)
        {
            result += Ip4RangeArray.Create(rangesArray_1[index])
                .Union(rangesArray_2[index])
                .RangesCount;
        }

        return result;
    }

    [Benchmark]
    public int Ip4RangeArray_Create_Except_span()
    {
        int result = 0;
        for (int index = 0; index < Count; ++index)
        {
            result += Ip4RangeArray.Create(rangesArray_1[index])
                .Except(rangesArray_2[index])
                .RangesCount;
        }

        return result;
    }
}