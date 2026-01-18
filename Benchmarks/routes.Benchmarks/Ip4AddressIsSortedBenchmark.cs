using BenchmarkDotNet.Attributes;

namespace routes.Benchmarks;

[Config(typeof(BenchmarkManualConfig))]
public class Ip4AddressIsSortedBenchmark
{
    [Params(100, 1000, 10000, 100000)]
    public int ArraySize { get; set; }

    [Params(SortInputType.Sorted, SortInputType.UnsortedEarly, SortInputType.UnsortedLate)]
    public SortInputType InputType { get; set; }

    private Ip4Address[] addresses = [];

    public enum SortInputType
    {
        Sorted,
        UnsortedEarly,
        UnsortedLate
    }

    [GlobalSetup]
    public void GlobalSetup()
    {
        addresses = new Ip4Address[ArraySize];
        var random = new Random(42); // Fixed seed for reproducible results

        // Generate sorted addresses
        for (int i = 0; i < ArraySize; i++)
        {
            addresses[i] = new Ip4Address((uint)random.Next(0, int.MaxValue));
        }

        // Sort them
        Array.Sort(addresses, (a, b) => a.CompareTo(b));

        // Modify based on sort type
        switch (InputType)
        {
            case SortInputType.Sorted:
                // Already sorted, do nothing
                break;
            case SortInputType.UnsortedEarly:
                // Make it unsorted early by swapping first two elements
                if (ArraySize >= 2)
                {
                    (addresses[0], addresses[1]) = (addresses[1], addresses[0]);
                }
                break;
            case SortInputType.UnsortedLate:
                // Make it unsorted late by swapping last two elements
                if (ArraySize >= 2)
                {
                    (addresses[ArraySize - 2], addresses[ArraySize - 1]) = (addresses[ArraySize - 1], addresses[ArraySize - 2]);
                }
                break;
            default:
                throw new InvalidOperationException($"Unknown SortInputType: {InputType}");
        }
    }

    [Benchmark]
    public bool IsSortedAscending_SIMD()
    {
        return Ip4Address.IsSortedAscendingSIMD(addresses);
    }

    [Benchmark]
    public bool IsSortedAscending_Scalar()
    {
        return Ip4Address.IsSortedAscending(addresses);
    }
}