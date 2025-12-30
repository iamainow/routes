using BenchmarkDotNet.Attributes;

namespace routes.Benchmarks.Ip4RangeSetBenchmark;

[MemoryDiagnoser]
[ExceptionDiagnoser(false)]
[Config(typeof(NoPowerPlanConfig))]
public class Ip4RangeSetUnionExcept
{
    [Params(1_000)]
    public int Count { get; set; }

    [Params(10, 100, 1_000)]
    public int SetSize { get; set; }

    private List<Ip4RangeSet> rangeSets_union_set = [];
    private List<Ip4RangeSet> rangeSets_union_array = [];
    private List<Ip4RangeSet> rangeSets_except_set = [];
    private List<Ip4RangeSet> rangeSets_except_array = [];
    private List<Ip4RangeSet> rangeSets_readonly = [];
    private List<Ip4Range[]> rangesArray_readonly = [];

    [GlobalSetup]
    public async Task GlobalSetup()
    {
        Random random = new();
        rangeSets_union_set = Enumerable.Range(0, Count).Select(_ => Ip4RangeSet.Generate(SetSize, random)).ToList();
        rangeSets_union_array = Enumerable.Range(0, Count).Select(_ => Ip4RangeSet.Generate(SetSize, random)).ToList();
        rangeSets_except_set = Enumerable.Range(0, Count).Select(_ => Ip4RangeSet.Generate(SetSize, random)).ToList();
        rangeSets_except_array = Enumerable.Range(0, Count).Select(_ => Ip4RangeSet.Generate(SetSize, random)).ToList();
        rangeSets_readonly = Enumerable.Range(0, Count).Select(_ => Ip4RangeSet.Generate(SetSize, random)).ToList();
        rangesArray_readonly = rangeSets_readonly.Select(x => x.ToIp4Ranges()).ToList();
    }

    [Benchmark]
    public int Ip4RangeSet_Union_Ip4RangeSet()
    {
        int result = 0;
        for (int index = 0; index < Count; ++index)
        {
            rangeSets_union_set[index].Union(rangeSets_readonly[index]);
            result += rangeSets_union_set[index].RangesCount;
        }

        return result;
    }

    [Benchmark]
    public int Ip4RangeSet_Union_Ip4RangeArray()
    {
        int result = 0;
        for (int index = 0; index < Count; ++index)
        {
            rangeSets_union_array[index].Union(rangesArray_readonly[index]);
            result += rangeSets_union_array[index].RangesCount;
        }

        return result;
    }

    [Benchmark]
    public int Ip4RangeSet_Except_Ip4RangeSet()
    {
        int result = 0;
        for (int index = 0; index < Count; ++index)
        {
            rangeSets_except_set[index].Except(rangeSets_readonly[index]);
            result += rangeSets_except_set[index].RangesCount;
        }

        return result;
    }

    [Benchmark]
    public int Ip4RangeSet_Except_Ip4RangeArray()
    {
        int result = 0;
        for (int index = 0; index < Count; ++index)
        {
            var temp = new Ip4RangeSet(rangesArray_readonly[index]);
            rangeSets_except_array[index].Except(temp);
            result += rangeSets_except_array[index].RangesCount;
        }

        return result;
    }
}