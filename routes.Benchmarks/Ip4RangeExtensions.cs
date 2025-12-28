namespace routes.Benchmarks;

internal static class Ip4RangeExtensions
{
    extension(Ip4Range)
    {
        internal static Ip4Range RandomS(Random random)
        {
            return Ip4Range.All.RandomShrink(random);
        }
        internal static Ip4Range RandomH(Random random)
        {
            return Ip4Range.All.RandomHalve(random, 0);
        }
    }

    extension(Ip4Range range)
    {
        internal Ip4Range RandomShrink(Random random)
        {
            uint val1 = range.FirstAddress.ToUInt32() + (uint)random.NextInt64((long)range.Count);
            uint val2 = range.FirstAddress.ToUInt32() + (uint)random.NextInt64((long)range.Count);
            return new Ip4Range(new Ip4Address(Math.Min(val1, val2)), new Ip4Address(Math.Max(val1, val2)));
        }

        internal Ip4Range RandomHalve(Random random, int bits)
        {
            long length = (long)(range.Count >> bits);
            uint address = (uint)random.NextInt64(range.FirstAddress.ToUInt32(), range.LastAddress.ToUInt32() - length + 1L);
            return new Ip4Range(new Ip4Address(address), new Ip4Address((uint)(address + length - 1)));
        }
    }
}