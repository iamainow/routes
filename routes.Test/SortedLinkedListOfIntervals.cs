using System.Numerics;

namespace routes.Test;
internal readonly struct Point : ICountable<Point>, IComparable<Point>, IComparisonOperators<Point, Point, bool>, IEquatable<Point>
{
    public int Value { get; }

    public Point(int val)
    {
        Value = val;
    }

    public int CompareTo(Point other)
    {
        return Value.CompareTo(other.Value);
    }

    public override bool Equals(object? obj)
    {
        return obj is Point point && Equals(point);
    }

    public bool Equals(Point other)
    {
        return Value == other.Value;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Value);
    }

    public Point GetNext()
    {
        return new Point(Value + 1);
    }

    public Point GetPrevious()
    {
        return new Point(Value - 1);
    }

    public static bool operator >(Point left, Point right)
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator >=(Point left, Point right)
    {
        return left.CompareTo(right) >= 0;
    }

    public static bool operator <(Point left, Point right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator <=(Point left, Point right)
    {
        return left.CompareTo(right) <= 0;
    }

    public static bool operator ==(Point left, Point right)
    {
        return left.CompareTo(right) == 0;
    }

    public static bool operator !=(Point left, Point right)
    {
        return left.CompareTo(right) != 0;
    }


}
internal class SortedLinkedListOfIntervalsTest
{
    [Theory]
    public void UnionTest(Point p1, Point p2)
    {
        var test = new SortedLinkedListOfIntervals<Point>();
        test.Union(new Interval<Point>(p1, p2));
    }
}
