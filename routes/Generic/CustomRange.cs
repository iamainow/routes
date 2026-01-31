namespace routes.Generic;

public interface ICustomRange<T>
{
    T FirstAddress { get; }
    T LastAddress { get; }
}

public readonly record struct CustomRange2<T>(T FirstAddress, T LastAddress);

public readonly struct CustomRange<T> : IEquatable<CustomRange<T>>, ICustomRange<T>
    where T : struct, IEquatable<T>, IComparable<T>
{
    private readonly T firstAddress;
    private readonly T lastAddress;

    public T FirstAddress => this.firstAddress;
    public T LastAddress => this.lastAddress;

    public CustomRange(T firstAddress, T lastAddress)
    {
        if (firstAddress.CompareTo(lastAddress) > 0)
        {
            throw new ArgumentException("firstAddress must be less than or equal to lastAddress");
        }
        this.firstAddress = firstAddress;
        this.lastAddress = lastAddress;
    }
    public override string ToString()
    {
        return $"{this.firstAddress} - {this.lastAddress}";
    }

    public override bool Equals(object? obj)
    {
        if (obj is not CustomRange<T> other)
        {
            return false;
        }
        return Equals(other);
    }

    public bool Equals(CustomRange<T> other)
    {
        return this.firstAddress.Equals(other.firstAddress) && this.lastAddress.Equals(other.lastAddress);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(this.firstAddress, this.lastAddress);
    }

    public static bool operator ==(CustomRange<T> left, CustomRange<T> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(CustomRange<T> left, CustomRange<T> right)
    {
        return !left.Equals(right);
    }
}
