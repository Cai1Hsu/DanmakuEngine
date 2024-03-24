namespace DanmakuEngine.Engine.Platform.Environments;

public static class Env
{
    public static string? GetString<TEnv>()
        where TEnv : IEnvironmentVariable, new()
        => new TEnv().Get();

    public static TResult? Get<TEnv, TResult>()
        where TEnv : IEnvironmentVariable<TResult>, new()
        where TResult : struct, Enum
        => new TEnv().Value;
}
