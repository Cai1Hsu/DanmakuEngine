using System.Diagnostics;
using Silk.NET.SDL;


namespace DanmakuEngine.Engine;

public unsafe partial class GameHost
{
    public WindowManager windowManager = null!;
    public void RequestClose()
        => isRunning = false;

    private bool isRunning = true;

    private readonly Stopwatch sw = new();

    private long lastUpdateTicks = 0;
    private long lastRenderTicks = 0;

    public void RunUntilExit()
    {
        DoLoad();

        sw.Reset();
        sw.Start();

        while (isRunning)
        {
            long currentTicks = sw.ElapsedTicks;

            UpdateDelta = (currentTicks - lastUpdateTicks) / (double)Stopwatch.Frequency;
            RenderDelta = (currentTicks - lastRenderTicks) / (double)Stopwatch.Frequency;

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
                        windowManager?.HandleWindowEvent((WindowEventID)e.Window.Event);
                        break;

                    case EventType.AppWillenterbackground:
                    case EventType.AppWillenterforeground:
                    case EventType.AppDidenterforeground:
                    case EventType.AppDidenterbackground:
                        windowManager?.HandleAppEvent(e.Type);
                        break;

                }
            }

            DoUpdate();

#if DEBUG
            UpdateFps(RenderDelta);
#endif

            lastUpdateTicks = currentTicks;

            DoRender();

            lastRenderTicks = currentTicks;
        }

        sw.Stop();
        PerformExit();
    }

    public event Action<KeyboardEvent> KeyDown = null!;

    public event Action<KeyboardEvent> KeyUp = null!;

    public event Action<MouseButtonEvent> MouseButtonDown = null!;

    public event Action<MouseButtonEvent> MouseButtonUp = null!;

    public event Action<MouseMotionEvent> MouseMove = null!;

    public event Action<MouseWheelEvent> MouseScroll = null!;
}