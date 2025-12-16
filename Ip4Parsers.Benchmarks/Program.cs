using BenchmarkDotNet.Running;
using Ip4Parsers.Benchmarks;

BenchmarkRunner.Run<Ip4ParsersBenchmarks>();

// To run: dotnet run --project routes.Benchmarks -c Release
// Note: Power plan changes are disabled via the [Config(typeof(NoPowerPlanConfig))] attribute
// on the Ip4RangeSetBenchmarks class to prevent Windows power plan modifications during benchmarking

/*
*/