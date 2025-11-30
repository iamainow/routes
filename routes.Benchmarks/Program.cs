using BenchmarkDotNet.Running;
using routes.Benchmarks;

BenchmarkRunner.Run<Ip4RangeSetBenchmarks>();

// To run: dotnet run --project routes.Benchmarks -c Release
// Note: Power plan changes are disabled via the [Config(typeof(NoPowerPlanConfig))] attribute
// on the Ip4RangeSetBenchmarks class to prevent Windows power plan modifications during benchmarking

/*
| Method    | Runtime        | Mean         | Error       | StdDev      | Gen0      | Allocated   |
|---------- |--------------- |-------------:|------------:|------------:|----------:|------------:|
| Realistic | .NET 10.0      | 396,197.3 us | 7,626.71 us | 6,760.88 us |         - | 10766.45 KB |
| o10k      | .NET 10.0      |     240.3 us |     1.41 us |     1.32 us |   27.8320 |   288.18 KB |
| o100k     | .NET 10.0      |   2,485.6 us |    47.66 us |    56.73 us |  285.1563 |  2921.47 KB |
| o1000k    | .NET 10.0      |  24,679.4 us |   122.78 us |   114.85 us | 2843.7500 | 29242.88 KB |
| Realistic | NativeAOT 10.0 | 406,735.1 us | 6,181.79 us | 5,479.99 us |         - | 10767.77 KB |
| o10k      | NativeAOT 10.0 |     332.2 us |     4.40 us |     3.90 us |   28.3203 |    291.3 KB |
| o100k     | NativeAOT 10.0 |   3,275.3 us |    29.96 us |    28.02 us |  285.1563 |  2924.75 KB |
| o1000k    | NativeAOT 10.0 |  34,838.4 us |   444.76 us |   416.03 us | 2800.0000 | 29214.46 KB |

| Method    | Runtime        | Mean         | Error        | StdDev       | Gen0      | Allocated   |
|---------- |--------------- |-------------:|-------------:|-------------:|----------:|------------:|
| Realistic | .NET 10.0      | 387,944.8 us | 18,983.87 us | 54,772.81 us |         - | 10766.45 KB |
| o10k      | .NET 10.0      |     514.6 us |     10.11 us |     11.64 us |   34.1797 |   286.63 KB |
| o100k     | .NET 10.0      |   5,672.8 us |    113.39 us |    130.58 us |  351.5625 |  2923.32 KB |
| o1000k    | .NET 10.0      |  54,929.9 us |    758.14 us |    709.17 us | 3555.5556 | 29211.92 KB |
| Realistic | NativeAOT 10.0 | 369,739.4 us |  3,836.22 us |  3,400.71 us |         - | 10767.77 KB |
| o10k      | NativeAOT 10.0 |     723.9 us |     10.97 us |     14.27 us |   35.1563 |   292.52 KB |
| o100k     | NativeAOT 10.0 |   7,623.2 us |    105.16 us |     98.36 us |  343.7500 |  2919.45 KB |
| o1000k    | NativeAOT 10.0 |  80,792.6 us |  1,600.19 us |  1,842.78 us | 3571.4286 | 29203.34 KB |

| Method    | Runtime        | Mean         | Error        | StdDev       | Median       | Gen0      | Allocated   |
|---------- |--------------- |-------------:|-------------:|-------------:|-------------:|----------:|------------:|
| Realistic | .NET 10.0      | 401,462.2 us | 10,706.98 us | 31,232.78 us | 386,644.2 us |         - | 10766.45 KB |
| o10k      | .NET 10.0      |     518.4 us |      9.84 us |     10.52 us |     515.9 us |   35.1563 |   294.16 KB |
| o100k     | .NET 10.0      |   5,420.2 us |     68.69 us |     64.25 us |   5,419.5 us |  351.5625 |  2917.52 KB |
| o1000k    | .NET 10.0      |  53,382.8 us |    567.20 us |    530.56 us |  53,415.0 us | 3555.5556 | 29172.75 KB |
| Realistic | NativeAOT 10.0 | 375,397.0 us |  5,213.10 us |  4,621.28 us | 374,880.2 us |         - | 10767.77 KB |
| o10k      | NativeAOT 10.0 |     719.0 us |      9.97 us |      8.84 us |     718.3 us |   35.1563 |   293.92 KB |
| o100k     | NativeAOT 10.0 |   9,562.3 us |    188.62 us |    397.87 us |   9,682.2 us |  343.7500 |  2916.91 KB |
| o1000k    | NativeAOT 10.0 | 100,060.8 us |  1,999.41 us |  2,802.90 us | 100,213.9 us | 3400.0000 | 29199.29 KB |

*/