using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Running;

namespace DanmakuEngine.Benchmarks;

public static class Program
{
    public static void Main(string[] args)
    {
        BenchmarkSwitcher
            .FromAssembly(typeof(Program).Assembly)
            .Run(args, DefaultConfig.Instance
                .WithOption(ConfigOptions.DisableOptimizationsValidator, true)
                .AddDiagnoser(MemoryDiagnoser.Default)
            );
    }
}
