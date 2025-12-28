using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;

namespace routes.Benchmarks;

public class NoPowerPlanConfig : ManualConfig
{
    public NoPowerPlanConfig()
    {
        // Explicitly use the user's current power plan to prevent BenchmarkDotNet
        // from changing the Windows power plan during benchmark execution
        AddJob(Job.Default
            .DontEnforcePowerPlan()
            .WithRuntime(CoreRuntime.Core10_0))
            .WithOptions(ConfigOptions.DisableOptimizationsValidator);

        //AddJob(Job.Default
        //    .DontEnforcePowerPlan()
        //    .WithRuntime(NativeAotRuntime.Net10_0));
    }
}