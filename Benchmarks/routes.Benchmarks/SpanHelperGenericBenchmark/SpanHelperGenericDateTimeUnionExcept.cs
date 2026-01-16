using BenchmarkDotNet.Attributes;
using routes.Extensions;
using routes.Generic;
using System.Text.RegularExpressions;

namespace routes.Benchmarks.SpanHelperBenchmark;

[Config(typeof(BenchmarkManualConfig))]
public partial class SpanHelperGenericDateTimeUnionExcept
{
    [Params(1_000)]
    public int Count { get; set; }

    [Params(10, 100, 1_000)]
    public int SetSize { get; set; }

    [Params("normalized", "sorted (overlap=0.25)", "sorted (overlap=0.5)", "sorted (overlap=0.75)", "unsorted (overlap=0.25)", "unsorted (overlap=0.5)", "unsorted (overlap=0.75)")]
    public required string Input { get; set; }

    private CustomRange<DateTimeWrapper>[][] rangesArray_1 = [];
    private CustomRange<DateTimeWrapper>[][] rangesArray_2 = [];

    [GeneratedRegex(@"overlap=([\d\.]+)")]
    private static partial Regex ParseOverlapPercent();

    private static CustomRange<DateTimeWrapper>[][] Generate(int count, int size, string input, Random random)
    {
        long minValue = DateTime.MinValue.Ticks;
        long maxValue = DateTime.MaxValue.Ticks;
        Func<ReadOnlySpan<byte>, DateTimeWrapper> convert = span => new DateTimeWrapper(DateTime.FromBinary(Math.Clamp(BitConverter.ToInt64(span), minValue, maxValue)));

        Func<CustomRange<DateTimeWrapper>[]> generator = input switch
        {
            "normalized" => () => CustomArrayExtensions.GenerateNormalized(size, convert, random),
            _ when input.StartsWith("sorted") => () =>
            {
                double overlappingPercent = ParseOverlapPercent().Match(input) is { Success: true } match
                    ? double.Parse(match.Groups[1].Value)
                    : throw new InvalidOperationException($"can't parse overlappingPercent in '{input}'");
                return CustomArrayExtensions.GenerateSorted(size, convert, overlappingPercent, random);
            }
            ,
            _ when input.StartsWith("unsorted") => () =>
            {
                double overlappingPercent = ParseOverlapPercent().Match(input) is { Success: true } match
                    ? double.Parse(match.Groups[1].Value)
                    : throw new InvalidOperationException($"can't parse overlappingPercent in '{input}'");
                return CustomArrayExtensions.GenerateSorted(size, convert, overlappingPercent, random);
            }
            ,
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
    public int SpanHelperGeneric_DateTime_Union_UnsortedUnsorted_Via_NormalizedNormalized()
    {
        var buffer = new CustomRange<DateTimeWrapper>[this.SetSize * 2];
        int result = 0;
        DateTimeWrapper one = new DateTimeWrapper(DateTime.FromBinary(1));
        for (int index = 0; index < this.Count; ++index)
        {
            result += SpanHelperGeneric.UnionUnsortedUnsortedViaNormalizedNormalized(
                this.rangesArray_1[index],
                this.rangesArray_2[index],
                buffer,
                one);
        }

        return result;
    }

    [Benchmark]
    public int SpanHelperGeneric_DateTime_Union_UnsortedUnsorted_Via_SortedSorted()
    {
        var buffer = new CustomRange<DateTimeWrapper>[this.SetSize * 2];
        int result = 0;
        DateTimeWrapper one = new DateTimeWrapper(DateTime.FromBinary(1));
        for (int index = 0; index < this.Count; ++index)
        {
            result += SpanHelperGeneric.UnionUnsortedUnsortedViaSortedSorted(
                this.rangesArray_1[index],
                this.rangesArray_2[index],
                buffer,
                one);
        }

        return result;
    }


}