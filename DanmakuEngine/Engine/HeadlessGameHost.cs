using System.Diagnostics;
using DanmakuEngine.Logging;
using DanmakuEngine.Timing;

namespace DanmakuEngine.Engine;

public class HeadlessGameHost : GameHost
{
    private Func<bool> Running { get; }

    private double refreshRate = 60;

    private double averageWaitTime => 1 / refreshRate;

    public Action? OnUpdate;

    /// <summary>
    /// Create an instance of HeadlessGameHost with a specified timeout
    ///
    /// NOTE: The timer starts when you create the instance instead of when you actually run the host.
    /// </summary>
    /// <param name="Timeout">timeout in ms</param>
    public HeadlessGameHost(double Timeout)
    {
        Stopwatch sw = new Stopwatch();

        sw.Start();

        Running = () => sw.ElapsedMilliseconds < Timeout;
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
        // TODO: Reimplement this and GameHost.HandleMessages()

        long LastWaitTicks = HostTimer.ElapsedTicks;

        while (isRunning && Running())
        {
            long currentTicks = HostTimer.ElapsedTicks;

            UpdateDelta = (currentTicks - lastUpdateTicks) / (double)Stopwatch.Frequency;
            RenderDelta = (currentTicks - lastRenderTicks) / (double)Stopwatch.Frequency;

            UpdateTime(RenderDelta);

            OnUpdate?.Invoke();
            DoUpdate();

            lastUpdateTicks = currentTicks;

            // we don't render in headless mode
            // DoRender();

            lastRenderTicks = currentTicks;

            // Wait for sync
            // This is a very simple sync method, only to prevent the CPU from running at 100%

            // Only do this when the wait time is greater than 1ms
            var waitTime = averageWaitTime - UpdateDelta;
            if (waitTime > 1E-3)
            {
                SpinWait.SpinUntil(() => (HostTimer.ElapsedTicks - LastWaitTicks) / (double)Stopwatch.Frequency > waitTime);
            }

            LastWaitTicks = HostTimer.ElapsedTicks;
        }
    }
}