using routes;
using System.Text.RegularExpressions;

namespace Ip4Parsers;

public static partial class Ip4SubnetParser
{
    [GeneratedRegex(@"(?:(?<rangebegin>\d+\.\d+\.\d+\.\d+)\s*\-\s*(?<rangeend>\d+\.\d+\.\d+\.\d+)|(?<cidrip>\d+\.\d+\.\d+\.\d+)(?<cidrmask>\/\d+)|(?<ip>\d+\.\d+\.\d+\.\d+))")]
    public static partial Regex RangeOrCidrOrIp();

    public static IEnumerable<Ip4Range> GetRanges(string text, Action<string>? errorWriter = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(text);

        var matches = RangeOrCidrOrIp().Matches(text);
        foreach (Match match in matches)
        {
            if (match.Success)
            {
                if (match.Groups["rangebegin"].Success && match.Groups["rangeend"].Success)
                {
                    if (!Ip4Address.TryParse(match.Groups["rangebegin"].Value, out Ip4Address begin))
                    {
                        errorWriter?.Invoke($"error parsing '{match.Groups["rangebegin"].Value}' as 'rangebegin' ip address");
                        continue;
                    }

                    if (!Ip4Address.TryParse(match.Groups["rangeend"].Value, out Ip4Address end))
                    {
                        errorWriter?.Invoke($"error parsing '{match.Groups["rangeend"].Value}' as 'rangeend' ip address");
                        continue;
                    }

                    yield return new Ip4Range(begin, end);
                }
                else if (match.Groups["cidrip"].Success && match.Groups["cidrmask"].Success)
                {
                    if (!Ip4Address.TryParse(match.Groups["cidrip"].Value, out Ip4Address ip))
                    {
                        errorWriter?.Invoke($"error parsing '{match.Groups["cidrip"].Value}' as 'cidrip' ip address");
                        continue;
                    }

                    if (!Ip4Mask.TryParseCidrString(match.Groups["cidrmask"].Value, out Ip4Mask mask))
                    {
                        errorWriter?.Invoke($"error parsing '{match.Groups["cidrmask"].Value}' as 'cidrmask' subnet mask");
                        continue;
                    }

                    yield return new Ip4Subnet(ip, mask);
                }
                else if (match.Groups["ip"].Success)
                {
                    if (!Ip4Address.TryParse(match.Groups["ip"].Value, out Ip4Address ip))
                    {
                        errorWriter?.Invoke($"error parsing '{match.Groups["ip"].Value}' as 'ip' ip address");
                        continue;
                    }

                    yield return ip;
                }
            }
        }
    }

    public static IEnumerable<Ip4Subnet> GetSubnets(string text, Action<string>? errorWriter)
    {
        ArgumentException.ThrowIfNullOrEmpty(text);

        return GetRanges(text, errorWriter).SelectMany(x => x.ToSubnets());
    }
}