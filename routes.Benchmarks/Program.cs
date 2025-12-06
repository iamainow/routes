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

| Method                 | Mean       | Error     | StdDev    | Gen0      | Allocated |
|----------------------- |-----------:|----------:|----------:|----------:|----------:|
| Realistic              | 336.843 ms | 6.4444 ms | 8.3795 ms | 1000.0000 |  12.27 MB |
| RealisticWithoutParser |  61.410 ms | 1.2264 ms | 1.6372 ms |         - |   1.04 MB |
| Union3Except1          |   7.021 ms | 0.1377 ms | 0.1791 ms | 1640.6250 |  16.36 MB |
| Union3Except2          |   6.964 ms | 0.1385 ms | 0.1849 ms | 1632.8125 |  16.35 MB |
| Union3Except3          |   7.047 ms | 0.0891 ms | 0.0790 ms | 1664.0625 |  16.61 MB |
| Union3Except4          |   7.149 ms | 0.1033 ms | 0.0967 ms | 1750.0000 |  17.48 MB |

| Method                 | Mean       | Error     | StdDev    | Gen0      | Allocated |
|----------------------- |-----------:|----------:|----------:|----------:|----------:|
| Realistic              | 347.440 ms | 6.5509 ms | 6.7273 ms | 1000.0000 |  12.27 MB |
| RealisticWithoutParser |  61.642 ms | 0.5825 ms | 0.5163 ms |         - |   1.04 MB |
| Union3Except1          |   7.229 ms | 0.1437 ms | 0.1869 ms | 1703.1250 |  17.01 MB |
| Union32Except1         |   7.408 ms | 0.1264 ms | 0.1120 ms | 1750.0000 |  17.52 MB |
| Union3Except2          |   7.318 ms | 0.1154 ms | 0.1023 ms | 1671.8750 |  16.76 MB |
| Union3Except3          |   7.473 ms | 0.1383 ms | 0.1480 ms | 1750.0000 |  17.47 MB |
| Union3Except4          |   6.881 ms | 0.1344 ms | 0.1320 ms | 1656.2500 |  16.58 MB |

| Method                 | Mean       | Error     | StdDev    | Gen0      | Gen1     | Allocated |
|----------------------- |-----------:|----------:|----------:|----------:|---------:|----------:|
| Realistic              | 6,192.4 us |  57.61 us |  51.07 us | 1226.5625 | 570.3125 |  12.24 MB |
| RealisticWithoutParser |   431.2 us |   8.23 us |   7.30 us |  101.5625 |  50.2930 |   1.02 MB |
| Union3Except1          | 7,540.4 us | 143.48 us | 147.35 us | 1718.7500 |        - |  17.17 MB |
| Union3Except2          | 7,417.3 us | 146.55 us | 168.77 us | 1742.1875 |        - |  17.37 MB |
| Union3Except3          | 7,287.6 us | 104.65 us |  97.89 us | 1664.0625 |        - |  16.63 MB |
| Union3Except4          | 7,155.9 us | 134.43 us | 125.75 us | 1703.1250 |        - |  17.01 MB |


*/