
using BenchmarkDotNet.Attributes;
using routes.Extensions;
using routes.Generic;
using System.Numerics;

namespace routes.Benchmarks.SpanHelperBenchmark;

[Config(typeof(BenchmarkManualConfig))]
public class SpanHelperGeneric_UnaryOperations
{
    [Params(1_000)]
    public int Count { get; set; }

    [Params(10, 100, 1_000, 10_000)]
    public int SetSize { get; set; }

    [Params(
        InputType.Normalized,
        InputType.Sorted_Overlapping_10,
        InputType.Sorted_Overlapping_20,
        InputType.Usorted_Overlapping_0,
        InputType.Usorted_Overlapping_10,
        InputType.Usorted_Overlapping_20)]
    public required InputType Input { get; set; }

    public InputTypeGeneral InputGeneral => InputTypeParser.Parse(Input).Item1;

    private CustomRange<Ip4Address>[][] rangesArray = [];

    private static CustomRange<Ip4Address>[][] Generate(int count, int size, InputType input, Random random)
    {
        Func<ReadOnlySpan<byte>, uint> convert = BitConverter.ToUInt32;

        Func<CustomRange<Ip4Address>[]> generator = InputTypeParser.Parse(input) switch
        {
            (InputTypeGeneral.Normalized, _) => () => CustomArrayExtensions.GenerateNormalized(size, convert, random).Select(x => new CustomRange<Ip4Address>(new Ip4Address(x.FirstAddress), new Ip4Address(x.LastAddress))).ToArray(),
            (InputTypeGeneral.Sorted, double overlappingPercent) => () => CustomArrayExtensions.GenerateSorted(size, convert, overlappingPercent, random).Select(x => new CustomRange<Ip4Address>(new Ip4Address(x.FirstAddress), new Ip4Address(x.LastAddress))).ToArray(),
            (InputTypeGeneral.Unsorted, double overlappingPercent) => () => CustomArrayExtensions.GenerateUnsorted(size, convert, overlappingPercent, random).Select(x => new CustomRange<Ip4Address>(new Ip4Address(x.FirstAddress), new Ip4Address(x.LastAddress))).ToArray(),
            _ => throw new NotImplementedException($"Input='{input}' is not implemented"),
        };

        return Enumerable.Range(0, count)
            .Select(_ => generator().ToArray())
            .ToArray();
    }

    [GlobalSetup]
    public async Task GlobalSetup()
    {
        Random random = new(42);
        this.rangesArray = Generate(Count, SetSize, Input, random);
    }

    public static int Convert<T>(Span<CustomRange<T>> span, T one, InputTypeGeneral fromType, InputTypeGeneral toType)
        where T : struct, IEquatable<T>, IComparable<T>, IMinMaxValue<T>, IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>
    {
        switch (fromType, toType)
        {
            case (InputTypeGeneral.Unsorted, InputTypeGeneral.Sorted):
                {
                    SpanHelperGeneric.Sort(span);
                    return span.Length;
                }
            case (InputTypeGeneral.Unsorted, InputTypeGeneral.Normalized):
                {
                    return SpanHelperGeneric.MakeNormalizedFromUnsorted(span, one);
                }
            case (InputTypeGeneral.Sorted, InputTypeGeneral.Normalized):
                {
                    return SpanHelperGeneric.MakeNormalizedFromSorted(span, one);
                }
            default: return span.Length;
        }
    }

    [Benchmark]
    public int SpanHelperGeneric_Ip4Address_MakeNormalizedFromUnsorted()
    {
        Ip4Address one = new Ip4Address(1);
        int result = 0;
        for (int index = 0; index < this.Count; ++index)
        {
            result += SpanHelperGeneric.MakeNormalizedFromUnsorted(this.rangesArray[index], one);
        }

        return result;
    }

    [Benchmark]
    public int SpanHelperGeneric_Ip4Address_MakeNormalizedFromUnsorted2()
    {
        int result = 0;
        for (int index = 0; index < this.Count; ++index)
        {
            result += SpanHelperGeneric.MakeNormalizedFromUnsorted2(this.rangesArray[index]);
        }

        return result;
    }

    [Benchmark]
    public int SpanHelperGeneric_Ip4Address_Sort()
    {
        int result = 0;
        for (int index = 0; index < this.Count; ++index)
        {
            SpanHelperGeneric.Sort(this.rangesArray[index]);
            result ^= this.rangesArray[index][0].GetHashCode(); // Prevent optimization
        }

        return result;
    }

    [Benchmark]
    public int SpanHelperGeneric_Ip4Address_Sort2()
    {
        int result = 0;
        for (int index = 0; index < this.Count; ++index)
        {
            SpanHelperGeneric.Sort2(this.rangesArray[index]);
            result ^= this.rangesArray[index][0].GetHashCode(); // Prevent optimization
        }

        return result;
    }

    [Benchmark]
    public int SpanHelperGeneric_Ip4Address_Sort3()
    {
        int result = 0;
        for (int index = 0; index < this.Count; ++index)
        {
            this.rangesArray[index].Sort(CustomRangeComparer<Ip4Address>.Instance);
            result ^= this.rangesArray[index][0].GetHashCode(); // Prevent optimization
        }

        return result;
    }
}