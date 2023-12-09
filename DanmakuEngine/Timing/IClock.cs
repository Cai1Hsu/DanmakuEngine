namespace DanmakuEngine.Timing;

public interface IClock
{
    public double UpdateDelta { get; }

    public double RenderDelta { get; }

    public double CurrentTime { get; }

    public virtual bool IsPaused => true;
}