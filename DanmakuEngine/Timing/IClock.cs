namespace DanmakuEngine.Timing;

public interface IClock
{
    public double DeltaTime { get; }

    public double ElapsedSeconds { get; }

    public bool IsPaused { get; }
}
