using System.Numerics;

namespace routes.Test;

internal readonly struct Point : IComparable<Point>, IComparisonOperators<Point, Point, bool>, IEquatable<Point>
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

    public static implicit operator Point(int val)
    {
        return new Point(val);
    }

    public static Point ToPoint(int val)
    {
        return new Point(val);
    }
}