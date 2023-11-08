using System.Diagnostics;
using DanmakuEngine.Configuration;
using DanmakuEngine.Dependency;
using DanmakuEngine.Graphics;
using DanmakuEngine.Logging;
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

    private void DoLoad()
    {
        Logger.Debug("Loading game in progress...");

        if (ConfigManager.HasConsole)
        {
            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                RequestClose();
            };
        }

        Root = new DrawableContainer(null!);

        screens = new(Root);

        Root.Add(screens);

        Dependencies.Cache(screens);
        DependencyContainer.AutoInject(Game);

        Root.load();

        RegisterEvents();

        Game.Begin();
    }

    private DrawableContainer Root = null!;
    private void DoUpdate()
    {
        if (Root == null)
            return;

        if (screens.Empty())
            isRunning = false;

        // if (window.WindowState != WindowState.Minimized)
        //     Root.Size = new Vector2D<float>(window.Size.X, window.Size.Y);

        Root.UpdateSubTree();
        // Root.UpdateSubTreeMasking(Root, Root.ScreenSpaceDrawQuad.AABBFloat);

        // using (var buffer = DrawRoots.GetForWrite())
        //     buffer.Object = Root.GenerateDrawNodeSubtree(buffer.Index, false);
    }

    // private DrawableContainer DrawRoot = null!;

    private bool doFrontToBackPass = false;
    private bool clearOnRender = false;

    private void DoRender()
    {
        // if (clearOnRender)
        //     _gl.Clear((uint)ClearBufferMask.ColorBufferBit);

        // if (doFrontToBackPass)
        // {
        //     _gl.Disable(EnableCap.Blend);

        //     _gl.Enable(EnableCap.DepthTest);

        //     // TODO: Front pass
        //     // buffer.Object.DrawOpaqueInteriorSubTree(Renderer, depthValue);

        //     _gl.Enable(EnableCap.Blend);

        //     _gl.DepthMask(false);
        // }
        // else
        // {
        //     _gl.Disable(EnableCap.DepthTest);
        // }

        // // TODO
        // // Do render
    }

    protected void UpdateFps(double delta)
    {
        count_time += delta;
        count_frame++;

        if (count_time < 1)
            return;

        if (ConfigManager.HasConsole)
            Logger.Write($"FPS: {ActualFPS:F2}\r", true);

        ActualFPS = count_frame / count_time;

        count_frame = 0;
        count_time = 0;
    }

    public void PerformExit()
    {
        sw.Stop();

        _sdl.DestroyRenderer(renderer);
        _sdl.DestroyWindow(window);

        if (ConfigManager.HasConsole)
            Console.CursorVisible = true;
    }


    public event Action<KeyboardEvent> KeyDown = null!;

    public event Action<KeyboardEvent> KeyUp = null!;

    public event Action<MouseButtonEvent> MouseButtonDown = null!;

    public event Action<MouseButtonEvent> MouseButtonUp = null!;

    public event Action<MouseMotionEvent> MouseMove = null!;

    public event Action<MouseWheelEvent> MouseScroll = null!;
}