using System.Diagnostics;
using DanmakuEngine.Engine.Threading;
using DanmakuEngine.Logging;
using DanmakuEngine.Scheduling;
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
        threadRunner.MultiThreaded.BindTo(MultiThreaded);

        registerThread(UpdateThread = new(delta =>
        {
            Update(delta);

            OnUpdate?.Invoke(this);
        }));
    }

    public override void RunMainLoop()
    {
        OnLoad?.Invoke(this);

        long lastWaitTicks = ElapsedTicks;

        ThrottledExecutor executor = new(_ => threadRunner.RunMainLoop())
        {
            ActiveHz = ConfigManager.Vsync ? refreshRate : 1000,
        };

        while (isRunning
            && (Running is null || Running.Invoke())
            && (!limitTime || ElapsedMilliseconds < timeout))
        {
            executor.RunNextFrame();
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
