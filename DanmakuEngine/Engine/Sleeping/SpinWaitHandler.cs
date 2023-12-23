
using System.Diagnostics;

namespace DanmakuEngine.Engine.Sleeping;

public class SpinWaitHandler : IWaitHandler
{
    private readonly SpinWait _spinWait = new();
    private readonly Stopwatch _stopwatch = new();

    public bool IsHighResolution => Stopwatch.IsHighResolution;

    public void Register()
    {
        _stopwatch.Reset();
    }

    public void Wait(double milliseconds)
        => Wait(TimeSpan.FromMilliseconds(milliseconds));

    public void Wait(TimeSpan timeSpan)
    {
        _stopwatch.Restart();

        while (_stopwatch.Elapsed < timeSpan)
            _spinWait.SpinOnce();

        _stopwatch.Stop();
    }
}
