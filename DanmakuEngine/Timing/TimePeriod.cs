namespace DanmakuEngine.Timing;

public class TimePeriod
{
    private double _startSeconds;

    public double StartSeconds => _startSeconds;

    public TimePeriod(double startSeconds)
    {
        _startSeconds = startSeconds;
    }

    public double Elapsed(double now)
    {
        return now - _startSeconds;
    }

    public static TimePeriod NewPeriod(double startSeconds)
    {
        return new TimePeriod(startSeconds);
    }

    public void New(double startSeconds)
    {
        _startSeconds = startSeconds;
    }
}
