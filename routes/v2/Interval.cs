using System.Numerics;

namespace routes.v2;

public readonly struct Interval<T> : IEquatable<Interval<T>>
    where T : IComparable<T>, IComparisonOperators<T, T, bool>, ICountable<T>
{
    public readonly T BeginInclusive { get; }
    public readonly T EndInclusive { get; }

    public Interval(T beginInclusive, T endInclusive)
    {
        if (beginInclusive > endInclusive)
        {
            throw new ArgumentException("beginInclusive must be less than or equal to endInclusive.");
        }

        BeginInclusive = beginInclusive;
        EndInclusive = endInclusive;
    }

    public bool IsIntersects(Interval<T> other)
    {

        return other.BeginInclusive <= EndInclusive && other.EndInclusive >= BeginInclusive;
    }

    public Interval<T> IntersectableUnion(Interval<T> other)
    {
        T begin = BeginInclusive < other.BeginInclusive ? BeginInclusive : other.BeginInclusive;
        T end = EndInclusive > other.EndInclusive ? EndInclusive : other.EndInclusive;

        return new Interval<T>(begin, end);
    }

    public Interval<T>[] Union(Interval<T> other)
    {
        return !IsIntersects(other) ? [this, other] : [IntersectableUnion(other)];
    }

    public Interval<T> IntersectableIntersect(Interval<T> other)
    {
        T begin = BeginInclusive > other.BeginInclusive ? BeginInclusive : other.BeginInclusive;
        T end = EndInclusive < other.EndInclusive ? EndInclusive : other.EndInclusive;
        return new Interval<T>(begin, end);
    }

    public Interval<T>? Intersect(Interval<T> other)
    {
        return !IsIntersects(other) ? null : IntersectableIntersect(other);
    }

    public Interval<T>[] IntersectableExcept(Interval<T> other)
    {
        return other.BeginInclusive <= BeginInclusive
            ? other.EndInclusive < EndInclusive ? [new Interval<T>(other.EndInclusive.GetNext(), EndInclusive)] : []
            : other.EndInclusive < EndInclusive
                ? [new Interval<T>(BeginInclusive, other.BeginInclusive.GetPrevious()), new Interval<T>(other.EndInclusive.GetNext(), EndInclusive)]
                : [new Interval<T>(BeginInclusive, other.BeginInclusive.GetPrevious())];
    }

    public Interval<T>[] Except(Interval<T> other)
    {
        return IsIntersects(other) ? IntersectableExcept(other) : [this];
    }

    public override bool Equals(object? obj)
    {
        return obj is Interval<T> interval && Equals(interval);
    }

    public bool Equals(Interval<T> other)
    {
        return BeginInclusive == other.BeginInclusive && EndInclusive == other.EndInclusive;
    }

    public static bool operator ==(Interval<T> left, Interval<T> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Interval<T> left, Interval<T> right)
    {
        return !(left == right);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(BeginInclusive, EndInclusive);
    }
}
