using System.Diagnostics;
using DanmakuEngine.Engine.Threading;
using DanmakuEngine.Engine.Windowing;
using DanmakuEngine.Logging;
using DanmakuEngine.Threading;
using Silk.NET.Input;
using Silk.NET.SDL;

namespace DanmakuEngine.Engine;

public partial class GameHost
{
    protected ThreadRunner threadRunner = null!;

    public virtual void RegisterThreads()
    {
        threadRunner = window is not null ? new ThreadRunner(MainThread = new(window.PumpEvents))
                                          : new ThreadRunner(MainThread = new(() => { /* Run like the wind */ }));

        threadRunner.MultiThreaded.BindTo(MultiThreaded);

        threadRunner.AddThread(UpdateThread = new(Update));

        if (Renderer is not null)
        {
            threadRunner.AddThread(RenderThread = new(Render, Renderer));
        }

        foreach (var t in threadRunner.Threads)
        {
            t.Executor.CountCooldown = 1.0 / DebugFpsHz;
        }
    }

    public void RunUntilExit()
    {
        try
        {
            threadRunner.Start();

            State = EngineState.Running;

            if (window is not null)
            {
                window.RunWhile(threadRunner.RunMainLoop,
                                () => !ScreenStack.Empty(), false);
            }
            else
            {
                if (this is HeadlessGameHost headlessGameHost)
                    headlessGameHost.RunMainLoop();
                else
                    throw new Exception("No window was set up");
            }
        }
        catch (Exception e)
        {
            Logger.Error(e.Message);
        }
    }

    public virtual void RequestClose()
        => window?.RequestClose();

    /// <summary>
    /// Return true to prevent the event from being passed to the next handler
    /// </summary>
    public event Action<KeyboardEvent> KeyDown
    {
        add => window.KeyDown += value;
        remove => window.KeyDown -= value;
    }

    /// <summary>
    /// Return true to prevent the event from being passed to the next handler
    /// </summary>
    public event Action<KeyboardEvent> KeyUp
    {
        add => window.KeyUp += value;
        remove => window.KeyUp -= value;
    }

    public event Action<MouseButtonEvent> MouseButtonDown
    {
        add => window.MouseButtonDown += value;
        remove => window.MouseButtonDown -= value;
    }

    public event Action<MouseButtonEvent> MouseButtonUp
    {
        add => window.MouseButtonUp += value;
        remove => window.MouseButtonUp -= value;
    }

    public event Action<MouseMotionEvent> MouseMove
    {
        add => window.MouseMove += value;
        remove => window.MouseMove -= value;
    }

    public event Action<MouseWheelEvent> MouseScroll
    {
        add => window.MouseScroll += value;
        remove => window.MouseScroll -= value;
    }

    public event Action MouseEnteredWindow
    {
        add => window.MouseEnteredWindow += value;
        remove => window.MouseEnteredWindow -= value;
    }

    public event Action MouseLeftWindow
    {
        add => window.MouseLeftWindow += value;
        remove => window.MouseLeftWindow -= value;
    }
}
