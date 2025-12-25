using BenchmarkDotNet.Running;

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);

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

| Method                 | Mean       | Error    | StdDev   | Gen0      | Gen1     | Allocated   |
|----------------------- |-----------:|---------:|---------:|----------:|---------:|------------:|
| Realistic              | 6,242.3 us | 68.32 us | 63.91 us | 1226.5625 | 570.3125 | 12532.37 KB |
| RealisticWithoutParser |   387.0 us |  3.69 us |  3.45 us |  101.5625 |  50.2930 |  1039.55 KB |
| Union4Except4          | 1,728.7 us | 21.08 us | 19.72 us |   25.3906 |        - |   270.89 KB |
| Union5Except4          | 1,632.7 us | 32.33 us | 28.66 us |   17.5781 |        - |   187.09 KB |

| Method                 | Mean       | Error    | StdDev   | Gen0      | Gen1     | Allocated   |
|----------------------- |-----------:|---------:|---------:|----------:|---------:|------------:|
| Realistic              | 5,845.3 us | 42.65 us | 33.30 us | 1226.5625 | 570.3125 | 12532.37 KB |
| RealisticWithoutParser |   382.0 us |  7.12 us |  6.66 us |  101.5625 |  49.8047 |  1039.55 KB |
| UnionExcept            | 1,847.1 us | 31.28 us | 27.73 us |   29.2969 |        - |   300.64 KB |

| Method                    | Mean         | Error      | StdDev     | Gen0      | Gen1     | Allocated   |
|-------------------------- |-------------:|-----------:|-----------:|----------:|---------:|------------:|
| Realistic                 |  6,310.12 us | 105.281 us |  98.480 us | 1226.5625 | 570.3125 | 12532.37 KB |
| RealisticWithoutParser    |    396.00 us |   5.780 us |   4.826 us |  101.5625 |  50.2930 |  1039.55 KB |
| UnionDisjointBaseline     |     35.13 us |   0.677 us |   0.665 us |   12.5122 |        - |   128.19 KB |
| UnionDisjointOptimized    |     70.23 us |   1.336 us |   1.539 us |   26.0010 |   0.1221 |   266.47 KB |
| UnionOverlappingBaseline  |     42.07 us |   0.762 us |   0.713 us |   12.0850 |        - |    123.5 KB |
| UnionOverlappingOptimized |     42.84 us |   0.841 us |   1.359 us |   17.3340 |   0.0610 |   177.41 KB |
| ExceptMixedBaseline       |     39.31 us |   0.514 us |   0.456 us |   16.2964 |   0.0610 |   166.47 KB |
| ExceptMixedOptimized      |     66.49 us |   1.295 us |   2.234 us |   24.7803 |   0.1221 |   253.19 KB |
| MixedLoopBaseline         |  5,581.81 us |  85.569 us |  80.041 us | 2000.0000 |        - | 20435.53 KB |
| MixedLoopOptimized        | 19,187.01 us | 374.366 us | 445.656 us | 6968.7500 |        - | 71460.43 KB |
| UnionExcept               |  1,857.68 us |  34.284 us |  42.104 us |   23.4375 |        - |   254.05 KB |

| Method                 | Mean       | Error    | StdDev   | Gen0     | Gen1    | Gen2    | Allocated  |
|----------------------- |-----------:|---------:|---------:|---------:|--------:|--------:|-----------:|
| Realistic              | 1,810.9 us | 33.47 us | 31.30 us | 130.8594 | 64.4531 | 11.7188 |  1362.7 KB |
| RealisticWithoutParser |   382.0 us |  5.54 us |  5.18 us | 101.5625 | 49.8047 |       - | 1039.45 KB |
| UnionExcept            | 1,804.7 us | 31.93 us | 28.31 us |  27.3438 |       - |       - |  280.72 KB |

| Method                 | Mean       | Error    | StdDev   | Gen0     | Gen1    | Gen2    | Allocated  |
|----------------------- |-----------:|---------:|---------:|---------:|--------:|--------:|-----------:|
| Realistic              | 1,811.8 us | 20.14 us | 16.82 us | 130.8594 | 64.4531 | 11.7188 |  1362.7 KB |
| RealisticWithoutParser |   381.5 us |  5.05 us |  4.72 us | 101.5625 | 50.2930 |       - | 1039.45 KB |
| UnionExcept            | 1,769.7 us | 29.28 us | 27.39 us |  25.3906 |       - |       - |  270.42 KB |







| Method             | Count  | Mean         | Error     | StdDev    | Exceptions | Allocated |
|------------------- |------- |-------------:|----------:|----------:|-----------:|----------:|
| Ip4RangeSet_Except | 1000   |     6.341 us | 0.1207 us | 0.1129 us |          - |         - |
| Ip4RangeSet_Except | 10000  |    65.489 us | 0.2816 us | 0.2496 us |          - |         - |
| Ip4RangeSet_Except | 100000 | 1,806.483 us | 4.6287 us | 3.6138 us |          - |         - |

| Method                             | Mean       | Error    | StdDev   | Exceptions | Gen0     | Gen1     | Gen2    | Allocated |
|----------------------------------- |-----------:|---------:|---------:|-----------:|---------:|---------:|--------:|----------:|
| Ip4RangeSet_Realistic              | 1,828.8 us | 36.11 us | 51.79 us |          - | 166.0156 |  82.0313 | 41.0156 |   1.33 MB |
| Ip4RangeSet_RealisticWithoutParser |   399.9 us |  7.70 us | 11.04 us |          - | 126.9531 | 108.3984 |       - |   1.02 MB |

 Method            | Count  | Mean        | Error     | StdDev    | Exceptions | Allocated |
|------------------ |------- |------------:|----------:|----------:|-----------:|----------:|
| Ip4RangeSet_Union | 1000   |    10.21 us |  0.202 us |  0.224 us |          - |         - |
| Ip4RangeSet_Union | 10000  |   186.81 us |  2.642 us |  2.472 us |          - |         - |
| Ip4RangeSet_Union | 100000 | 2,314.50 us | 44.760 us | 39.679 us |          - |         - |


| Method                  | Count  | Mean        | Error     | StdDev    | Exceptions | Gen0    | Allocated |
|------------------------ |------- |------------:|----------:|----------:|-----------:|--------:|----------:|
| Ip4RangeSet_UnionExcept | 1000   |    10.79 us |  0.209 us |  0.224 us |          - |  1.4801 |  12.13 KB |
| Ip4RangeSet_UnionExcept | 10000  |   122.07 us |  1.231 us |  1.152 us |          - |  8.0566 |  65.99 KB |
| Ip4RangeSet_UnionExcept | 100000 | 1,262.24 us | 14.890 us | 13.200 us |          - | 46.8750 | 392.98 KB |

| Method                       | Count  | Mean     | Error   | StdDev  | Gen0    | Exceptions | Allocated |
|----------------------------- |------- |---------:|--------:|--------:|--------:|-----------:|----------:|
| Ip4RangeSetStackAlloc_Except | 1000   | 244.1 us | 4.87 us | 4.56 us | 20.5078 |          - | 167.81 KB |
| Ip4RangeSetStackAlloc_Except | 10000  |       NA |      NA |      NA |      NA |         NA |        NA |
| Ip4RangeSetStackAlloc_Except | 100000 |       NA |      NA |      NA |      NA |         NA |        NA |

| Method                                       | Mean       | Error    | StdDev   | Gen0    | Exceptions | Gen1    | Gen2    | Allocated |
|--------------------------------------------- |-----------:|---------:|---------:|--------:|-----------:|--------:|--------:|----------:|
| Ip4RangeSetStackAlloc_Realistic              | 1,464.2 us | 22.51 us | 19.95 us | 41.0156 |          - | 41.0156 | 41.0156 | 390.31 KB |
| Ip4RangeSetStackAlloc_RealisticWithoutParser |   123.1 us |  2.35 us |  2.80 us |  8.0566 |          - |  1.9531 |       - |  67.05 KB |

| Method                                    | Count  | Mean     | Error   | StdDev  | Gen0    | Exceptions | Allocated |
|------------------------------------------ |------- |---------:|--------:|--------:|--------:|-----------:|----------:|
| Ip4RangeSetStackAlloc_Union1              | 1000   | 377.5 us | 6.56 us | 6.14 us | 22.9492 |          - |  187.5 KB |
| Ip4RangeSetStackAlloc_Union2              | 1000   | 344.2 us | 5.70 us | 4.76 us | 15.1367 |          - |    125 KB |
| Ip4RangeSetStackAlloc_SmartUnionUnordered | 1000   | 271.6 us | 5.38 us | 6.80 us | 15.1367 |          - |    125 KB |
| Ip4RangeSetStackAlloc_ctor                | 1000   | 146.0 us | 2.49 us | 2.33 us |  7.5684 |          - |   62.5 KB |
| Ip4RangeSetStackAlloc_Union1              | 10000  |       NA |      NA |      NA |      NA |         NA |        NA |
| Ip4RangeSetStackAlloc_Union2              | 10000  |       NA |      NA |      NA |      NA |         NA |        NA |
| Ip4RangeSetStackAlloc_SmartUnionUnordered | 10000  |       NA |      NA |      NA |      NA |         NA |        NA |
| Ip4RangeSetStackAlloc_ctor                | 10000  |       NA |      NA |      NA |      NA |         NA |        NA |
| Ip4RangeSetStackAlloc_Union1              | 100000 |       NA |      NA |      NA |      NA |         NA |        NA |
| Ip4RangeSetStackAlloc_Union2              | 100000 |       NA |      NA |      NA |      NA |         NA |        NA |
| Ip4RangeSetStackAlloc_SmartUnionUnordered | 100000 |       NA |      NA |      NA |      NA |         NA |        NA |
| Ip4RangeSetStackAlloc_ctor                | 100000 |       NA |      NA |      NA |      NA |         NA |        NA |

| Method                                                  | Count  | Mean         | Error      | StdDev     | Exceptions | Gen0      | Allocated |
|-------------------------------------------------------- |------- |-------------:|-----------:|-----------:|-----------:|----------:|----------:|
| Ip4RangeSetStackAlloc_SmartUnionUnorderedExceptUnsorted | 1000   |     85.31 us |   1.069 us |   0.947 us |          - |   10.1318 |  83.41 KB |
| Ip4RangeSetStackAlloc_SmartUnionUnorderedExceptUnsorted | 10000  |  1,035.70 us |  15.671 us |  14.659 us |          - |  101.5625 | 832.43 KB |
| Ip4RangeSetStackAlloc_SmartUnionUnorderedExceptUnsorted | 100000 | 10,996.90 us | 195.253 us | 182.640 us |          - | 1015.6250 | 8338.5 KB |

*/