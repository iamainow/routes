using BenchmarkDotNet.Attributes;
using routes.Extensions;

namespace routes.Benchmarks.SpanHelperBenchmark;

[Config(typeof(BenchmarkManualConfig))]
public class SpanHelperGenericDateTimeUnionExcept
{
    [Params(1_000)]
    public int Count { get; set; }

    [Params(10, 100, 1_000)]
    public int SetSize { get; set; }

    private CustomRange<DateTimeWrapper>[][] rangesArray_1 = [];
    private CustomRange<DateTimeWrapper>[][] rangesArray_2 = [];

    private static T Clamp<T>(T value, T minValue, T maxValue)
        where T : IComparable<T>
    {
        if (value.CompareTo(minValue) < 0)
        {
            return minValue;
        }
        if (value.CompareTo(maxValue) > 0)
        {
            return maxValue;
        }
        return value;
    }

    [GlobalSetup]
    public async Task GlobalSetup()
    {
        Random random = new();
        long minValue = DateTime.MinValue.Ticks;
        long maxValue = DateTime.MaxValue.Ticks;
        this.rangesArray_1 = Enumerable.Range(0, this.Count)
            .Select(_ => CustomArrayExtensions.GenerateNormalized(
                this.SetSize,
                span => new DateTimeWrapper(DateTime.FromBinary(Clamp(BitConverter.ToInt64(span), minValue, maxValue))),
                random
            ).ToArray())
        .ToArray();
        this.rangesArray_2 = Enumerable.Range(0, this.Count)
            .Select(_ => CustomArrayExtensions.GenerateNormalized(
                this.SetSize,
                span => new DateTimeWrapper(DateTime.FromBinary(Clamp(BitConverter.ToInt64(span), minValue, maxValue))),
                random
            ).ToArray())
        .ToArray();
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