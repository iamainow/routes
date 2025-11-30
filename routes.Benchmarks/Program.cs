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

| Method        | Runtime        | Mean       | Error     | StdDev    | Gen0      | Allocated |
|-------------- |--------------- |-----------:|----------:|----------:|----------:|----------:|
| Realistic     | .NET 10.0      | 393.552 ms | 5.5662 ms | 4.9343 ms | 1000.0000 |  10.51 MB |
| Union3Except1 | .NET 10.0      |   6.469 ms | 0.0552 ms | 0.0516 ms | 1695.3125 |  16.94 MB |
| Union3Except2 | .NET 10.0      |   6.382 ms | 0.0367 ms | 0.0326 ms | 1671.8750 |  16.68 MB |
| Union3Except3 | .NET 10.0      |   6.560 ms | 0.0600 ms | 0.0561 ms | 1757.8125 |  17.59 MB |
| Union3Except4 | .NET 10.0      |   6.223 ms | 0.0577 ms | 0.0511 ms | 1679.6875 |  16.76 MB |
| Realistic     | NativeAOT 10.0 | 395.424 ms | 7.3715 ms | 7.5700 ms | 1000.0000 |  10.52 MB |
| Union3Except1 | NativeAOT 10.0 |   7.463 ms | 0.0576 ms | 0.0538 ms | 1617.1875 |  16.19 MB |
| Union3Except2 | NativeAOT 10.0 |   7.669 ms | 0.0497 ms | 0.0465 ms | 1710.9375 |  17.09 MB |
| Union3Except3 | NativeAOT 10.0 |   7.508 ms | 0.0406 ms | 0.0380 ms | 1671.8750 |  16.71 MB |
| Union3Except4 | NativeAOT 10.0 |   7.527 ms | 0.0597 ms | 0.0529 ms | 1648.4375 |  16.46 MB |

*/