using BenchmarkDotNet.Attributes;

namespace routes.Benchmarks.Ip4RangeSetStackAllocBenchmark;

[MemoryDiagnoser]
[ExceptionDiagnoser(false)]
[Config(typeof(NoPowerPlanConfig))]
public class Ip4RangeSetStackAllocUnionExcept
{
    [Params(1_000)]
    public int Count { get; set; }

    [Params(10, 100, 1_000)]
    public int SetSize { get; set; }

    private List<Ip4Range[]> rangesArray_readonly_1 = [];
    private List<Ip4Range[]> rangesArray_readonly_2 = [];
    private List<Ip4Range[]> rangesArray_1 = [];
    private List<Ip4Range[]> rangesArray_2 = [];

    [GlobalSetup]
    public async Task GlobalSetup()
    {
        Random random = new();
        rangesArray_readonly_1 = Enumerable.Range(0, Count).Select(_ => Ip4RangeSet.Generate(SetSize, random)).Select(x => x.ToIp4Ranges()).ToList();
        rangesArray_readonly_2 = Enumerable.Range(0, Count).Select(_ => Ip4RangeSet.Generate(SetSize, random)).Select(x => x.ToIp4Ranges()).ToList();
        rangesArray_1 = Enumerable.Range(0, Count).Select(_ => Ip4RangeSet.Generate(SetSize, random)).Select(x => x.ToIp4Ranges()).ToList();
        rangesArray_2 = Enumerable.Range(0, Count).Select(_ => Ip4RangeSet.Generate(SetSize, random)).Select(x => x.ToIp4Ranges()).ToList();
    }

    [Benchmark]
    public int Ip4RangeSetStackAlloc_ctor_Union1_span()
    {
        Span<Ip4Range> span = stackalloc Ip4Range[SetSize * 2];
        int result = 0;
        for (int index = 0; index < Count; ++index)
        {
            var set = new Ip4RangeSetStackAlloc(span, rangesArray_readonly_1[index].AsSpan());
            set.Union1(rangesArray_readonly_2[index].AsSpan());
            result += set.RangesCount;
        }

        return result;
    }

    [Benchmark]
    public int Ip4RangeSetStackAlloc_ctor_Union2_span()
    {
        Span<Ip4Range> span = stackalloc Ip4Range[SetSize * 2];
        int result = 0;
        for (int index = 0; index < Count; ++index)
        {
            var set = new Ip4RangeSetStackAlloc(span, rangesArray_readonly_1[index].AsSpan());
            set.Union2ModifySpan(rangesArray_1[index].AsSpan());
            result += set.RangesCount;
        }

        return result;
    }

    [Benchmark]
    public int Ip4RangeSetStackAlloc_ctor_ExceptUnsorted_span()
    {
        Span<Ip4Range> span = stackalloc Ip4Range[SetSize * 2];
        int result = 0;
        for (int index = 0; index < Count; ++index)
        {
            var set = new Ip4RangeSetStackAlloc(span, rangesArray_readonly_1[index].AsSpan());
            set.ExceptUnsortedModifySpan(rangesArray_2[index].AsSpan());
            result += set.RangesCount;
        }

        return result;
    }
}