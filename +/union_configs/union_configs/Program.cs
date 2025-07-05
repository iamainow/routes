if (args.Length < 3)
{
    throw new Exception("args.Length < 3");
}

SortedSet<string> sortedSet = new();
sortedSet.UnionWith(await File.ReadAllLinesAsync(args[0]));
sortedSet.UnionWith(await File.ReadAllLinesAsync(args[1]));

await File.WriteAllLinesAsync(args[2], sortedSet.ToArray());