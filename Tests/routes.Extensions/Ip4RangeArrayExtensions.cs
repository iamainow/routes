namespace routes.Extensions;

public static class Ip4RangeArrayExtensions
{
    public static Ip4Range[] Generate(int size, Random random)
    {
        ArgumentNullException.ThrowIfNull(random);

        SortedSet<Ip4Address> addresses = new();
        Span<byte> buffer = stackalloc byte[4];

        while (addresses.Count < size * 2)
        {
            random.NextBytes(buffer);
            addresses.Add(new Ip4Address(buffer));
        }

        return addresses.Chunk(2).Select(x => new Ip4Range(x[0], x[1])).ToArray();
    }
}
