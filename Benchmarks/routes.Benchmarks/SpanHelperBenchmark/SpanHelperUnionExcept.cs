using BenchmarkDotNet.Attributes;
using routes.Extensions;

namespace routes.Benchmarks.SpanHelperBenchmark;

[Config(typeof(BenchmarkManualConfig))]
public class SpanHelperUnionExcept
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
        rangesArray_1 = Enumerable.Range(0, Count).Select(_ => Ip4RangeArrayExtensions.Generate(SetSize, random)).ToArray();
        rangesArray_2 = Enumerable.Range(0, Count).Select(_ => Ip4RangeArrayExtensions.Generate(SetSize, random)).ToArray();
    }

    [Benchmark]
    public int SpanHelper_Union_UnsortedUnsorted_Via_NormalizedNormalized()
    {
        var buffer = new Ip4Range[SetSize * 2];
        int result = 0;
        for (int index = 0; index < Count; ++index)
        {
            result += routes.SpanHelper.UnionUnsortedUnsortedViaNormalizedNormalized(
                rangesArray_1[index],
                rangesArray_2[index],
                buffer);
        }

        return result;
    }

    [Benchmark]
    public int SpanHelper_Union_UnsortedUnsorted_Via_SortedSorted()
    {
        var buffer = new Ip4Range[SetSize * 2];
        int result = 0;
        for (int index = 0; index < Count; ++index)
        {
            result += routes.SpanHelper.UnionUnsortedUnsortedViaSortedSorted(
                rangesArray_1[index],
                rangesArray_2[index],
                buffer);
        }

        return result;
    }
}