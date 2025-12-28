namespace routes.Benchmarks;

internal static class Ip4RangeSetExtensions
{
    extension(Ip4RangeSet)
    {
        internal static Ip4RangeSet Generate(int size, Random random)
        {
            SortedSet<Ip4Address> addresses = new();
            byte[] buffer = new byte[4];
            for (int index = 0; index < size; ++index)
            {
                while (true)
                {
                    random.NextBytes(buffer);
                    if (addresses.Add(new Ip4Address(buffer)))
                    {
                        break;
                    }
                }
                while (true)
                {
                    random.NextBytes(buffer);
                    if (addresses.Add(new Ip4Address(buffer)))
                    {
                        break;
                    }
                }
            }

            while (true)
            {
                var ranges = addresses.Chunk(2).Select(x => new Ip4Range(x[0], x[1])).ToArray();
                Ip4RangeSet result = new(ranges);
                if (result.RangesCount >= size)
                {
                    return result;
                }

                for (int index = 0; index < (size - result.RangesCount); ++index)
                {
                    while (true)
                    {
                        random.NextBytes(buffer);
                        if (addresses.Add(new Ip4Address(buffer)))
                        {
                            break;
                        }
                    }
                    while (true)
                    {
                        random.NextBytes(buffer);
                        if (addresses.Add(new Ip4Address(buffer)))
                        {
                            break;
                        }
                    }
                }
            }
        }
    }
}