using System.Diagnostics;
using DanmakuEngine.Engine.Threading;
using DanmakuEngine.Logging;
using DanmakuEngine.Threading;
using DanmakuEngine.Timing;

namespace DanmakuEngine.Engine;

public class HeadlessGameHost : GameHost
{
    private Func<bool> Running { get; } = null!;

    private double refreshRate = 60;

    private double averageWaitTime => 1 / refreshRate;

    public Action<HeadlessGameHost>? OnUpdate;

    public Action<HeadlessGameHost>? OnLoad;

    private bool limitTime = false;

    public Action? OnTimedout;

    public bool ThrowOnTimedOut = true;

    public bool IgnoreTimedout = false;

    /// <summary>
    /// Bypass the wait for sync, if you want high refresh rate
    /// </summary>
    public bool BypassWaitForSync = false;

    private double timeout = 0;

    /// <summary>
    /// Create an instance of HeadlessGameHost with a specified timeout
    /// </summary>
    /// <param name="timeout">timeout in ms</param>
    public HeadlessGameHost(double timeout)
    {
        this.timeout = timeout;

        limitTime = true;
    }

    public HeadlessGameHost(Func<bool> running)
    {
        this.Running = running;
    }

    public override void SetUpSdl()
    {
        // Do nothing
    }

    public override void SetUpWindowAndRenderer()
    {
        // Do nothing
    }

    public override void RegisterEvents()
    {
        // Do nothing

        refreshRate = ConfigManager.RefreshRate;
    }

    public override void RegisterThreads()
    {
        base.RegisterThreads();

        // we don't handle messages in HeadlessGameHost
        threadRunner.Remove(MainThread);

        // We don't render in HeadlessGameHost
        threadRunner.Remove(RenderThread);
    }

    public override void RunMainLoop()
    {
        OnLoad?.Invoke(this);

        long lastWaitTicks = ElapsedTicks;

        while (isRunning
            && (Running is null || Running.Invoke())
            && (!limitTime || ElapsedMilliseconds < timeout))
        {
            threadRunner.RunMainLoop();

            if (!BypassWaitForSync && ConfigManager.Vsync)
            {
                // Only do this when the wait time is greater than 1ms
                var waitTime = averageWaitTime - UpdateDelta;
                if (waitTime > 1E-3)
                {
                    SpinWait.SpinUntil(() => (ElapsedTicks - lastWaitTicks) / (double)Stopwatch.Frequency > waitTime);
                }
            }

            lastWaitTicks = ElapsedTicks;
        }

        // The host exited with timed out
        if (limitTime && ElapsedMilliseconds >= timeout)
        {
            if (!IgnoreTimedout)
                Logger.Error("[HeadlessGameHost] timed out");

            OnTimedout?.Invoke();

            if (ThrowOnTimedOut)
                throw new Exception($"Reached time limit({timeout} ms) when running HeadlessGameHost");
        }
    }
}
