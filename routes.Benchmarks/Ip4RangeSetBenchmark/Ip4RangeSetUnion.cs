using BenchmarkDotNet.Attributes;

namespace routes.Benchmarks.Ip4RangeSetBenchmark;

[MemoryDiagnoser]
[ExceptionDiagnoser]
[Config(typeof(NoPowerPlanConfig))]
public class Ip4RangeSetUnion
{
    [Params(1_000)]
    public int Count { get; set; }

    [Params(16, 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192, 16384, 32768, 65538)]
    public int SetSize { get; set; }

    private List<Ip4RangeSet> rangeSets1 = [];
    private List<Ip4RangeSet> rangeSets2 = [];

    private List<Ip4Range[]> ranges3 = [];

    [GlobalSetup]
    public async Task GlobalSetup()
    {
        Random random = new();
        byte[] buffer = new byte[4];
        List<Ip4Range> ranges = Enumerable.Range(0, (Count + 1) * SetSize).Select(_ =>
        {
            Ip4Mask mask = new(16);

            random.NextBytes(buffer);

            var address = new Ip4Address(BitConverter.ToUInt32(buffer) & mask.AsUInt32());
            Ip4Subnet subnet = new(address, mask);

            return new Ip4Subnet().ToIp4Range();
        }).ToList();

        ranges3 = ranges.Chunk(SetSize).ToList();

        rangeSets1 = ranges3.Take(Count).Select(chunk => new Ip4RangeSet(chunk)).ToList();
        rangeSets2 = ranges.Skip(1).Select(chunk => new Ip4RangeSet(chunk)).ToList();
    }

    [Benchmark]
    public int Ip4RangeSet_Union_Sorted()
    {
        int result = 0;
        for (int index = 0; index < Count; index++)
        {
            var set1 = rangeSets1[index];
            var set2 = rangeSets2[index];
            set1.Union(set2);
            result += set1.RangesCount;
        }

        return result;
    }

    [Benchmark]
    public int Ip4RangeSet_Union_Unsorted()
    {
        int result = 0;
        for (int index = 0; index < Count; index++)
        {
            var set1 = rangeSets2[index];
            var set2 = ranges3[index];
            set1.Union(set2);
            result += set1.RangesCount;
        }

        return result;
    }
}