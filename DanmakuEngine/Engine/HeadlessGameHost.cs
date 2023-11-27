using System.Diagnostics;
using DanmakuEngine.Logging;

namespace DanmakuEngine.Engine;

public class HeadlessGameHost : GameHost
{
    private Func<bool> Running { get; }

    private double refreshRate = 60;

    private double averageWaitTime => 1 / refreshRate;

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

        long LastWaitTicks = sw.ElapsedTicks;

        while (isRunning && Running())
        {
            long currentTicks = sw.ElapsedTicks;

            UpdateDelta = (currentTicks - lastUpdateTicks) / (double)Stopwatch.Frequency;
            RenderDelta = (currentTicks - lastRenderTicks) / (double)Stopwatch.Frequency;

            UpdateTime(RenderDelta);

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
                SpinWait.SpinUntil(() => (sw.ElapsedTicks - LastWaitTicks) / (double)Stopwatch.Frequency > waitTime);
            }

            LastWaitTicks = sw.ElapsedTicks;
        }
    }
}