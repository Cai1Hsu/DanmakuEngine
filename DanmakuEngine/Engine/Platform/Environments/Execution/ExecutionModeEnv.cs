namespace DanmakuEngine.Engine.Platform.Environments.Execution;

public class ExecutionModeEnv : IEnvironmentVariable<ExecutionMode>
{
    private const string env_var = "DE_EXECUTION_MODE";

    private static readonly IList<string> single_threadeds =
    [
        "s",
        "single",
        "singlethread",
        "single_thread",
        "single-thread",
        "singlethreaded",
        "single_threaded",
        "single-threaded",
    ];

    private static readonly IList<string> multi_threadeds =
    [
        "m",
        "multi",
        "multithread",
        "multi_thread",
        "multi-thread",
        "multithreaded",
        "multi_threaded",
        "multi-threaded",
    ];

    ExecutionMode? IEnvironmentVariable<ExecutionMode>.Value
    {
        get
        {
            var value = Environment.GetEnvironmentVariable(env_var);

            if (value is null)
                return null;

            if (single_threadeds.Any(v => v.Equals(value, StringComparison.OrdinalIgnoreCase)))
                return ExecutionMode.SingleThreaded;

            if (multi_threadeds.Any(v => v.Equals(value, StringComparison.OrdinalIgnoreCase)))
                return ExecutionMode.MultiThreaded;

#if DEBUG
            throw new NotSupportedException($"Unrecognized value \"{value}\" for environment variable \"{env_var}\"");
#else
            return null;
#endif
        }
    }
}
