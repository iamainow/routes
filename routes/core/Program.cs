using System.Text.RegularExpressions;

namespace routes.core;

public static partial class Program2
{
    [GeneratedRegex(@"(?<ip>[\d\.]+)\/(?<cidr>\d+)")]
    public static partial Regex IpParsingRegex();

    public static async Task Main(string filename)
    {
        var ruLines = await File.ReadAllLinesAsync(filename);
        foreach (string ruLine in ruLines)
        {
            if (!string.IsNullOrEmpty(ruLine))
            {
                string[] stringArray = ruLine.Split('/');
                var ip = Ip4Address.Parse(stringArray[0]);
                var mask = new Ip4Mask(int.Parse(stringArray[1]));

                var subnet = new Ip4Subnet(ip, mask);
            }
        }
    }
}