using System.Diagnostics;
using DanmakuEngine.Allocations;
using DanmakuEngine.Timing;

namespace DanmakuEngine.Scheduling;

public class FrameExecutor(Action onFrame)
{
    protected readonly Action runFrame = onFrame;

    public double DeltaTime => CurrentTime - LastFrameTime;

    public double SourceTime => clock.ElapsedSeconds;

    public virtual double CurrentTime { get; protected set; }

    public virtual double LastFrameTime { get; protected set; }

    protected RingPool<double> _delta_pool = new(128);

    public double Jitter { get; private set; }

    public double AverageFramerate { get; private set; } = 1000; // an optimistic initial value

    public double CountCooldown { get; set; } = 1;

    public long FrameCount { get; set; } = 0;

    protected double count_time = 0;
    protected int count_frame = 0;

    protected readonly StopwatchClock clock = new();

    public StopwatchClock TimeSource => clock;

    public virtual void RunFrame()
    {
        ++FrameCount;

        if (clock.IsPaused)
            clock.Start();
        else
            clock.Update();

        // do work
        runFrame.Invoke();

        _delta_pool.CurrentAndNext = clock.DeltaTime;

        // standard deviation
        double avg = _delta_pool.Average();
        Jitter = Math.Sqrt(_delta_pool.Average(v => Math.Pow(v - avg, 2)));

        if (count_time < CountCooldown)
        {
            ++count_frame;
            count_time += DeltaTime;
        }
        else
        {
            AverageFramerate = count_frame / count_time;

            count_frame = 0;
            count_time = 0;
        }

        LastFrameTime = CurrentTime;
        CurrentTime = SourceTime;
    }
}
