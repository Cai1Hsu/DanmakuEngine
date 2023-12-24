using System.Diagnostics;

namespace DanmakuEngine.Timing;

public class StopwatchClock : Stopwatch, IClock
{
    public double UpdateDelta => _elapsedTime;

    public double RenderDelta => UpdateDelta;

    public double CurrentTime
        => this.IsRunning ? ElapsedTicks / (double)Frequency : 0;

    private double _last_tick = 0;

    private double _elapsedTime = 0;

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

            return;
        }

        double _elapsedTicks = this.ElapsedTicks;

        _elapsedTime = (_elapsedTicks - _last_tick) / (double)Frequency;

        _last_tick = _elapsedTicks;
    }
}
