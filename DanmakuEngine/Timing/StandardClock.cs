namespace DanmakuEngine.Timing;

public class StandardClock : IClock
{
    public double DeltaTime => Time.UpdateDelta;

    public double ElapsedSeconds => Time.ElapsedSeconds;

    public bool IsPaused => false;
}
