using System.Numerics;

namespace routes.Benchmarks.SpanHelperBenchmark;

internal readonly struct DateTimeWrapper : IEquatable<DateTimeWrapper>, IComparable<DateTimeWrapper>, IMinMaxValue<DateTimeWrapper>, IAdditionOperators<DateTimeWrapper, DateTimeWrapper, DateTimeWrapper>, ISubtractionOperators<DateTimeWrapper, DateTimeWrapper, DateTimeWrapper>
{
    public readonly DateTime _value;
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
