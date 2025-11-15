using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using routes.Benchmarks;

var config = ManualConfig.Create(DefaultConfig.Instance)
    .AddJob(Job.Default
        .WithRuntime(CoreRuntime.Core90)
        .WithId("net9.0"))
    .AddJob(Job.Default
        .WithRuntime(CoreRuntime.Core10_0)
        .WithId("net10.0"));

BenchmarkRunner.Run<Ip4RangeSetBenchmarks>(config);
