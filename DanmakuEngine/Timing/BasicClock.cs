namespace DanmakuEngine.Timing;

public class BasicClock : IClock, ICanPause
{
    private double _startTime;

    private double _accumulatedTime = 0;

    private bool isPaused;

    public double DeltaTime => isPaused ? 0 : Time.UpdateDelta;

    public double ElapsedSeconds
    => isPaused ? _accumulatedTime : Time.ElapsedSeconds - _startTime + _accumulatedTime;

    public bool IsPaused => isPaused;

    public void Pause()
    {
        isPaused = true;

        _accumulatedTime = ElapsedSeconds;
    }

    public void Reset()
    {
        _accumulatedTime = 0;
        _startTime = Time.ElapsedSeconds;
        isPaused = true;
    }

    public void Resume()
    {
        _accumulatedTime += Time.ElapsedSeconds - _startTime;
        _startTime = Time.ElapsedSeconds;
        isPaused = false;
    }

    public void Start()
    {
        Reset();

        isPaused = false;
    }

    public void Stop()
    {
        isPaused = true;
    }

    public BasicClock(bool start = false)
    {
        Reset();

        if (start)
            Start();
        else
            Pause();
    }
}
