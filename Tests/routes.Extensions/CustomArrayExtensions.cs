namespace routes.Extensions;

public static class CustomArrayExtensions
{
    private static T Clamp<T>(T value, T minValue, T maxValue)
        where T : IComparable<T>
    {
        if (value.CompareTo(minValue) < 0)
        {
            return minValue;
        }
        if (value.CompareTo(maxValue) > 0)
        {
            return maxValue;
        }
        return value;
    }
    public static unsafe CustomRange<T>[] GenerateNormalized<T>(int size, Func<ReadOnlySpan<byte>, T> convert, T minValue, T maxValue, Random random)
        where T : unmanaged, IEquatable<T>, IComparable<T>
    {
        ArgumentNullException.ThrowIfNull(convert);
        ArgumentNullException.ThrowIfNull(random);

        SortedSet<T> addresses = new();
        Span<byte> buffer = stackalloc byte[sizeof(T)];

        while (addresses.Count < size * 2)
        {
            random.NextBytes(buffer);
            T value = convert(buffer);
            addresses.Add(Clamp(value, minValue, maxValue));
        }

        return addresses.Chunk(2).Select(x => new CustomRange<T>(x[0], x[1])).ToArray();
    }

    public static unsafe CustomRange<T>[] GenerateNormalized<T>(int size, Func<ReadOnlySpan<byte>, T> convert, Random random)
        where T : unmanaged, IEquatable<T>, IComparable<T>
    {
        ArgumentNullException.ThrowIfNull(convert);
        ArgumentNullException.ThrowIfNull(random);

        SortedSet<T> addresses = new();
        Span<byte> buffer = stackalloc byte[sizeof(T)];

        while (addresses.Count < size * 2)
        {
            random.NextBytes(buffer);
            addresses.Add(convert(buffer));
        }

        return addresses.Chunk(2).Select(x => new CustomRange<T>(x[0], x[1])).ToArray();
    }
}