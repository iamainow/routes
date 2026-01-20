using BenchmarkDotNet.Attributes;
using CommunityToolkit.HighPerformance.Buffers;
using routes.Extensions;

namespace routes.Benchmarks.SpanHelperBenchmark;

[Config(typeof(BenchmarkManualConfig))]
public class SpanHelperUnionExcept
{
    [Params(1_000)]
    public int Count { get; set; }

    [Params(10, 100, 1_000)]
    public int SetSize { get; set; }

    [Params(InputType.Normalized, InputType.Sorted_Overlapping_25, InputType.Sorted_Overlapping_50, InputType.Sorted_Overlapping_75, InputType.Usorted_Overlapping_25, InputType.Usorted_Overlapping_50, InputType.Usorted_Overlapping_75)]
    public required InputType Input { get; set; }

    public InputTypeGeneral InputGeneral => InputTypeParser.Parse(Input).Item1;

    private Ip4Range[][] rangesArray_1 = [];
    private Ip4Range[][] rangesArray_2 = [];

    private static Ip4Range[][] Generate(int count, int size, InputType input, Random random)
    {
        Func<ReadOnlySpan<byte>, uint> convert = BitConverter.ToUInt32;

        Func<Ip4Range[]> generator = InputTypeParser.Parse(input) switch
        {
            (InputTypeGeneral.Normalized, _) => () => CustomArrayExtensions.GenerateNormalized(size, convert, random).Select(x => new Ip4Range(new Ip4Address(x.FirstAddress), new Ip4Address(x.LastAddress))).ToArray(),
            (InputTypeGeneral.Sorted, double overlappingPercent) => () => CustomArrayExtensions.GenerateSorted(size, convert, overlappingPercent, random).Select(x => new Ip4Range(new Ip4Address(x.FirstAddress), new Ip4Address(x.LastAddress))).ToArray(),
            (InputTypeGeneral.Unsorted, double overlappingPercent) => () => CustomArrayExtensions.GenerateUnsorted(size, convert, overlappingPercent, random).Select(x => new Ip4Range(new Ip4Address(x.FirstAddress), new Ip4Address(x.LastAddress))).ToArray(),
            _ => throw new NotImplementedException($"Input='{input}' is not implemented"),
        };

        return Enumerable.Range(0, count)
            .Select(_ => generator().ToArray())
            .ToArray();
    }

    [GlobalSetup]
    public async Task GlobalSetup()
    {
        Random random = new();
        this.rangesArray_1 = Generate(Count, SetSize, Input, random);
        this.rangesArray_2 = Generate(Count, SetSize, Input, random);
    }

    public static int Convert(Span<Ip4Range> span, InputTypeGeneral fromType, InputTypeGeneral toType)
    {
        switch (fromType, toType)
        {
            case (InputTypeGeneral.Unsorted, InputTypeGeneral.Sorted):
                {
                    span.Sort(Ip4RangeComparer.Instance);
                    return span.Length;
                }
            case (InputTypeGeneral.Unsorted, InputTypeGeneral.Normalized):
                {
                    return SpanHelper.MakeNormalizedFromUnsorted(span);
                }
            case (InputTypeGeneral.Sorted, InputTypeGeneral.Normalized):
                {
                    return SpanHelper.MakeNormalizedFromSorted(span);
                }
            default: return span.Length;
        }
    }


    [Benchmark]
    public int SpanHelper_Ip4Range_UnionNormalizedNormalized()
    {
        using var bufferSpanOwner = SpanOwner<Ip4Range>.Allocate(this.SetSize * 2);
        var buffer = bufferSpanOwner.Span;
        int result = 0;
        var fromType = InputGeneral;
        for (int index = 0; index < this.Count; ++index)
        {
            Span<Ip4Range> span1 = this.rangesArray_1[index];
            Span<Ip4Range> span2 = this.rangesArray_2[index];
            int length1 = Convert(span1, fromType, InputTypeGeneral.Normalized);
            int length2 = Convert(span2, fromType, InputTypeGeneral.Normalized);
            result += SpanHelper.UnionNormalizedNormalized(
                span1[..length1],
                span2[..length2],
                buffer);
        }

        return result;
    }

    [Benchmark]
    public int SpanHelper_Ip4Range_UnionSortedSorted()
    {
        using var bufferSpanOwner = SpanOwner<Ip4Range>.Allocate(this.SetSize * 2);
        var buffer = bufferSpanOwner.Span;
        int result = 0;
        var fromType = InputGeneral;
        for (int index = 0; index < this.Count; ++index)
        {
            Span<Ip4Range> span1 = this.rangesArray_1[index];
            Span<Ip4Range> span2 = this.rangesArray_2[index];
            int length1 = Convert(span1, fromType, InputTypeGeneral.Sorted);
            int length2 = Convert(span2, fromType, InputTypeGeneral.Sorted);
            result += SpanHelper.UnionSortedSorted(
                span1[..length1],
                span2[..length2],
                buffer);
        }

        return result;
    }

    [Benchmark]
    public int SpanHelper_Ip4Range_ExceptNormalizedSorted()
    {
        using var bufferSpanOwner = SpanOwner<Ip4Range>.Allocate(this.SetSize * 2);
        var buffer = bufferSpanOwner.Span;
        int result = 0;
        var fromType = InputGeneral;
        for (int index = 0; index < this.Count; ++index)
        {
            Span<Ip4Range> span1 = this.rangesArray_1[index];
            Span<Ip4Range> span2 = this.rangesArray_2[index];
            int length1 = Convert(span1, fromType, InputTypeGeneral.Normalized);
            int length2 = Convert(span2, fromType, InputTypeGeneral.Sorted);
            result += SpanHelper.ExceptNormalizedSorted(
                span1[..length1],
                span2[..length2],
                buffer);
        }

        return result;
    }

    [Benchmark]
    public int SpanHelper_Ip4Range_ExceptNormalizedNormalized()
    {
        using var bufferSpanOwner = SpanOwner<Ip4Range>.Allocate(this.SetSize * 2);
        var buffer = bufferSpanOwner.Span;
        int result = 0;
        var fromType = InputGeneral;
        for (int index = 0; index < this.Count; ++index)
        {
            Span<Ip4Range> span1 = this.rangesArray_1[index];
            Span<Ip4Range> span2 = this.rangesArray_2[index];
            int length1 = Convert(span1, fromType, InputTypeGeneral.Normalized);
            int length2 = Convert(span2, fromType, InputTypeGeneral.Normalized);
            result += SpanHelper.ExceptNormalizedSorted(
                span1[..length1],
                span2[..length2],
                buffer);
        }

        return result;
    }
}