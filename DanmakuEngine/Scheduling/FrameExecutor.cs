using DanmakuEngine.Allocations;
using DanmakuEngine.Timing;

namespace DanmakuEngine.Scheduling;

public class FrameExecutor(Action<double> task)
{
    protected readonly Action<double> runFrame = task;

    public double DeltaTime => clock.UpdateDelta;

    public virtual double CurrentTime { get; protected set; }
    
    public virtual double LastFrameTime { get; protected set; }

    protected RingPool<double> _delta_pool = new(128);

    public double Jitter { get; private set; }

    public double AverageFramerate { get; private set; }

    public double CountCooldown { get; set; } = 1;

    protected double count_time = 0;
    protected int count_frame = 0;

    protected StopwatchClock clock = new();

    public virtual void RunNextFrame()
    {
        clock.Update();

        // do work
        runFrame.Invoke(DeltaTime);

        _delta_pool.CurrentAndNext = clock.UpdateDelta;

        // standard deviation
        double avg = _delta_pool.Get().Average();
        Jitter = Math.Sqrt(_delta_pool.Get().Average(v => Math.Pow(v - avg, 2)));

        if (count_time < CountCooldown)
        {
            ++count_frame;
            ++count_time;
        }
        else
        {
            AverageFramerate = count_frame / count_time;

            count_frame = 0;
            count_time = 0;
        }

        LastFrameTime = CurrentTime;
        CurrentTime = clock.CurrentTime;
    }
}