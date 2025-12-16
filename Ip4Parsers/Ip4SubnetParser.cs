using routes;
using System.Text.RegularExpressions;

namespace Ip4Parsers;

public static partial class Ip4SubnetParser
{
    [GeneratedRegex(@"(?:(?<rangebegin>\d+\.\d+\.\d+\.\d+)\s*\-\s*(?<rangeend>\d+\.\d+\.\d+\.\d+)|(?<cidrip>\d+\.\d+\.\d+\.\d+)(?<cidrmask>\/\d+)|(?<ip>\d+\.\d+\.\d+\.\d+))")]
    public static partial Regex RangeOrCidrOrIp();

    public static IEnumerable<Ip4Range> GetRanges1(string texts)
    {
        ArgumentException.ThrowIfNullOrEmpty(texts);

        foreach (string text in texts.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            MatchCollection matches = RangeOrCidrOrIp().Matches(text);
            foreach (Match match in matches)
            {
                if (match.Success)
                {
                    if (match.Groups["rangebegin"].Success && match.Groups["rangeend"].Success)
                    {
                        if (!Ip4Address.TryParse(match.Groups["rangebegin"].Value, out Ip4Address begin))
                        {
                            continue;
                        }

                        if (!Ip4Address.TryParse(match.Groups["rangeend"].Value, out Ip4Address end))
                        {
                            continue;
                        }

                        yield return new Ip4Range(begin, end);
                    }
                    else if (match.Groups["cidrip"].Success && match.Groups["cidrmask"].Success)
                    {
                        if (!Ip4Address.TryParse(match.Groups["cidrip"].Value, out Ip4Address ip))
                        {
                            continue;
                        }

                        if (!Ip4Mask.TryParseCidrString(match.Groups["cidrmask"].Value, out Ip4Mask mask))
                        {
                            continue;
                        }

                        yield return new Ip4Subnet(ip, mask);
                    }
                    else if (match.Groups["ip"].Success)
                    {
                        if (!Ip4Address.TryParse(match.Groups["ip"].Value, out Ip4Address ip))
                        {
                            continue;
                        }

                        yield return ip;
                    }
                }
            }
        }
    }

    public static IEnumerable<Ip4Range> GetRanges2(string texts)
    {
        ArgumentException.ThrowIfNullOrEmpty(texts);

        foreach (string text in texts.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            MatchCollection matches = RangeOrCidrOrIp().Matches(text);
            foreach (Match match in matches)
            {
                if (match.Success)
                {
                    if (match.Groups["rangebegin"].Success && match.Groups["rangeend"].Success)
                    {
                        if (!Ip4Address.TryParse(match.Groups["rangebegin"].ValueSpan, out Ip4Address begin))
                        {
                            continue;
                        }

                        if (!Ip4Address.TryParse(match.Groups["rangeend"].ValueSpan, out Ip4Address end))
                        {
                            continue;
                        }

                        yield return new Ip4Range(begin, end);
                    }
                    else if (match.Groups["cidrip"].Success && match.Groups["cidrmask"].Success)
                    {
                        if (!Ip4Address.TryParse(match.Groups["cidrip"].ValueSpan, out Ip4Address ip))
                        {
                            continue;
                        }

                        if (!int.TryParse(match.Groups["cidrmask"].ValueSpan, out int mask))
                        {
                            continue;
                        }

                        yield return new Ip4Subnet(ip, mask);
                    }
                    else if (match.Groups["ip"].Success)
                    {
                        if (!Ip4Address.TryParse(match.Groups["ip"].ValueSpan, out Ip4Address ip))
                        {
                            continue;
                        }

                        yield return ip;
                    }
                }
            }
        }
    }

    public static IEnumerable<Ip4Range> GetRanges(string texts) => GetRanges2(texts);

    public static IEnumerable<Ip4Subnet> GetSubnets(string text)
    {
        ArgumentException.ThrowIfNullOrEmpty(text);

        return GetRanges(text).SelectMany(x => x.ToSubnets());
    }
}