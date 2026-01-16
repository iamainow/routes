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
