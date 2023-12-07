namespace DanmakuEngine.Timing;

public class BasicClock : IClock, ICanPause
{
    private double _startTime;

    private double _accumulatedTime = 0;

    private bool isPaused;

    public double UpdateDelta => isPaused ? 0 : Time.UpdateDelta;

    public double RenderDelta => isPaused ? 0 : Time.RenderDelta;

    public double CurrentTime
    => isPaused ? _accumulatedTime : Time.CurrentTime - _startTime + _accumulatedTime;

    public bool IsPaused => isPaused;

    public void Pause()
    {
        isPaused = true;

        _accumulatedTime = CurrentTime;
    }

    public void Reset()
    {
        _accumulatedTime = 0;
        _startTime = Time.CurrentTime;
        isPaused = true;
    }

    public void Resume()
    {
        _accumulatedTime += Time.CurrentTime - _startTime;
        _startTime = Time.CurrentTime;
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
