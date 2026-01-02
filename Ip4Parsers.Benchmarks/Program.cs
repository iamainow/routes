using BenchmarkDotNet.Running;

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);

// To run: dotnet run --project Ip4Parsers.Benchmarks -c Release