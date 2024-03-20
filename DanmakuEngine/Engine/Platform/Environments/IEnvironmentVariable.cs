namespace DanmakuEngine.Engine.Platform.Environments;

public interface IEnvironmentVariable
{
    public string? Get();
}

public interface IEnvironmentVariable<T>
    where T : struct
{
    Nullable<T> Value { get; }
}
