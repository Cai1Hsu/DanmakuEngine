namespace DanmakuEngine.Timing;

public class Time
{
    protected double count_time = 0;
    protected int count_frame = 0;

    public double ActualFPS { get; protected set; }

    public static double RenderDelta { get; protected set; }
    public static double UpdateDelta { get; protected set; }
}