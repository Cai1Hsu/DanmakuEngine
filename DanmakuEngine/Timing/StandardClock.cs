namespace DanmakuEngine.Timing;

public class StandardClock : IClock
{
    public double UpdateDelta => Time.UpdateDelta;

    public double RenderDelta => Time.RenderDelta;

    public double CurrentTime => Time.CurrentTime;
}