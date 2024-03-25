namespace DanmakuEngine.Engine.Platform.Environments.Threading;

public class ThreadingModeEnv : IEnvironmentVariable<ThreadingMode>
{
    private const string env_var = "DE_THREADING_MODE";

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

    ThreadingMode? IEnvironmentVariable<ThreadingMode>.Value
    {
        get
        {
            var value = Environment.GetEnvironmentVariable(env_var);

            if (value is null)
                return null;

            if (single_threadeds.Any(v => v.Equals(value, StringComparison.OrdinalIgnoreCase)))
                return ThreadingMode.SingleThreaded;

            if (multi_threadeds.Any(v => v.Equals(value, StringComparison.OrdinalIgnoreCase)))
                return ThreadingMode.MultiThreaded;

#if DEBUG
            throw new NotSupportedException($"Unrecognized value \"{value}\" for environment variable \"{env_var}\"");
#else
            return null;
#endif
        }
    }
}
