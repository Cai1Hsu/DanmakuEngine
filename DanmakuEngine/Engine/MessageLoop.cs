using System.Diagnostics;
using DanmakuEngine.Logging;
using Silk.NET.SDL;

namespace DanmakuEngine.Engine;

public unsafe partial class GameHost
{
    public WindowManager windowManager = null!;

    protected bool isRunning = true;

    protected readonly Stopwatch HostTimer = new();

    protected long lastUpdateTicks = 0;
    protected long lastRenderTicks = 0;

    public void RunUntilExit()
    {
        ResetTime(ConfigManager.RefreshRate);

        HostTimer.Reset();
        HostTimer.Start();

        // We do it here 
        // because the load process is also a part of the Update Loop
        // The only difference is that it only executes once at the beginning
        DoLoad();

        RunMainLoop();

        HostTimer.Stop();
        PerformExit();
    }

    public virtual void RunMainLoop()
    {
        do
        {
            HandleMessages();

            long currentTicks = HostTimer.ElapsedTicks;

            UpdateDelta = (currentTicks - lastUpdateTicks) / (double)Stopwatch.Frequency;
            RenderDelta = (currentTicks - lastRenderTicks) / (double)Stopwatch.Frequency;

            UpdateTime(RenderDelta);

            DoUpdate();

            lastUpdateTicks = currentTicks;

            DoRender();

            lastRenderTicks = currentTicks;
        } while (isRunning && !screens.Empty());
    }

    public void RequestClose()
    {
        Logger.Debug("Requesting close");

        isRunning = false;
    }

    private void HandleMessages()
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

                case EventType.Keydown:
                    KeyDown?.Invoke(e.Key);
                    break;

                case EventType.Keyup:
                    KeyUp?.Invoke(e.Key);
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

    public event Action<KeyboardEvent> KeyDown = null!;

    public event Action<KeyboardEvent> KeyUp = null!;

    public event Action<MouseButtonEvent> MouseButtonDown = null!;

    public event Action<MouseButtonEvent> MouseButtonUp = null!;

    public event Action<MouseMotionEvent> MouseMove = null!;

    public event Action<MouseWheelEvent> MouseScroll = null!;
}
