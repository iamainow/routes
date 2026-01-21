
using BenchmarkDotNet.Attributes;
using CommunityToolkit.HighPerformance.Buffers;
using routes.Extensions;

namespace routes.Benchmarks.SpanHelperBenchmark;

[Config(typeof(BenchmarkManualConfig))]
public class SpanHelper_UnaryOperations
{
    [Params(1_000)]
    public int Count { get; set; }

    [Params(10, 100, 1_000, 10_000, 100_000)]
    public int SetSize { get; set; }

    [Params(InputType.Normalized,
        InputType.Sorted_Overlapping_10,
        InputType.Sorted_Overlapping_20,
        InputType.Usorted_Overlapping_0,
        InputType.Usorted_Overlapping_10,
        InputType.Usorted_Overlapping_20)]
    public required InputType Input { get; set; }

    public InputTypeGeneral InputGeneral => InputTypeParser.Parse(Input).Item1;

    private Ip4Range[][] rangesArray = [];

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
        this.rangesArray = Generate(Count, SetSize, Input, random);
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
    public int SpanHelper_Ip4Range_MakeNormalizedFromUnsorted()
    {
        int result = 0;
        for (int index = 0; index < this.Count; ++index)
        {
            result += SpanHelper.MakeNormalizedFromUnsorted(this.rangesArray[index]);
        }

        return result;
    }
}