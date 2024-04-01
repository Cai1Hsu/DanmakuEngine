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
    protected long lastUpdateTime = 0;
    protected long lastRenderTime = 0;

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
    }

    public void RunUntilExit()
    {
        try
        {
            // start timer here to prevent jit compiling time from being counted
            EngineTimer = Stopwatch.StartNew();

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

    public event Action<KeyboardEvent> KeyEvent
    {
        add => window.KeyEvent += value;
        remove => window.KeyEvent -= value;
    }

    /// <summary>
    /// Return true to prevent the event from being passed to the next handler
    /// </summary>
    public event Func<KeyboardEvent, bool> Keydown
    {
        add => window.KeyDown += value;
        remove => window.KeyDown -= value;
    }

    /// <summary>
    /// Return true to prevent the event from being passed to the next handler
    /// </summary>
    public event Func<KeyboardEvent, bool> KeyUp
    {
        add => window.KeyUp += value;
        remove => window.KeyUp -= value;
    }

    public event Action<MouseButtonEvent> Mousebuttondown
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
}
