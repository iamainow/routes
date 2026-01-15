using BenchmarkDotNet.Attributes;
using routes.Extensions;
using System.Numerics;

namespace routes.Benchmarks.SpanHelperBenchmark;

internal struct Ip4AddressWrapper : IEquatable<Ip4AddressWrapper>, IComparable<Ip4AddressWrapper>, IMinMaxValue<Ip4AddressWrapper>, IAdditionOperators<Ip4AddressWrapper, Ip4AddressWrapper, Ip4AddressWrapper>, ISubtractionOperators<Ip4AddressWrapper, Ip4AddressWrapper, Ip4AddressWrapper>
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

    public static Ip4AddressWrapper operator +(Ip4AddressWrapper left, Ip4AddressWrapper right)
    {
        return new Ip4AddressWrapper(new Ip4Address(left._value.ToUInt32() + right._value.ToUInt32()));
    }

    public static Ip4AddressWrapper operator -(Ip4AddressWrapper left, Ip4AddressWrapper right)
    {
        return new Ip4AddressWrapper(new Ip4Address(left._value.ToUInt32() - right._value.ToUInt32()));
    }

    public override bool Equals(object? obj)
    {
        return obj is Ip4AddressWrapper dtw && this.Equals(dtw);
    }

    public override int GetHashCode()
    {
        return this._value.GetHashCode();
    }
}

internal struct DateTimeWrapper : IEquatable<DateTimeWrapper>, IComparable<DateTimeWrapper>, IMinMaxValue<DateTimeWrapper>, IAdditionOperators<DateTimeWrapper, DateTimeWrapper, DateTimeWrapper>, ISubtractionOperators<DateTimeWrapper, DateTimeWrapper, DateTimeWrapper>
{
    public DateTime _value;
    public DateTimeWrapper(DateTime val)
    {
        this._value = val;
    }

    public static DateTimeWrapper MaxValue => new DateTimeWrapper(DateTime.MaxValue);

    public static DateTimeWrapper MinValue => new DateTimeWrapper(DateTime.MinValue);

    public int CompareTo(DateTimeWrapper other)
    {
        return this._value.CompareTo(other._value);
    }

    public bool Equals(DateTimeWrapper other)
    {
        return this._value.Equals(other._value);
    }

    public static DateTimeWrapper operator +(DateTimeWrapper left, DateTimeWrapper right)
    {
        return new DateTimeWrapper(new DateTime(left._value.ToBinary() + right._value.ToBinary()));
    }

    public static DateTimeWrapper operator +(DateTimeWrapper left, TimeSpan right)
    {
        return new DateTimeWrapper(left._value + right);
    }

    public static DateTimeWrapper operator -(DateTimeWrapper left, DateTimeWrapper right)
    {
        return new DateTimeWrapper(new DateTime(left._value.ToBinary() - right._value.ToBinary()));
    }

    public override bool Equals(object? obj)
    {
        return obj is DateTimeWrapper dtw && this.Equals(dtw);
    }

    public override int GetHashCode()
    {
        return this._value.GetHashCode();
    }
}

[Config(typeof(BenchmarkManualConfig))]
public class SpanHelperGenericUInt32UnionExcept
{
    [Params(1_000)]
    public int Count { get; set; }

    [Params(10, 100, 1_000)]
    public int SetSize { get; set; }

    private CustomRange<uint>[][] rangesArray_1 = [];
    private CustomRange<uint>[][] rangesArray_2 = [];

    [GlobalSetup]
    public async Task GlobalSetup()
    {
        Random random = new();
        this.rangesArray_1 = Enumerable.Range(0, this.Count).Select(_ => Ip4RangeArrayExtensions.Generate(this.SetSize, random).Select(x => new CustomRange<uint>(x.FirstAddress.ToUInt32(), x.LastAddress.ToUInt32())).ToArray()).ToArray();
        this.rangesArray_2 = Enumerable.Range(0, this.Count).Select(_ => Ip4RangeArrayExtensions.Generate(this.SetSize, random).Select(x => new CustomRange<uint>(x.FirstAddress.ToUInt32(), x.LastAddress.ToUInt32())).ToArray()).ToArray();
    }

    [Benchmark]
    public int SpanHelperGeneric_UInt32_Union_UnsortedUnsorted_Via_NormalizedNormalized()
    {
        var buffer = new CustomRange<uint>[this.SetSize * 2];
        int result = 0;
        for (int index = 0; index < this.Count; ++index)
        {
            result += SpanHelperGeneric.UnionUnsortedUnsortedViaNormalizedNormalized(
                this.rangesArray_1[index],
                this.rangesArray_2[index],
                buffer,
                1U);
        }

        return result;
    }

    [Benchmark]
    public int SpanHelperGeneric_UInt32_Union_UnsortedUnsorted_Via_SortedSorted()
    {
        var buffer = new CustomRange<uint>[this.SetSize * 2];
        int result = 0;
        for (int index = 0; index < this.Count; ++index)
        {
            result += SpanHelperGeneric.UnionUnsortedUnsortedViaSortedSorted(
                this.rangesArray_1[index],
                this.rangesArray_2[index],
                buffer,
                1U);
        }

        return result;
    }
}


[Config(typeof(BenchmarkManualConfig))]
public class SpanHelperGenericDateTimeUnionExcept
{
    [Params(1_000)]
    public int Count { get; set; }

    [Params(10, 100, 1_000)]
    public int SetSize { get; set; }

    private CustomRange<DateTimeWrapper>[][] rangesArray_1 = [];
    private CustomRange<DateTimeWrapper>[][] rangesArray_2 = [];

    //private static long round(long val)
    //{
    //    return val / DateTime.MinValue.Ticks * DateTime.MinValue.Ticks;
    //}

    [GlobalSetup]
    public async Task GlobalSetup()
    {
        Random random = new();
        this.rangesArray_1 = Enumerable.Range(0, this.Count).Select(_ => CustomArrayExtensions.Generate(this.SetSize, span => new DateTimeWrapper(DateTime.FromBinary(BitConverter.ToInt64(span))), sizeof(long), random).ToArray()).ToArray();
        this.rangesArray_2 = Enumerable.Range(0, this.Count).Select(_ => CustomArrayExtensions.Generate(this.SetSize, span => new DateTimeWrapper(DateTime.FromBinary(BitConverter.ToInt64(span))), sizeof(long), random).ToArray()).ToArray();
    }

    [Benchmark]
    public int SpanHelperGeneric_DateTime_Union_UnsortedUnsorted_Via_NormalizedNormalized()
    {
        var buffer = new CustomRange<DateTimeWrapper>[this.SetSize * 2];
        int result = 0;
        DateTimeWrapper one = new DateTimeWrapper(DateTime.FromBinary(DateTime.MinValue.Ticks));
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
        DateTimeWrapper one = new DateTimeWrapper(DateTime.FromBinary(DateTime.MinValue.Ticks));
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