using BenchmarkDotNet.Running;
using routes.Benchmarks;

BenchmarkRunner.Run<Ip4RangeSetBenchmarks>();

// To run: dotnet run --project routes.Benchmarks -c Release
// Note: Power plan changes are disabled via the [Config(typeof(NoPowerPlanConfig))] attribute
// on the Ip4RangeSetBenchmarks class to prevent Windows power plan modifications during benchmarking

/*
| Method | Job       | Runtime   | Mean       | Error    | StdDev   | Ratio | RatioSD | Gen0        | Gen1        | Gen2       | Allocated  | Alloc Ratio |
|------- |---------- |---------- |-----------:|---------:|---------:|------:|--------:|------------:|------------:|-----------:|-----------:|------------:|
| v1     | .NET 10.0 | .NET 10.0 | 2,592.7 ms | 43.06 ms | 40.28 ms |  1.00 |    0.02 | 722000.0000 |  11000.0000 |  9000.0000 | 7229.88 MB |        1.00 |
| v1     | .NET 8.0  | .NET 8.0  | 3,082.1 ms | 20.30 ms | 15.85 ms |  1.19 |    0.02 | 705000.0000 |  64000.0000 | 11000.0000 | 7230.43 MB |        1.00 |
| v1     | .NET 9.0  | .NET 9.0  | 2,866.3 ms | 29.70 ms | 27.78 ms |  1.11 |    0.02 | 705000.0000 | 139000.0000 | 12000.0000 | 7230.44 MB |        1.00 |
|        |           |           |            |          |          |       |         |             |             |            |            |             |
| v2     | .NET 10.0 | .NET 10.0 |   507.1 ms |  8.88 ms |  8.30 ms |  1.00 |    0.02 |           - |           - |          - |    10.9 MB |        1.00 |
| v2     | .NET 8.0  | .NET 8.0  |   519.9 ms |  7.57 ms |  7.08 ms |  1.03 |    0.02 |           - |           - |          - |    10.9 MB |        1.00 |
| v2     | .NET 9.0  | .NET 9.0  |   516.8 ms | 10.30 ms | 10.57 ms |  1.02 |    0.03 |           - |           - |          - |    10.9 MB |        1.00 |

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

| Method                 | Mean       | Error     | StdDev    | Gen0      | Allocated |
|----------------------- |-----------:|----------:|----------:|----------:|----------:|
| Realistic              | 336.843 ms | 6.4444 ms | 8.3795 ms | 1000.0000 |  12.27 MB |
| RealisticWithoutParser |  61.410 ms | 1.2264 ms | 1.6372 ms |         - |   1.04 MB |
| Union3Except1          |   7.021 ms | 0.1377 ms | 0.1791 ms | 1640.6250 |  16.36 MB |
| Union3Except2          |   6.964 ms | 0.1385 ms | 0.1849 ms | 1632.8125 |  16.35 MB |
| Union3Except3          |   7.047 ms | 0.0891 ms | 0.0790 ms | 1664.0625 |  16.61 MB |
| Union3Except4          |   7.149 ms | 0.1033 ms | 0.0967 ms | 1750.0000 |  17.48 MB |

| Method                 | Runtime        | Mean       | Error     | StdDev    | Gen0      | Gen1     | Allocated |
|----------------------- |--------------- |-----------:|----------:|----------:|----------:|---------:|----------:|
| Realistic              | .NET 10.0      | 6,077.0 us |  32.72 us |  27.32 us | 1226.5625 | 570.3125 |  12.24 MB |
| RealisticWithoutParser | .NET 10.0      |   398.2 us |   4.26 us |   3.78 us |  101.5625 |  50.2930 |   1.02 MB |
| Union3Except4          | .NET 10.0      | 7,278.7 us | 144.51 us | 154.63 us | 1742.1875 |        - |  17.38 MB |
| Realistic              | NativeAOT 10.0 | 7,036.8 us |  79.99 us |  74.82 us | 1226.5625 | 570.3125 |  12.24 MB |
| RealisticWithoutParser | NativeAOT 10.0 |   574.4 us |   7.66 us |   7.17 us |  101.5625 |  43.9453 |   1.02 MB |
| Union3Except4          | NativeAOT 10.0 | 8,614.1 us | 168.09 us | 186.83 us | 1750.0000 |        - |  17.47 MB |

| Method                 | Mean       | Error    | StdDev   | Gen0      | Gen1     | Allocated   |
|----------------------- |-----------:|---------:|---------:|----------:|---------:|------------:|
| Realistic              | 5,875.9 us | 65.52 us | 58.08 us | 1226.5625 | 570.3125 | 12532.37 KB |
| RealisticWithoutParser |   381.8 us |  5.04 us |  4.71 us |  101.5625 |  50.2930 |  1039.55 KB |
| Union3Except4          | 6,747.4 us | 95.81 us | 80.01 us | 1679.6875 |        - | 17174.29 KB |
| Union4Except4          | 1,718.7 us | 16.86 us | 14.95 us |   23.4375 |        - |   248.26 KB |

*/