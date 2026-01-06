using BenchmarkDotNet.Attributes;
using routes.Extensions;

namespace routes.Benchmarks.Ip4RangeReadonlySpanBenchmark;

[Config(typeof(BenchmarkManualConfig))]
public class Ip4RangeReadonlySpanUnionExcept
{
    [Params(1_000)]
    public int Count { get; set; }

    [Params(10, 100, 1_000)]
    public int SetSize { get; set; }

    private List<Ip4Range[]> rangesArray_1 = [];
    private List<Ip4Range[]> rangesArray_2 = [];

    [GlobalSetup]
    public async Task GlobalSetup()
    {
        Random random = new();
        rangesArray_1 = Enumerable.Range(0, Count).Select(_ => Ip4RangeSet.Generate(SetSize, random)).Select(x => x.ToIp4Ranges()).ToList();
        rangesArray_2 = Enumerable.Range(0, Count).Select(_ => Ip4RangeSet.Generate(SetSize, random)).Select(x => x.ToIp4Ranges()).ToList();
    }

    [Benchmark]
    public int Ip4RangeReadonlySpan_Union_Union()
    {
        Span<Ip4Range> s0 = stackalloc Ip4Range[SetSize];
        Span<Ip4Range> s1 = stackalloc Ip4Range[SetSize * 2];
        int result = 0;
        for (int index = 0; index < Count; ++index)
        {
            var resultInit = new Ip4RangeReadonlySpan();

            int l0 = resultInit.Union(rangesArray_1[index], s0);
            var result0 = new Ip4RangeReadonlySpan(s0[..l0]);

            int l1 = result0.Union(rangesArray_2[index], s1);
            var result1 = new Ip4RangeReadonlySpan(s1[..l1]);

            result += result1.RangesCount;
        }

        return result;
    }

    [Benchmark]
    public int Ip4RangeReadonlySpan_Union_Except()
    {
        Span<Ip4Range> s0 = stackalloc Ip4Range[SetSize];
        Span<Ip4Range> s1 = stackalloc Ip4Range[SetSize * 2];
        int result = 0;
        for (int index = 0; index < Count; ++index)
        {
            var resultInit = new Ip4RangeReadonlySpan();

            int l0 = resultInit.Union(rangesArray_1[index], s0);
            var result0 = new Ip4RangeReadonlySpan(s0[..l0]);

            int l1 = result0.Except(rangesArray_2[index], s1);
            var result1 = new Ip4RangeReadonlySpan(s1[..l1]);

            result += result1.RangesCount;
        }

        return result;
    }
}