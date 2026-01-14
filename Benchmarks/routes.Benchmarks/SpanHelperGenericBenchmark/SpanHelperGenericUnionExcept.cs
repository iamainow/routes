using BenchmarkDotNet.Attributes;
using routes.Extensions;
using System.Numerics;

namespace routes.Benchmarks.SpanHelperBenchmark;

internal struct Ip4AddressWrapper : IEquatable<Ip4AddressWrapper>, IComparable<Ip4AddressWrapper>, IMinMaxValue<Ip4AddressWrapper>, IAdditionOperators<Ip4AddressWrapper, int, Ip4AddressWrapper>, ISubtractionOperators<Ip4AddressWrapper, int, Ip4AddressWrapper>
{
    public Ip4Address _value;

    public Ip4AddressWrapper(Ip4Address val)
    {
        this._value = val;
    }

    public static Ip4AddressWrapper MaxValue => new Ip4AddressWrapper(Ip4Address.MaxValue);

    public static Ip4AddressWrapper MinValue => new Ip4AddressWrapper(Ip4Address.MinValue);

    public int CompareTo(Ip4AddressWrapper other)
    {
        return this._value.CompareTo(other._value);
    }

    public bool Equals(Ip4AddressWrapper other)
    {
        return this._value.Equals(other._value);
    }

    public static Ip4AddressWrapper operator +(Ip4AddressWrapper left, int right)
    {
        return new Ip4AddressWrapper(new Ip4Address(left._value.ToUInt32() + (uint)right));
    }

    public static Ip4AddressWrapper operator -(Ip4AddressWrapper left, int right)
    {
        return new Ip4AddressWrapper(new Ip4Address(left._value.ToUInt32() - (uint)right));
    }

    public override bool Equals(object? obj)
    {
        return obj is Ip4AddressWrapper dtw && this.Equals(dtw);
    }

    public override int GetHashCode()
    {
        return this._value.GetHashCode();
    }

    public static explicit operator uint(Ip4AddressWrapper address) => address._value.ToUInt32();
    public static implicit operator Ip4Address(Ip4AddressWrapper address) => address._value;
}

[Config(typeof(BenchmarkManualConfig))]
public class SpanHelperGenericUnionExcept
{
    [Params(1_000)]
    public int Count { get; set; }

    [Params(10, 100, 1_000)]
    public int SetSize { get; set; }

    private CustomRange<Ip4AddressWrapper>[][] rangesArray_1 = [];
    private CustomRange<Ip4AddressWrapper>[][] rangesArray_2 = [];

    [GlobalSetup]
    public async Task GlobalSetup()
    {
        Random random = new();
        this.rangesArray_1 = Enumerable.Range(0, this.Count).Select(_ => Ip4RangeArrayExtensions.Generate(this.SetSize, random).Select(x => new CustomRange<Ip4AddressWrapper>(new Ip4AddressWrapper(x.FirstAddress), new Ip4AddressWrapper(x.LastAddress))).ToArray()).ToArray();
        this.rangesArray_2 = Enumerable.Range(0, this.Count).Select(_ => Ip4RangeArrayExtensions.Generate(this.SetSize, random).Select(x => new CustomRange<Ip4AddressWrapper>(new Ip4AddressWrapper(x.FirstAddress), new Ip4AddressWrapper(x.LastAddress))).ToArray()).ToArray();
    }

    [Benchmark]
    public int SpanHelperGeneric_Union_UnsortedUnsorted_Via_NormalizedNormalized()
    {
        var buffer = new CustomRange<Ip4AddressWrapper>[this.SetSize * 2];
        int result = 0;
        for (int index = 0; index < this.Count; ++index)
        {
            result += SpanHelperGeneric.UnionUnsortedUnsortedViaNormalizedNormalized(
                this.rangesArray_1[index],
                this.rangesArray_2[index],
                buffer);
        }

        return result;
    }

    [Benchmark]
    public int SpanHelperGeneric_Union_UnsortedUnsorted_Via_SortedSorted()
    {
        var buffer = new CustomRange<Ip4AddressWrapper>[this.SetSize * 2];
        int result = 0;
        for (int index = 0; index < this.Count; ++index)
        {
            result += SpanHelperGeneric.UnionUnsortedUnsortedViaSortedSorted(
                this.rangesArray_1[index],
                this.rangesArray_2[index],
                buffer);
        }

        return result;
    }
}