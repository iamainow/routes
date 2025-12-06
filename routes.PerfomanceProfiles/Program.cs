using routes;
using routes.PerfomanceProfiles;

var profile = new Ip4RangeSetPerfomanceProfile();
await profile.GlobalSetup();

Ip4RangeSet2 result = new();

for (int t = 0; t < 10; t++)
{
    result.Union(profile.Realistic());
}

Console.WriteLine(result);