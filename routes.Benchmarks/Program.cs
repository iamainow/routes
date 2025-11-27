using BenchmarkDotNet.Running;
using routes.Benchmarks;

// Run benchmarks with configuration that prevents power plan changes
// The NoPowerPlanConfig attribute on the benchmark class handles this
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



| Method    | Job            | Runtime        | Mean         | Error       | StdDev      | Gen0       | Gen1     | Gen2     | Allocated |
|---------- |--------------- |--------------- |-------------:|------------:|------------:|-----------:|---------:|---------:|----------:|
| Realistic | .NET 10.0      | .NET 10.0      | 356,814.2 us | 4,999.76 us | 4,432.16 us |  1000.0000 |        - |        - |   10.9 MB |
| o10k      | .NET 10.0      | .NET 10.0      |     799.3 us |     8.81 us |     7.36 us |   173.8281 |   9.7656 |        - |   1.39 MB |
| o100k     | .NET 10.0      | .NET 10.0      |   9,402.1 us |    89.99 us |    75.15 us |  1750.0000 |  78.1250 |        - |  14.01 MB |
| o1000k    | .NET 10.0      | .NET 10.0      |  96,738.5 us | 1,098.46 us |   973.76 us | 17666.6667 | 500.0000 | 166.6667 | 139.65 MB |
| Realistic | NativeAOT 10.0 | NativeAOT 10.0 |           NA |          NA |          NA |         NA |       NA |       NA |        NA |
| o10k      | NativeAOT 10.0 | NativeAOT 10.0 |           NA |          NA |          NA |         NA |       NA |       NA |        NA |
| o100k     | NativeAOT 10.0 | NativeAOT 10.0 |           NA |          NA |          NA |         NA |       NA |       NA |        NA |
| o1000k    | NativeAOT 10.0 | NativeAOT 10.0 |           NA |          NA |          NA |         NA |       NA |       NA |        NA |



| Method    | Job            | Runtime        | Mean         | Error       | StdDev       | Gen0       | Gen1     | Gen2     | Allocated |
|---------- |--------------- |--------------- |-------------:|------------:|-------------:|-----------:|---------:|---------:|----------:|
| Realistic | .NET 10.0      | .NET 10.0      | 515,697.5 us | 9,794.04 us | 10,057.76 us |          - |        - |        - |   10.9 MB |
| o10k      | .NET 10.0      | .NET 10.0      |     708.6 us |    13.30 us |     13.06 us |   138.6719 |   7.8125 |        - |   1.39 MB |
| o100k     | .NET 10.0      | .NET 10.0      |   8,937.4 us |    93.43 us |     82.83 us |  1390.6250 |  31.2500 |        - |  14.01 MB |
| o1000k    | .NET 10.0      | .NET 10.0      |  91,436.0 us | 1,765.48 us |  1,733.94 us | 13666.6667 | 833.3333 | 500.0000 | 139.65 MB |
| Realistic | NativeAOT 10.0 | NativeAOT 10.0 |           NA |          NA |           NA |         NA |       NA |       NA |        NA |
| o10k      | NativeAOT 10.0 | NativeAOT 10.0 |           NA |          NA |           NA |         NA |       NA |       NA |        NA |
| o100k     | NativeAOT 10.0 | NativeAOT 10.0 |           NA |          NA |           NA |         NA |       NA |       NA |        NA |
| o1000k    | NativeAOT 10.0 | NativeAOT 10.0 |           NA |          NA |           NA |         NA |       NA |       NA |        NA |



| Method    | Job            | PowerPlanMode                        | Runtime        | Mean         | Error        | StdDev       | Gen0       | Gen1     | Gen2     | Allocated |
|---------- |--------------- |------------------------------------- |--------------- |-------------:|-------------:|-------------:|-----------:|---------:|---------:|----------:|
| Realistic | Job-YIJHXA     | 67b4a053-3646-4532-affd-0535c9ea82a7 | NativeAOT 10.0 | 564,019.6 us | 11,166.50 us |  9,898.81 us |  1000.0000 |        - |        - |   10.9 MB |
| o10k      | Job-YIJHXA     | 67b4a053-3646-4532-affd-0535c9ea82a7 | NativeAOT 10.0 |   1,049.6 us |     18.45 us |     16.35 us |   132.8125 |   5.8594 |        - |   1.33 MB |
| o100k     | Job-YIJHXA     | 67b4a053-3646-4532-affd-0535c9ea82a7 | NativeAOT 10.0 |  13,014.1 us |    184.48 us |    154.05 us |  1343.7500 |  78.1250 |        - |  13.44 MB |
| o1000k    | Job-YIJHXA     | 67b4a053-3646-4532-affd-0535c9ea82a7 | NativeAOT 10.0 | 135,857.6 us |  2,683.82 us |  3,762.34 us | 13500.0000 | 750.0000 | 250.0000 | 133.93 MB |
| Realistic | .NET 10.0      | 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c | .NET 10.0      | 515,647.9 us |  9,025.23 us |  8,000.63 us |  1000.0000 |        - |        - |   10.9 MB |
| o10k      | .NET 10.0      | 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c | .NET 10.0      |     706.6 us |     11.95 us |     13.28 us |   138.6719 |   7.8125 |        - |   1.39 MB |
| o100k     | .NET 10.0      | 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c | .NET 10.0      |   8,877.9 us |    125.36 us |    104.68 us |  1390.6250 |  31.2500 |        - |  14.01 MB |
| o1000k    | .NET 10.0      | 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c | .NET 10.0      |  94,099.1 us |  1,794.84 us |  2,740.91 us | 14166.6667 | 500.0000 | 166.6667 | 139.65 MB |
| Realistic | NativeAOT 10.0 | 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c | NativeAOT 10.0 | 570,658.5 us | 11,387.24 us | 13,555.70 us |  1000.0000 |        - |        - |   10.9 MB |
| o10k      | NativeAOT 10.0 | 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c | NativeAOT 10.0 |   1,041.6 us |     17.69 us |     14.77 us |   132.8125 |   5.8594 |        - |   1.33 MB |
| o100k     | NativeAOT 10.0 | 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c | NativeAOT 10.0 |  13,270.5 us |    258.15 us |    241.47 us |  1343.7500 |  78.1250 |        - |  13.44 MB |
| o1000k    | NativeAOT 10.0 | 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c | NativeAOT 10.0 | 135,801.7 us |  2,697.88 us |  4,725.11 us | 13500.0000 | 750.0000 | 250.0000 | 133.93 MB |
*/