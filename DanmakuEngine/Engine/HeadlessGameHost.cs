using System.Diagnostics;
using DanmakuEngine.Engine.Threading;
using DanmakuEngine.Logging;
using DanmakuEngine.Scheduling;
using DanmakuEngine.Threading;
using DanmakuEngine.Timing;

namespace DanmakuEngine.Engine;

public class HeadlessGameHost : GameHost
{
    private volatile bool isRunning = true;
    private Func<bool> Running { get; } = null!;

    public event Action<HeadlessGameHost>? OnUpdate;

    public event Action<HeadlessGameHost>? OnLoad;

    private bool limitTime = false;

    public event Action OnTimedout = null!;

    public bool ThrowOnTimedOut = true;

    public bool IgnoreTimedout = false;

    private double timeout = 0;

    public long CurrentFrame => UpdateThread.Executor.FrameCount;

    public bool SkipFirstFrame { get; set; } = false;


    /// <summary>
    /// Create an instance of HeadlessGameHost with a specified timeout
    /// </summary>
    /// <param name="timeout">timeout in ms</param>
    public HeadlessGameHost(double timeout)
        : base()
    {
        this.timeout = timeout;

        limitTime = true;
    }

    public HeadlessGameHost(Func<bool> running)
        : base()
    {
        this.Running = running;
    }

    public override void SetUpWindowAndRenderer()
    {
        // Do nothing
    }

    public override void RegisterEvents()
    {
        // Do nothing
    }

    public new void RequestClose()
    {
        isRunning = false;
    }

    public void RunMainLoop()
    {
        Root.OnStart += _ => OnLoad?.Invoke(this);

        Root.OnUpdate += _ =>
        {
            if (SkipFirstFrame && CurrentFrame == 1)
                return;

            OnUpdate?.Invoke(this);
        };

        if (HasConsole())
        {
            Console.CancelKeyPress += (_, _) => isRunning = false;
        }

        while (isRunning
            && (Running is null || Running.Invoke())
            && (!limitTime || EngineTimer.ElapsedMilliseconds < timeout))
        {
            threadRunner.RunMainLoop();
        }

        // The host exited with timed out
        if (limitTime && EngineTimer.ElapsedMilliseconds >= timeout)
        {
            if (!IgnoreTimedout)
                Logger.Error("[HeadlessGameHost] timed out");

            OnTimedout?.Invoke();

            if (ThrowOnTimedOut)
                throw new Exception($"Reached time limit({timeout} ms) when running HeadlessGameHost");
        }
    }
}
