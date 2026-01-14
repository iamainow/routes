namespace routes;

public readonly struct CustomRange<T> : IEquatable<CustomRange<T>>
    where T : struct, IEquatable<T>, IComparable<T>
{
    public T FirstAddress { get; init; }
    public T LastAddress { get; init; }
    public CustomRange(T firstAddress, T lastAddress)
    {
        if (firstAddress.CompareTo(lastAddress) > 0)
        {
            throw new ArgumentException("FirstAddress must be less than or equal to LastAddress");
        }
        this.FirstAddress = firstAddress;
        this.LastAddress = lastAddress;
    }
    public override string ToString()
    {
        return $"{this.FirstAddress} - {this.LastAddress}";
    }

    public override bool Equals(object? obj)
    {
        if (obj is not CustomRange<T> other)
        {
            return false;
        }
        return this.FirstAddress.Equals(other.FirstAddress) && this.LastAddress.Equals(other.LastAddress);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(this.FirstAddress, this.LastAddress);
    }

    public static bool operator ==(CustomRange<T> left, CustomRange<T> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(CustomRange<T> left, CustomRange<T> right)
    {
        return !(left == right);
    }

    public bool Equals(CustomRange<T> other)
    {
        return this.FirstAddress.Equals(other.FirstAddress) && this.LastAddress.Equals(other.LastAddress);
    }
}
