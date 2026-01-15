namespace routes.Extensions;

public static class CustomArrayExtensions
{
    public static CustomRange<T>[] Generate<T>(int size, Func<ReadOnlySpan<byte>, T> convert, int bytesSize, Random random)
        where T : struct, IEquatable<T>, IComparable<T>
    {
        ArgumentNullException.ThrowIfNull(convert);
        ArgumentNullException.ThrowIfNull(random);

        SortedSet<T> addresses = new();
        Span<byte> buffer = stackalloc byte[bytesSize];

        while (addresses.Count < size * 2)
        {
            random.NextBytes(buffer);
            addresses.Add(convert(buffer));
        }

        return addresses.Chunk(2).Select(x => new CustomRange<T>(x[0], x[1])).ToArray();
    }
}