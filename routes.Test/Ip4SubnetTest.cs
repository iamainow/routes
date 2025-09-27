namespace routes.Test;

public class Ip4SubnetTest
{
    [Theory]
    [InlineData("193.227.134.0/24", "193.227.134.0", "193.227.134.255")]
    [InlineData("193.228.161.128/25", "193.228.161.128", "193.228.161.255")]
    [InlineData("193.233.80.188/32", "193.233.80.188", "193.233.80.188")]
    [InlineData("217.197.2.16/31", "217.197.2.16", "217.197.2.17")]
    public void CreateByUInt_Check_FirstAddress_LastAddress(string subnetString, string firstIp, string lastIp)
    {
        Ip4Subnet subnet = Ip4Subnet.Parse(subnetString);
        string actualResult = $"{subnet.FirstAddress}-{subnet.LastAddress}";

        Assert.Equal($"{firstIp}-{lastIp}", actualResult);
    }
}