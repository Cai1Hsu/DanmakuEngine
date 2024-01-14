using System.Diagnostics;
using DanmakuEngine.Engine.Threading;
using DanmakuEngine.Logging;
using DanmakuEngine.Threading;
using Silk.NET.Input;
using Silk.NET.SDL;

namespace DanmakuEngine.Engine;

public unsafe partial class GameHost
{
    public WindowManager windowManager = null!;

    // Since we have just implemented multi-threading, and most of the time, this variable is accessed in main loop at a rather high frequency
    // it's possible that the compiler will optimize it to a register, and the CPU may NOT update the value in the register, so the game won't exit even if we request it to exit and the value of isRunning is already true
    // To avoid this, we must add the volatile keyword to the variable.
    // However, I don't think it's necessary to use a lock to protect the variable, because we only read it in the main loop, and the only write operation is to make it true. The main loop will detect the change sooner or later as long as the CPU updates the value in the register.
    protected volatile bool isRunning = true;

    protected long lastUpdateTime = 0;
    protected long lastRenderTime = 0;

    protected readonly ThreadRunner threadRunner = new();

    protected void registerThread(GameThread thread)
        => threadRunner.Add(thread);

    public virtual void RegisterThreads()
    {
        threadRunner.MultiThreaded.BindTo(MultiThreaded);

        registerThread(MainThread = new(HandleMessages));

        registerThread(UpdateThread = new(Update));

        registerThread(RenderThread = new(Render));
    }

    public void RunUntilExit()
    {
        ResetTime(ConfigManager.RefreshRate);

        // We do it here 
        // because the load process is also a part of the Update Loop
        // The only difference is that it only executes once at the beginning
        DoLoad();

        threadRunner.Start();
        this.Start();

        RunMainLoop();

        PerformExit();
    }

    public virtual void RunMainLoop()
    {
        do
        {
            threadRunner.RunMainLoop();
        } while (isRunning && (!screens.Empty()));
    }

    public void RequestClose()
    {
        Logger.Debug("Requesting close");

        isRunning = false;
    }

    private void HandleMessages(double _)
    {
        Event e = new();
        while (_sdl.PollEvent(ref e) != 0)
        {
            switch ((EventType)e.Type)
            {
                case EventType.Firstevent:
                    // This is not reliable, so we ignore it
                    break;

                case EventType.AppTerminating:
                case EventType.Quit:
                    isRunning = false;
                    break;

                // we should only handle the event once
                // and KeyDown(KeyUp) should has higher priority than KeyEvent as it is Engine level
                case EventType.Keydown:
                    if (KeyDown?.Invoke(e.Key) is not true)
                        KeyEvent?.Invoke(e.Key);
                    break;

                case EventType.Keyup:
                    if (KeyUp?.Invoke(e.Key) is not true)
                        KeyEvent?.Invoke(e.Key);
                    break;

                case EventType.Mousebuttondown:
                    MouseButtonDown?.Invoke(e.Button);
                    break;

                case EventType.Mousebuttonup:
                    MouseButtonUp?.Invoke(e.Button);
                    break;

                case EventType.Mousemotion:
                    MouseMove?.Invoke(e.Motion);
                    break;

                case EventType.Mousewheel:
                    MouseScroll?.Invoke(e.Wheel);
                    break;

                case EventType.Windowevent:
                    windowManager?.HandleWindowEvent(e.Window);
                    break;

                case EventType.AppWillenterbackground:
                case EventType.AppWillenterforeground:
                case EventType.AppDidenterforeground:
                case EventType.AppDidenterbackground:
                    windowManager?.HandleAppEvent(e.Type);
                    break;
            }
        }
    }

    public event Action<KeyboardEvent> KeyEvent = null!;

    public event Func<KeyboardEvent, bool> KeyDown = null!;

    public event Func<KeyboardEvent, bool> KeyUp = null!;

    public event Action<MouseButtonEvent> MouseButtonDown = null!;

    public event Action<MouseButtonEvent> MouseButtonUp = null!;

    public event Action<MouseMotionEvent> MouseMove = null!;

    public event Action<MouseWheelEvent> MouseScroll = null!;
}
