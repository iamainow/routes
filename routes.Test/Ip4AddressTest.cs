using routes.core;

namespace routes.Test;

public class Ip4AddressTest
{
    [Fact]
    public void CreateByUInt()
    {
        var ip = new Ip4Address(0x12345678);

        var ipAsBytes = ip.AsByteArray();

        Assert.Equal(0x12, ipAsBytes[0]);
        Assert.Equal(0x34, ipAsBytes[1]);
        Assert.Equal(0x56, ipAsBytes[2]);
        Assert.Equal(0x78, ipAsBytes[3]);
    }

    [Fact]
    public void CreateByBytes()
    {
        var ip = new Ip4Address(0x12, 0x34, 0x56, 0x78);

        Assert.Equal((uint)0x12345678, ip.AsUInt32());
    }
}