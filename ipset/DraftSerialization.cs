//using System.Text.Json;

namespace ipset;

//private static async Task SerializeToAmneziaJsonAsync(Ip4RangeSet set, string filePath)
//{
//    var objectToSerialize = set.ToIp4Subnets()
//        .Select(x => new AmneziaItem(x.ToCidrString()))
//        .ToArray();

//    await File.WriteAllTextAsync(filePath, JsonSerializer.Serialize(objectToSerialize, SourceGenerationContext.Default.AmneziaItemArray));
//}

//[JsonSourceGenerationOptions(WriteIndented = true)]
//[JsonSerializable(typeof(GoogleIpsResponseRoot))]
//[JsonSerializable(typeof(AmneziaItem[]))]
//internal sealed partial class SourceGenerationContext : JsonSerializerContext
//{
//}

//internal sealed record AmneziaItem(string hostname);

//internal sealed record GoogleIpsResponseRoot(GoogleIpsResponseItem[] prefixes);

//internal sealed record GoogleIpsResponseItem(string ipv4Prefix);
