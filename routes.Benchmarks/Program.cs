using BenchmarkDotNet.Running;
using routes.Benchmarks;

BenchmarkRunner.Run<Ip4RangeSetBenchmarks>();

// To run: dotnet run --project routes.Benchmarks -c Release
// Note: Power plan changes are disabled via the [Config(typeof(NoPowerPlanConfig))] attribute
// on the Ip4RangeSetBenchmarks class to prevent Windows power plan modifications during benchmarking

/*
| Method    | Runtime        | Mean         | Error        | StdDev       | Gen0       | Gen1     | Gen2     | Allocated |
|---------- |--------------- |-------------:|-------------:|-------------:|-----------:|---------:|---------:|----------:|
| Realistic | .NET 10.0      | 498,518.7 us |  9,332.64 us |  9,985.82 us |          - |        - |        - |   10.9 MB |
| o10k      | .NET 10.0      |     671.7 us |      6.78 us |      6.01 us |   138.6719 |   7.8125 |        - |   1.39 MB |
| o100k     | .NET 10.0      |   8,703.5 us |    173.80 us |    162.57 us |  1390.6250 |  31.2500 |        - |  14.01 MB |
| o1000k    | .NET 10.0      |  89,039.2 us |  1,693.66 us |  1,882.50 us | 13666.6667 | 833.3333 | 500.0000 | 139.65 MB |
| Realistic | NativeAOT 10.0 | 541,797.8 us | 10,439.79 us | 12,427.82 us |  1000.0000 |        - |        - |   10.9 MB |
| o10k      | NativeAOT 10.0 |     962.3 us |     18.68 us |     18.35 us |   132.8125 |   6.8359 |        - |   1.33 MB |
| o100k     | NativeAOT 10.0 |  12,245.7 us |    140.67 us |    124.70 us |  1343.7500 |  78.1250 |        - |  13.44 MB |
| o1000k    | NativeAOT 10.0 | 123,077.1 us |  1,835.44 us |  1,716.88 us | 13250.0000 | 750.0000 | 250.0000 | 133.93 MB |
*/