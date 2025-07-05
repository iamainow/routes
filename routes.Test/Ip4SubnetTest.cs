using routes.core;

namespace routes.Test;

public class Ip4SubnetTest
{
    [Fact]
    public void CreateByUInt_ThenToString()
    {
        var ip = new Ip4Address(0x80800000);

        var mask = new Ip4Mask(0xFFFF0000);
        var subnet = new Ip4Subnet(ip, mask);
        string actualResult = $"{subnet.FirstAddress}-{subnet.LastAddress}";

        Assert.Equal("128.128.0.0-128.128.255.255", actualResult);
    }

    [Fact]
    public void Create()
    {
        var ip1 = new Ip4Address(0xCDE00000);
        var ip2 = new Ip4Address(0xCDE0FFFF);
        var subnet = new Ip4Subnet(ip1, ip2);

        Assert.Equal(ip1, subnet.FirstAddress);
        Assert.Equal(0xFFFF0000.ToString("x2"), subnet.Mask.AsUInt32().ToString("x2"));
        Assert.Equal(ip2, subnet.LastAddress);

    }
}