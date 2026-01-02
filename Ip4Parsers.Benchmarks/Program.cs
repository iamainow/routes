using BenchmarkDotNet.Running;

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);

// To run: dotnet run --project Ip4Parsers.Benchmarks -c Release
// Note: Power plan changes are disabled via the [Config(typeof(NoPowerPlanConfig))] attribute
// on the Ip4RangeSetBenchmarks class to prevent Windows power plan modifications during benchmarking

/*
| Method                    | Count   | Mean       | Error     | StdDev    | Gen0    | Allocated   |
|-------------------------- |-------- |-----------:|----------:|----------:|--------:|------------:|
| RealisticGetRanges        | 1000000 |   1.395 ms | 0.0123 ms | 0.0115 ms | 11.7188 |    256.3 KB |
| ParseAddressesByGetRanges | 1000000 | 185.765 ms | 2.3335 ms | 2.1827 ms |       - | 16384.45 KB |
| ParseRangesByGetRanges    | 1000000 | 248.535 ms | 3.6126 ms | 3.3792 ms |       - | 16384.45 KB |
| ParseSubnetsByGetRanges   | 1000000 | 168.843 ms | 1.8746 ms | 1.4636 ms |       - | 16384.45 KB |
*/