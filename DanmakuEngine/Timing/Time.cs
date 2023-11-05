namespace DanmakuEngine.Timing;

public class Time
{
    private double count_time = 0;
    private int count_frame = 0;

    public double ActualFPS { get; private set; }

    protected void UpdateFps(double delta)
    {
        count_time += delta;
        count_frame++;

        if (count_time < 1)
            return;

        ActualFPS = count_frame / count_time;

        count_frame = 0;
        count_time = 0;
    }

    public static double RenderDelta { get; protected set; }
    public static double UpdateDelta { get; protected set; }
}