using System.Diagnostics;

namespace DanmakuEngine.Timing;

public class StopwatchClock : Stopwatch, IClock
{
    public bool IsPaused => !IsRunning;

    public double ElapsedSeconds
        => IsPaused ? 0 : ElapsedTicks / (double)Frequency;

    public double DeltaTime { get; private set; }

    private long _last_tick = 0;

    public void BeginTimer()
    {
        Reset();
        Start();
    }

    public void ResetTime()
    {
        Reset();
        _last_tick = ElapsedTicks;
    }

    public void Update()
    {
        if (!IsRunning)
        {
            Start();
            _last_tick = ElapsedTicks;
            return;
        }

        long this_tick = ElapsedTicks;
        DeltaTime = (this_tick - _last_tick) / (double)Frequency;
        _last_tick = this_tick;
    }
}
