using System.Diagnostics;
using DanmakuEngine.Logging;
using DanmakuEngine.Timing;

namespace DanmakuEngine.Engine;

public class HeadlessGameHost : GameHost
{
    private Func<bool> Running { get; } = null!;

    private double refreshRate = 60;

    private double averageWaitTime => 1 / refreshRate;

    public Action<HeadlessGameHost>? OnUpdate;

    public Action<HeadlessGameHost>? OnLoad;

    public Action? OnTimedout;

    public bool ThrowOnTimedOut = true;

    public bool IgnoreTimedout = false;

    /// <summary>
    /// Bypass the wait for sync, if you want high refresh rate
    /// </summary>
    public bool BypassWaitForSync = false;

    private Stopwatch timer = null!;

    private double timeout = 0;

    /// <summary>
    /// Create an instance of HeadlessGameHost with a specified timeout
    /// </summary>
    /// <param name="timeout">timeout in ms</param>
    public HeadlessGameHost(double timeout)
    {
        timer = new Stopwatch();

        this.timeout = timeout;
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

    public override void RunMainLoop()
    {
        OnLoad?.Invoke(this);

        bool empty_screens = screens.Empty();

        // TODO: Reimplement this and GameHost.HandleMessages()

        timer?.Start();

        long LastWaitTicks = HostTimer.ElapsedTicks;

        while (isRunning
            && (Running is null || Running.Invoke())
            && (timer is null || timer.ElapsedMilliseconds < timeout))
        {
            long currentTicks = HostTimer.ElapsedTicks;

            UpdateDelta = (currentTicks - lastUpdateTicks) / (double)Stopwatch.Frequency;
            RenderDelta = (currentTicks - lastRenderTicks) / (double)Stopwatch.Frequency;

            UpdateTime();

            OnUpdate?.Invoke(this);

            DoUpdate();

            lastUpdateTicks = currentTicks;

            // we don't render in headless mode
            // DoRender();

            lastRenderTicks = currentTicks;

            if (!BypassWaitForSync)
            {
                // Wait for sync
                // This is a very simple sync method, only to prevent the CPU from running at 100%

                // Only do this when the wait time is greater than 1ms
                var waitTime = averageWaitTime - UpdateDelta;
                if (waitTime > 1E-3)
                {
                    SpinWait.SpinUntil(() => (HostTimer.ElapsedTicks - LastWaitTicks) / (double)Stopwatch.Frequency > waitTime);
                }
            }

            LastWaitTicks = HostTimer.ElapsedTicks;
        }

        // The host exited with timed out
        if (timer is not null && timer.IsRunning && timer.ElapsedMilliseconds >= timeout)
        {
            if (!IgnoreTimedout)
                Logger.Error("[HeadlessGameHost] timed out");

            OnTimedout?.Invoke();

            if (ThrowOnTimedOut)
                throw new Exception($"Reached time limit({timeout} ms) when running HeadlessGameHost");
        }
    }
}
