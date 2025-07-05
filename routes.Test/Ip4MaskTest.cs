using routes.core;

namespace routes.Test;

public class Ip4MaskTest
{
    [Theory]
    [InlineData(32, "255.255.255.255")]
    [InlineData(31, "255.255.255.254")]
    [InlineData(30, "255.255.255.252")]
    [InlineData(29, "255.255.255.248")]
    [InlineData(28, "255.255.255.240")]
    [InlineData(27, "255.255.255.224")]
    [InlineData(26, "255.255.255.192")]
    [InlineData(25, "255.255.255.128")]
    [InlineData(24, "255.255.255.0")]
    [InlineData(23, "255.255.254.0")]
    [InlineData(22, "255.255.252.0")]
    [InlineData(21, "255.255.248.0")]
    [InlineData(20, "255.255.240.0")]
    [InlineData(19, "255.255.224.0")]
    [InlineData(18, "255.255.192.0")]
    [InlineData(17, "255.255.128.0")]
    [InlineData(16, "255.255.0.0")]
    [InlineData(15, "255.254.0.0")]
    [InlineData(14, "255.252.0.0")]
    [InlineData(13, "255.248.0.0")]
    [InlineData(12, "255.240.0.0")]
    [InlineData(11, "255.224.0.0")]
    [InlineData(10, "255.192.0.0")]
    [InlineData(9, "255.128.0.0")]
    [InlineData(8, "255.0.0.0")]
    [InlineData(7, "254.0.0.0")]
    [InlineData(6, "252.0.0.0")]
    [InlineData(5, "248.0.0.0")]
    [InlineData(4, "240.0.0.0")]
    [InlineData(3, "224.0.0.0")]
    [InlineData(2, "192.0.0.0")]
    [InlineData(1, "128.0.0.0")]
    [InlineData(0, "0.0.0.0")]

    public void ParseFromCidr_CheckSubnet(int cidr, string subnet)
    {
        var mask = new Ip4Mask(cidr);

        Assert.Equal(subnet, mask.ToFullString());
    }
}