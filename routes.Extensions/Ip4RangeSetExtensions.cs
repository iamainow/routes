#pragma warning disable CA1034 // Nested types should not be visible

using System.Diagnostics;

namespace routes.Extensions;

public static class Ip4RangeSetExtensions
{
    extension(Ip4RangeSet)
    {
        public static Ip4RangeSet Generate(int size, Random random)
        {
            ArgumentNullException.ThrowIfNull(random);

            SortedSet<Ip4Address> addresses = new();
            Span<byte> buffer = stackalloc byte[4];

            while (addresses.Count < size * 2)
            {
                random.NextBytes(buffer);
                addresses.Add(new Ip4Address(buffer));
            }

            var ranges = addresses.Chunk(2).Select(x => new Ip4Range(x[0], x[1])).ToArray();
            Ip4RangeSet result = new(ranges);

            Debug.Assert(result.RangesCount == size);

            return result;
        }
    }
}