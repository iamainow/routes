using routes;
using System.Text.RegularExpressions;

namespace Ip4Parsers;

public static partial class Ip4SubnetParser
{
    [GeneratedRegex(@"(?:(?<rangebegin>\d+\.\d+\.\d+\.\d+)\s*\-\s*(?<rangeend>\d+\.\d+\.\d+\.\d+)|(?<cidrip>\d+\.\d+\.\d+\.\d+)(?<cidrmask>\/\d+)|(?<ip>\d+\.\d+\.\d+\.\d+))")]
    public static partial Regex RangeOrCidrOrIp();

    [GeneratedRegex(@"(?:(?<rangebegin1>\d+)\.(?<rangebegin2>\d+)\.(?<rangebegin3>\d+)\.(?<rangebegin4>\d+)\s*\-\s*(?<rangeend1>\d+)\.(?<rangeend2>\d+)\.(?<rangeend3>\d+)\.(?<rangeend4>\d+)|(?<cidrip1>\d+)\.(?<cidrip2>\d+)\.(?<cidrip3>\d+)\.(?<cidrip4>\d+)(?<cidrmask>\/\d+)|(?<ip1>\d+)\.(?<ip2>\d+)\.(?<ip3>\d+)\.(?<ip4>\d+))")]
    public static partial Regex RangeOrCidrOrIp2();

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
            MatchCollection matches = RangeOrCidrOrIp2().Matches(text);
            foreach (Match match in matches)
            {
                if (match.Success)
                {
                    if (match.Groups["rangebegin1"].Success && match.Groups["rangebegin2"].Success && match.Groups["rangebegin3"].Success && match.Groups["rangebegin4"].Success
                        && match.Groups["rangeend1"].Success && match.Groups["rangeend2"].Success && match.Groups["rangeend3"].Success && match.Groups["rangeend4"].Success)
                    {
                        if (!byte.TryParse(match.Groups["rangebegin1"].Value, out byte rangebegin1))
                        {
                            continue;
                        }
                        if (!byte.TryParse(match.Groups["rangebegin2"].Value, out byte rangebegin2))
                        {
                            continue;
                        }
                        if (!byte.TryParse(match.Groups["rangebegin3"].Value, out byte rangebegin3))
                        {
                            continue;
                        }
                        if (!byte.TryParse(match.Groups["rangebegin4"].Value, out byte rangebegin4))
                        {
                            continue;
                        }

                        Ip4Address rangebegin = new Ip4Address(rangebegin1, rangebegin2, rangebegin3, rangebegin4);

                        if (!byte.TryParse(match.Groups["rangeend1"].Value, out byte rangeend1))
                        {
                            continue;
                        }
                        if (!byte.TryParse(match.Groups["rangeend2"].Value, out byte rangeend2))
                        {
                            continue;
                        }
                        if (!byte.TryParse(match.Groups["rangeend3"].Value, out byte rangeend3))
                        {
                            continue;
                        }
                        if (!byte.TryParse(match.Groups["rangeend4"].Value, out byte rangeend4))
                        {
                            continue;
                        }

                        Ip4Address rangeend = new Ip4Address(rangeend1, rangeend2, rangeend3, rangeend4);

                        yield return new Ip4Range(rangebegin, rangeend);
                    }
                    else if (match.Groups["cidrip1"].Success && match.Groups["cidrip2"].Success && match.Groups["cidrip3"].Success && match.Groups["cidrip4"].Success
                        && match.Groups["cidrmask"].Success)
                    {
                        if (!byte.TryParse(match.Groups["cidrip1"].Value, out byte cidrip1))
                        {
                            continue;
                        }
                        if (!byte.TryParse(match.Groups["cidrip2"].Value, out byte cidrip2))
                        {
                            continue;
                        }
                        if (!byte.TryParse(match.Groups["cidrip3"].Value, out byte cidrip3))
                        {
                            continue;
                        }
                        if (!byte.TryParse(match.Groups["cidrip4"].Value, out byte cidrip4))
                        {
                            continue;
                        }

                        Ip4Address cidrip = new Ip4Address(cidrip1, cidrip2, cidrip3, cidrip4);

                        if (!Ip4Mask.TryParseCidrString(match.Groups["cidrmask"].Value, out Ip4Mask cidrmask))
                        {
                            continue;
                        }

                        yield return new Ip4Subnet(cidrip, cidrmask);
                    }
                    else if (match.Groups["ip1"].Success && match.Groups["ip2"].Success && match.Groups["ip3"].Success && match.Groups["ip4"].Success)
                    {
                        if (!byte.TryParse(match.Groups["ip1"].Value, out byte ip1))
                        {
                            continue;
                        }
                        if (!byte.TryParse(match.Groups["ip2"].Value, out byte ip2))
                        {
                            continue;
                        }
                        if (!byte.TryParse(match.Groups["ip3"].Value, out byte ip3))
                        {
                            continue;
                        }
                        if (!byte.TryParse(match.Groups["ip4"].Value, out byte ip4))
                        {
                            continue;
                        }

                        yield return new Ip4Address(ip1, ip2, ip3, ip4);
                    }
                }
            }
        }
    }

    public static IEnumerable<Ip4Range> GetRanges(string texts) => GetRanges1(texts);

    public static IEnumerable<Ip4Subnet> GetSubnets(string text)
    {
        ArgumentException.ThrowIfNullOrEmpty(text);

        return GetRanges(text).SelectMany(x => x.ToSubnets());
    }
}