using routes;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Ip4Parsers;

public static partial class Ip4SubnetParser
{
    [GeneratedRegex(@"(?:(?<rangebegin>\d+\.\d+\.\d+\.\d+)\s*\-\s*(?<rangeend>\d+\.\d+\.\d+\.\d+)|(?<cidrip>\d+\.\d+\.\d+\.\d+)(?<cidrmask>\/\d+)|(?<ip>\d+\.\d+\.\d+\.\d+))")]
    public static partial Regex RangeOrCidrOrIp();

    public static Span<Ip4Range> GetRanges(ReadOnlySpan<char> texts)
    {
        List<Ip4Range> result = new();
        ReadOnlySpan<char> separators = stackalloc char[] { '\r', '\n' };

        foreach (var range in texts.SplitAny(separators))
        {
            var text = texts[range];
            foreach (var matchRange in RangeOrCidrOrIp().EnumerateMatches(text))
            {
                Debug.Assert(matchRange.Length > 0);
                var match = text[matchRange.Index..(matchRange.Index + matchRange.Length)];
                int dashIndex = match.IndexOf('-');
                if (dashIndex >= 0)
                {
                    var address1Span = match[..dashIndex].TrimEnd();
                    if (!Ip4Address.TryParse(address1Span, out var address1))
                    {
                        continue;
                    }

                    var address2Span = match[(dashIndex + 1)..].TrimStart();
                    if (!Ip4Address.TryParse(address2Span, out var address2))
                    {
                        continue;
                    }

                    result.Add(new Ip4Range(address1, address2));
                }
                else
                {
                    int slashIndex = match.IndexOf('/');
                    if (slashIndex >= 0)
                    {
                        var addressSpan = match[..slashIndex];
                        if (!Ip4Address.TryParse(addressSpan, out var address))
                        {
                            continue;
                        }

                        var maskSpan = match[(slashIndex + 1)..];
                        if (!int.TryParse(maskSpan, out int mask))
                        {
                            continue;
                        }

                        result.Add(new Ip4Subnet(address, new Ip4Mask(mask)));
                    }
                    else
                    {
                        if (!Ip4Address.TryParse(match, out var address))
                        {
                            continue;
                        }

                        result.Add(address);
                    }
                }
            }
        }

        return CollectionsMarshal.AsSpan(result);
    }

    public static Span<Ip4Subnet> GetSubnets(ReadOnlySpan<char> text)
    {
        var ranges = GetRanges(text);
        var result = new List<Ip4Subnet>();
        foreach (var range in ranges)
        {
            result.AddRange(range.ToSubnets());
        }

        return CollectionsMarshal.AsSpan(result);
    }
}