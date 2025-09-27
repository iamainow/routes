using routes;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ipops;

internal static class Ip4RangeSetSerializers
{
    public static string SerializeToAmneziaJson(Ip4RangeSet set)
    {
        AmneziaItem[] objectToSerialize = set.ToIp4Subnets()
            .Select(x => new AmneziaItem(x.ToCidrString()))
            .ToArray();

        return JsonSerializer.Serialize(objectToSerialize, SourceGenerationContext.Default.AmneziaItemArray);
    }
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(GoogleIpsResponseRoot))]
[JsonSerializable(typeof(AmneziaItem[]))]
internal sealed partial class SourceGenerationContext : JsonSerializerContext
{
}

internal sealed record AmneziaItem(string hostname);

internal sealed record GoogleIpsResponseRoot(GoogleIpsResponseItem[] prefixes);

internal sealed record GoogleIpsResponseItem(string ipv4Prefix);
