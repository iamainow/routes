using BenchmarkDotNet.Attributes;
using CommunityToolkit.HighPerformance.Buffers;
using routes.Extensions;
using routes.Generic;

namespace routes.Benchmarks.SpanHelperBenchmark;

[Config(typeof(BenchmarkManualConfig))]
public class SpanHelperGenericInt64UnionExcept
{
    [Params(1_000)]
    public int Count { get; set; }

    [Params(10, 100, 1_000)]
    public int SetSize { get; set; }

    [Params(InputType.Normalized, InputType.Sorted_Overlapping_25, InputType.Sorted_Overlapping_50, InputType.Sorted_Overlapping_75, InputType.Usorted_Overlapping_25, InputType.Usorted_Overlapping_50, InputType.Usorted_Overlapping_75)]
    public required InputType Input { get; set; }

    public InputTypeGeneral InputGeneral => InputTypeParser.Parse(Input).Item1;

    private CustomRange<long>[][] rangesArray_1 = [];
    private CustomRange<long>[][] rangesArray_2 = [];

    private static CustomRange<long>[][] Generate(int count, int size, InputType input, Random random)
    {
        long minValue = DateTime.MinValue.Ticks;
        long maxValue = DateTime.MaxValue.Ticks;
        Func<ReadOnlySpan<byte>, long> convert = span => Math.Clamp(BitConverter.ToInt64(span), minValue, maxValue);

        Func<CustomRange<long>[]> generator = InputTypeParser.Parse(input) switch
        {
            (InputTypeGeneral.Normalized, _) => () => CustomArrayExtensions.GenerateNormalized(size, convert, random),
            (InputTypeGeneral.Sorted, double overlappingPercent) => () => CustomArrayExtensions.GenerateSorted(size, convert, overlappingPercent, random),
            (InputTypeGeneral.Unsorted, double overlappingPercent) => () => CustomArrayExtensions.GenerateUnsorted(size, convert, overlappingPercent, random),
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

    [Benchmark]
    public int SpanHelperGeneric_DateTime_UnionNormalizedNormalized()
    {
        using var bufferSpanOwner = SpanOwner<CustomRange<long>>.Allocate(this.SetSize * 2);
        var buffer = bufferSpanOwner.Span;
        int result = 0;
        long one = 1;
        var fromType = InputGeneral;
        var toType = InputTypeGeneral.Normalized;
        for (int index = 0; index < this.Count; ++index)
        {
            Span<CustomRange<long>> span1 = this.rangesArray_1[index];
            Span<CustomRange<long>> span2 = this.rangesArray_2[index];
            int length1 = InputTypeParser.Convert(span1, one, fromType, toType);
            int length2 = InputTypeParser.Convert(span2, one, fromType, toType);
            result += SpanHelperGeneric.UnionNormalizedNormalized(
                span1[..length1],
                span2[..length2],
                buffer,
                one);
        }

        return result;
    }
}