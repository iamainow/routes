using BenchmarkDotNet.Attributes;
using routes.Extensions;

namespace routes.Benchmarks.Ip4RangeArrayBenchmark;

[Config(typeof(BenchmarkManualConfig))]
public class Ip4RangeArrayUnionExcept
{
    [Params(1_000)]
    public int Count { get; set; }

    [Params(10, 100, 1_000)]
    public int SetSize { get; set; }

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
        this.rangesArray_1 = Generate(Count, SetSize, Input, random);
        this.rangesArray_2 = Generate(Count, SetSize, Input, random);
    }

    [Benchmark]
    public int Ip4RangeArray_Create_Union()
    {
        int result = 0;
        for (int index = 0; index < Count; ++index)
        {
            result += Ip4RangeArray.Create(rangesArray_1[index])
                .Union(rangesArray_2[index])
                .RangesCount;
        }

        return result;
    }

    [Benchmark]
    public int Ip4RangeArray_Create_Except()
    {
        int result = 0;
        for (int index = 0; index < Count; ++index)
        {
            result += Ip4RangeArray.Create(rangesArray_1[index])
                .Except(rangesArray_2[index])
                .RangesCount;
        }

        return result;
    }
}