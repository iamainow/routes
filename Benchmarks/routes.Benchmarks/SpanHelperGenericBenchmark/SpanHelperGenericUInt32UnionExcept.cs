using BenchmarkDotNet.Attributes;
using routes.Extensions;
using routes.Generic;

namespace routes.Benchmarks.SpanHelperBenchmark;

[Config(typeof(BenchmarkManualConfig))]
public class SpanHelperGenericUInt32UnionExcept
{
    [Params(1_000)]
    public int Count { get; set; }

    [Params(10, 100, 1_000)]
    public int SetSize { get; set; }

    private CustomRange<uint>[][] rangesArray_1 = [];
    private CustomRange<uint>[][] rangesArray_2 = [];

    [GlobalSetup]
    public async Task GlobalSetup()
    {
        Random random = new();
        this.rangesArray_1 = Enumerable.Range(0, this.Count).Select(_ => Ip4RangeArrayExtensions.Generate(this.SetSize, random).Select(x => new CustomRange<uint>(x.FirstAddress.ToUInt32(), x.LastAddress.ToUInt32())).ToArray()).ToArray();
        this.rangesArray_2 = Enumerable.Range(0, this.Count).Select(_ => Ip4RangeArrayExtensions.Generate(this.SetSize, random).Select(x => new CustomRange<uint>(x.FirstAddress.ToUInt32(), x.LastAddress.ToUInt32())).ToArray()).ToArray();
    }

    [Benchmark]
    public int SpanHelperGeneric_UInt32_Union_UnsortedUnsorted_Via_NormalizedNormalized()
    {
        var buffer = new CustomRange<uint>[this.SetSize * 2];
        int result = 0;
        for (int index = 0; index < this.Count; ++index)
        {
            result += SpanHelperGeneric.UnionUnsortedUnsortedViaNormalizedNormalized(
                this.rangesArray_1[index],
                this.rangesArray_2[index],
                buffer,
                1U);
        }

        return result;
    }

    [Benchmark]
    public int SpanHelperGeneric_UInt32_Union_UnsortedUnsorted_Via_SortedSorted()
    {
        var buffer = new CustomRange<uint>[this.SetSize * 2];
        int result = 0;
        for (int index = 0; index < this.Count; ++index)
        {
            result += SpanHelperGeneric.UnionUnsortedUnsortedViaSortedSorted(
                this.rangesArray_1[index],
                this.rangesArray_2[index],
                buffer,
                1U);
        }

        return result;
    }
}
