using BenchmarkDotNet.Running;
using routes.Benchmarks;

BenchmarkRunner.Run<Ip4RangeSetBenchmarks>();

// dotnet run --project routes.Benchmarks -c Release --framework net10.0 net9.0 net8.0

/*
| Method | Job       | Runtime   | Mean    | Error    | StdDev   | Ratio | RatioSD | Gen0        | Gen1       | Gen2       | Allocated | Alloc Ratio |
|------- |---------- |---------- |--------:|---------:|---------:|------:|--------:|------------:|-----------:|-----------:|----------:|------------:|
| Test   | .NET 8.0  | .NET 8.0  | 3.002 s | 0.0433 s | 0.0405 s |  1.18 |    0.02 | 704000.0000 | 62000.0000 | 11000.0000 |   7.06 GB |        1.00 |
| Test   | .NET 9.0  | .NET 9.0  | 2.845 s | 0.0560 s | 0.0496 s |  1.12 |    0.03 | 705000.0000 | 55000.0000 | 11000.0000 |   7.06 GB |        1.00 |
| Test   | .NET 10.0 | .NET 10.0 | 2.547 s | 0.0482 s | 0.0427 s |  1.00 |    0.02 | 719000.0000 | 11000.0000 |  9000.0000 |   7.06 GB |        1.00 |
*/