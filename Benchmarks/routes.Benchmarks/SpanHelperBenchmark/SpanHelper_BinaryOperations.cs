using BenchmarkDotNet.Attributes;
using CommunityToolkit.HighPerformance.Buffers;
using routes.Extensions;

namespace routes.Benchmarks.SpanHelperBenchmark;

[Config(typeof(BenchmarkManualConfig))]
public class SpanHelper_BinaryOperations
{
    [Params(1_000)]
    public int Count { get; set; }

    [ParamsSource(nameof(SizesSource))]
    public required (int Size1, int Size2) Sizes { get; set; }

    public static IEnumerable<(int Size1, int Size2)> SizesSource
    {
        get
        {
            yield return (1, 1);
            yield return (100, 1);
            yield return (100, 100);
            yield return (10_000, 1);
            yield return (10_000, 100);
            yield return (10_000, 10_000);
        }
    }

    [Params(InputType.Normalized,
        InputType.Sorted_Overlapping_10,
        InputType.Sorted_Overlapping_20,
        InputType.Usorted_Overlapping_0,
        InputType.Usorted_Overlapping_10,
        InputType.Usorted_Overlapping_20)]
    public required InputType Input { get; set; }

    public InputTypeGeneral InputGeneral => InputTypeParser.Parse(Input).Item1;

    private Ip4Range[][] rangesArray_1 = [];
    private Ip4Range[][] rangesArray_2 = [];

    private static Ip4Range[][] Generate(int count, int size, InputType input, Random random)
    {
        Func<ReadOnlySpan<byte>, uint> convert = BitConverter.ToUInt32;

        Func<Ip4Range[]> generator = InputTypeParser.Parse(input) switch
        {
            (InputTypeGeneral.Normalized, _) => () => CustomArrayExtensions.GenerateNormalized(size, convert, random).Select(x => new Ip4Range(new Ip4Address(x.First), new Ip4Address(x.Last))).ToArray(),
            (InputTypeGeneral.Sorted, double overlappingPercent) => () => CustomArrayExtensions.GenerateSorted(size, convert, overlappingPercent, random).Select(x => new Ip4Range(new Ip4Address(x.First), new Ip4Address(x.Last))).ToArray(),
            (InputTypeGeneral.Unsorted, double overlappingPercent) => () => CustomArrayExtensions.GenerateUnsorted(size, convert, overlappingPercent, random).Select(x => new Ip4Range(new Ip4Address(x.First), new Ip4Address(x.Last))).ToArray(),
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
        this.rangesArray_1 = Generate(Count, Sizes.Size1, Input, random);
        this.rangesArray_2 = Generate(Count, Sizes.Size2, Input, random);
    }

    public static int Convert(Span<Ip4Range> span, InputTypeGeneral fromType, InputTypeGeneral toType)
    {
        switch (fromType, toType)
        {
            case (InputTypeGeneral.Unsorted, InputTypeGeneral.Sorted):
                {
                    SpanHelper.Sort(span);
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
        int result = 0;
        for (int index = 0; index < this.Count; ++index)
        {
            Span<Ip4Range> span1 = this.rangesArray_1[index];
            Span<Ip4Range> span2 = this.rangesArray_2[index];
            int length1 = SpanHelper.MakeNormalizedFromUnsorted(span1);
            int length2 = SpanHelper.MakeNormalizedFromUnsorted(span2);
            using var bufferSpanOwner = SpanOwner<Ip4Range>.Allocate(length1 + length2);
            var buffer = bufferSpanOwner.Span;
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
        int result = 0;
        for (int index = 0; index < this.Count; ++index)
        {
            Span<Ip4Range> span1 = this.rangesArray_1[index];
            Span<Ip4Range> span2 = this.rangesArray_2[index];
            SpanHelper.Sort(span1);
            SpanHelper.Sort(span2);
            using var bufferSpanOwner = SpanOwner<Ip4Range>.Allocate(span1.Length + span2.Length);
            var buffer = bufferSpanOwner.Span;
            result += SpanHelper.UnionSortedSorted(
                span1,
                span2,
                buffer);
        }

        return result;
    }

    [Benchmark]
    public int SpanHelper_Ip4Range_ExceptNormalizedSorted()
    {
        int result = 0;
        for (int index = 0; index < this.Count; ++index)
        {
            Span<Ip4Range> span1 = this.rangesArray_1[index];
            Span<Ip4Range> span2 = this.rangesArray_2[index];
            int length1 = SpanHelper.MakeNormalizedFromUnsorted(span1);
            SpanHelper.Sort(span2);
            using var bufferSpanOwner = SpanOwner<Ip4Range>.Allocate(length1 + span2.Length);
            var buffer = bufferSpanOwner.Span;
            result += SpanHelper.ExceptNormalizedSorted(
                span1[..length1],
                span2,
                buffer);
        }

        return result;
    }

    [Benchmark]
    public int SpanHelper_Ip4Range_ExceptNormalizedNormalized()
    {
        int result = 0;
        for (int index = 0; index < this.Count; ++index)
        {
            Span<Ip4Range> span1 = this.rangesArray_1[index];
            Span<Ip4Range> span2 = this.rangesArray_2[index];
            int length1 = SpanHelper.MakeNormalizedFromUnsorted(span1);
            int length2 = SpanHelper.MakeNormalizedFromUnsorted(span2);
            using var bufferSpanOwner = SpanOwner<Ip4Range>.Allocate(length1 + length2);
            var buffer = bufferSpanOwner.Span;
            result += SpanHelper.ExceptNormalizedSorted(
                span1[..length1],
                span2[..length2],
                buffer);
        }

        return result;
    }
}
