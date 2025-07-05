using routes.core;

namespace routes.Test;

public class Ip4SubnetTest
{
    [Theory]
    [InlineData("193.227.134.0/24", "193.227.134.0", "193.227.134.255")] // 255.255.255.0
    [InlineData("193.228.161.128/25", "193.228.161.128", "193.228.161.255")] // 255.255.255.128
    [InlineData("193.233.80.188/32", "193.233.80.188", "193.233.80.188")] // 255.255.255.255
    [InlineData("217.197.2.16/31", "217.197.2.16", "217.197.2.17")] // 255.255.255.254
    public void CreateByUInt_Check_FirstAddress_LastAddress(string ipWithCidrMask, string firstIp, string lastIp)
    {
        string[] parts = ipWithCidrMask.Split('/');
        var ip = Ip4Address.Parse(parts[0]);
        var mask = new Ip4Mask(int.Parse(parts[1]));
        var subnet = new Ip4Subnet(ip, mask);
        string actualResult = $"{subnet.FirstAddress}-{subnet.LastAddress}";

        Assert.Equal($"{firstIp}-{lastIp}", actualResult);
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