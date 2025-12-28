namespace routes.Benchmarks;

internal static class Ip4SubnetExtensions
{
    extension(Ip4Subnet)
    {
        internal static Ip4Subnet Generate(Random random)
        {
            Ip4Mask mask = new(random.Next(0, 33));

            Span<byte> buffer = stackalloc byte[4];
            random.NextBytes(buffer);
            Ip4Address address = new(BitConverter.ToUInt32(buffer) & mask.AsUInt32());

            return new Ip4Subnet(address, mask);
        }
    }
}
