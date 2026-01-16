using BenchmarkDotNet.Attributes;
using CommunityToolkit.HighPerformance.Buffers;
using routes.Extensions;
using routes.Generic;
using System.Numerics;
using System.Text.RegularExpressions;

namespace routes.Benchmarks.SpanHelperBenchmark;

public enum InputType
{
    Normalized,
    Sorted_Overlapping_25,
    Sorted_Overlapping_50,
    Sorted_Overlapping_75,
    Usorted_Overlapping_25,
    Usorted_Overlapping_50,
    Usorted_Overlapping_75,
}

public enum InputTypeGeneral
{
    Normalized,
    Sorted,
    Unsorted,
}

public static class InputTypeParser
{
    public static (InputTypeGeneral, double) Parse(InputType inputType)
    {
        return inputType switch
        {
            InputType.Normalized => (InputTypeGeneral.Normalized, default),

            InputType.Sorted_Overlapping_25 => (InputTypeGeneral.Sorted, 0.25),
            InputType.Sorted_Overlapping_50 => (InputTypeGeneral.Sorted, 0.5),
            InputType.Sorted_Overlapping_75 => (InputTypeGeneral.Sorted, 0.75),

            InputType.Usorted_Overlapping_25 => (InputTypeGeneral.Unsorted, 0.25),
            InputType.Usorted_Overlapping_50 => (InputTypeGeneral.Unsorted, 0.5),
            InputType.Usorted_Overlapping_75 => (InputTypeGeneral.Unsorted, 0.75),
            _ => throw new NotImplementedException(),
        };
    }

    public static int Convert<T, TOne>(Span<CustomRange<T>> span, TOne one, InputTypeGeneral fromType, InputTypeGeneral toType)
        where T : struct, IEquatable<T>, IComparable<T>, IMinMaxValue<T>, IAdditionOperators<T, TOne, T>, ISubtractionOperators<T, TOne, T>
    {
        if (fromType == toType) return span.Length;
        if (toType == InputTypeGeneral.Unsorted) return span.Length;
        if (fromType == InputTypeGeneral.Normalized && toType == InputTypeGeneral.Sorted) return span.Length;
        if (fromType == InputTypeGeneral.Unsorted && toType == InputTypeGeneral.Sorted)
        {
            span.Sort(CustomRangeComparer<T>.Instance);
            return span.Length;
        }
        if (fromType == InputTypeGeneral.Unsorted && toType == InputTypeGeneral.Normalized) return SpanHelperGeneric.MakeNormalizedFromUnsorted(span, one);
        if (fromType == InputTypeGeneral.Sorted && toType == InputTypeGeneral.Normalized) return SpanHelperGeneric.MakeNormalizedFromSorted(span, one);
        throw new NotImplementedException();
    }
}

[Config(typeof(BenchmarkManualConfig))]
public class SpanHelperGenericDateTimeUnionExcept
{
    [Params(1_000)]
    public int Count { get; set; }

    [Params(10, 100, 1_000)]
    public int SetSize { get; set; }

    [Params(InputType.Normalized, InputType.Sorted_Overlapping_25, InputType.Sorted_Overlapping_50, InputType.Sorted_Overlapping_75, InputType.Usorted_Overlapping_25, InputType.Usorted_Overlapping_50, InputType.Usorted_Overlapping_75)]
    public required InputType Input { get; set; }

    public InputTypeGeneral InputGeneral => InputTypeParser.Parse(Input).Item1;

    private CustomRange<DateTimeWrapper>[][] rangesArray_1 = [];
    private CustomRange<DateTimeWrapper>[][] rangesArray_2 = [];

    private static CustomRange<DateTimeWrapper>[][] Generate(int count, int size, InputType input, Random random)
    {
        long minValue = DateTime.MinValue.Ticks;
        long maxValue = DateTime.MaxValue.Ticks;
        Func<ReadOnlySpan<byte>, DateTimeWrapper> convert = span => new DateTimeWrapper(DateTime.FromBinary(Math.Clamp(BitConverter.ToInt64(span), minValue, maxValue)));

        Func<CustomRange<DateTimeWrapper>[]> generator = InputTypeParser.Parse(input) switch
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
        using var bufferSpanOwner = SpanOwner<CustomRange<DateTimeWrapper>>.Allocate(this.SetSize * 2);
        var buffer = bufferSpanOwner.Span;
        int result = 0;
        DateTimeWrapper one = new DateTimeWrapper(DateTime.FromBinary(1));
        var fromType = InputGeneral;
        var toType = InputTypeGeneral.Normalized;
        for (int index = 0; index < this.Count; ++index)
        {
            Span<CustomRange<DateTimeWrapper>> span1 = this.rangesArray_1[index];
            Span<CustomRange<DateTimeWrapper>> span2 = this.rangesArray_2[index];
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

    [Benchmark]
    public int SpanHelperGeneric_DateTime_UnionSortedSorted()
    {
        using var bufferSpanOwner = SpanOwner<CustomRange<DateTimeWrapper>>.Allocate(this.SetSize * 2);
        var buffer = bufferSpanOwner.Span;
        int result = 0;
        DateTimeWrapper one = new DateTimeWrapper(DateTime.FromBinary(1));
        var fromType = InputGeneral;
        var toType = InputTypeGeneral.Sorted;
        for (int index = 0; index < this.Count; ++index)
        {
            Span<CustomRange<DateTimeWrapper>> span1 = this.rangesArray_1[index];
            Span<CustomRange<DateTimeWrapper>> span2 = this.rangesArray_2[index];
            int length1 = InputTypeParser.Convert(span1, one, fromType, toType);
            int length2 = InputTypeParser.Convert(span2, one, fromType, toType);
            result += SpanHelperGeneric.UnionSortedSorted(
                span1[..length1],
                span2[..length2],
                buffer,
                one);
        }

        return result;
    }


}