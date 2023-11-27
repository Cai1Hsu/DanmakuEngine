using System.Diagnostics;

namespace DanmakuEngine.Engine;

public class HeadlessGameHost : GameHost
{
    private Func<bool> Running { get; }

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
    }

    public override void HandleMessages()
    {
        // TODO: Reimplement this and GameHost.HandleMessages()

        SpinWait.SpinUntil(() =>
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

            return !isRunning || !Running();
        });
    }
}