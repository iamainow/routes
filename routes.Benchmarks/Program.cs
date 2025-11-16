using BenchmarkDotNet.Running;
using routes.Benchmarks;

BenchmarkRunner.Run<Ip4RangeSetBenchmarks>();

// dotnet run --project routes.Benchmarks -c Release --framework net10.0 net9.0 net8.0

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
*/