using BenchmarkDotNet.Running;
using Ip4Parsers.Benchmarks;

BenchmarkRunner.Run<Ip4ParsersBenchmarks>();

// To run: dotnet run --project Ip4Parsers.Benchmarks -c Release
// Note: Power plan changes are disabled via the [Config(typeof(NoPowerPlanConfig))] attribute
// on the Ip4RangeSetBenchmarks class to prevent Windows power plan modifications during benchmarking

/*
| Method                      | Mean       | Error      | StdDev     | Gen0        | Gen1       | Gen2      | Allocated     |
|---------------------------- |-----------:|-----------:|-----------:|------------:|-----------:|----------:|--------------:|
| RealisticGetRanges1         |   5.738 ms |  0.0945 ms |  0.0884 ms |   1125.0000 |    62.5000 |         - |   11560.67 KB |
| RealisticGetRanges2         |   4.645 ms |  0.0809 ms |  0.0717 ms |    835.9375 |   828.1250 |         - |    8577.39 KB |
| RealisticGetRanges3         |   1.254 ms |  0.0082 ms |  0.0068 ms |     25.3906 |     1.9531 |         - |     389.46 KB |
| PareseAddressesByGetRanges1 | 684.879 ms | 13.0861 ms | 17.0156 ms | 122000.0000 |  7000.0000 | 2000.0000 | 1244521.09 KB |
| PareseAddressesByGetRanges2 | 601.418 ms | 11.9841 ms | 20.0227 ms |  98000.0000 |  7000.0000 | 2000.0000 | 1003558.73 KB |
| PareseAddressesByGetRanges3 | 166.285 ms |  1.8776 ms |  1.7564 ms |           - |          - |         - |   32009.49 KB |
| PareseRangesByGetRanges1    | 819.371 ms | 12.6267 ms | 11.1932 ms | 155000.0000 | 10000.0000 | 2000.0000 | 1582824.16 KB |
| PareseRangesByGetRanges2    | 664.296 ms | 13.1430 ms | 18.4246 ms |  98000.0000 | 10000.0000 | 2000.0000 | 1000027.38 KB |
| PareseRangesByGetRanges3    | 215.983 ms |  2.3912 ms |  2.2367 ms |           - |          - |         - |   32009.49 KB |
| PareseSubnetsByGetRanges1   | 729.719 ms | 13.9337 ms | 17.6216 ms | 130000.0000 |  7000.0000 | 1000.0000 | 1340651.55 KB |
| PareseSubnetsByGetRanges2   | 597.748 ms | 11.7446 ms | 16.0762 ms |  97000.0000 |  7000.0000 | 1000.0000 |  996845.26 KB |
| PareseSubnetsByGetRanges3   | 162.521 ms |  2.2680 ms |  2.1215 ms |           - |          - |         - |   32009.49 KB |

*/