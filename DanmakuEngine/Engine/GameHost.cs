using System.Reflection;
using System.Runtime;
using DanmakuEngine.Arguments;
using DanmakuEngine.Configuration;
using DanmakuEngine.Dependency;
using DanmakuEngine.Games;
using DanmakuEngine.Games.Screens;
using DanmakuEngine.Graphics;
using DanmakuEngine.Input;
using DanmakuEngine.Logging;
using DanmakuEngine.Timing;
using Silk.NET.Maths;
using Silk.NET.SDL;

namespace DanmakuEngine.Engine;

public partial class GameHost : Time, IDisposable
{
    private static GameHost _instance = null!;

    private Sdl _sdl = null!;

    public Game Game { get; private set; } = null!;

    public ConfigManager ConfigManager { get; private set; } = null!;

    public InputManager InputManager { get; private set; } = null!;

    public DependencyContainer Dependencies { get; private set; } = null!;

    private ScreenStack screens = null!;

    public void Run(Game game)
    {
        SetUpDependency();

        this.Game = game;

        Dependencies.Cache(Game);

        SetUpConsole();

        LoadConfig();

        GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

        SetUpSdl();

        SetUpWindowAndRenderer();

        RunUntilExit();
    }

    protected virtual void SetUpConsole()
    {
        if (ConfigManager.HasConsole)
            Console.CursorVisible = false;
    }

    private void SetUpDependency()
    {
        Dependencies = new DependencyContainer(this);

        Dependencies.Cache((Time)this);
    }
    private void LoadConfig()
    {
        ConfigManager = new ConfigManager();
        Dependencies.CacheAndInject(ConfigManager);

        using var argParser = new ArgumentParser(new ParamTemplate());
        using var argProvider = argParser.CreateArgumentProvider();

        ConfigManager.LoadFromArguments(argProvider);

        doFrontToBackPass = ConfigManager.DebugMode;
        clearOnRender = ConfigManager.ClearScreen;

        DependencyContainer.AutoInject(Logger.GetLogger());
    }
    private void SetUpSdl()
    {
        _sdl = Sdl.GetApi();
    }

    public WindowFlags GenerateWindowFlags(bool FullScreen = false, bool exclusive = true, bool resiable = false, bool allowHighDpi = true, bool alwaysOnTop = false)
    {
        var flags = WindowFlags.Opengl;

        if (FullScreen)
        {
            if (exclusive)
                flags |= WindowFlags.Fullscreen;
            else
                flags |= WindowFlags.FullscreenDesktop;
        }

        if (allowHighDpi)
            flags |= WindowFlags.AllowHighdpi;

        if (resiable)
            flags |= WindowFlags.Resizable;

        if (alwaysOnTop)
            flags |= WindowFlags.AlwaysOnTop;

        flags |= WindowFlags.InputGrabbed;
        flags |= WindowFlags.InputFocus;

        flags |= WindowFlags.MouseCapture;
        flags |= WindowFlags.MouseFocus;

        return flags;
    }

    public RendererFlags GetRendererFlags(bool accelerated = true, bool Vsync = true, bool targettexture = false)
    {
        var flags = accelerated ? RendererFlags.Accelerated : RendererFlags.Software;

        if (Vsync)
            flags |= RendererFlags.Presentvsync;

        if (targettexture)
            flags |= RendererFlags.Targettexture;

        return flags;
    }

    private string GetWindowTitle()
    {
        var ver = Assembly.GetExecutingAssembly().GetName().Version;
        var name = Game.Name;

        if (ConfigManager.DebugBuild)
            return name + $" - Debug {ver}";

        return name + $" - ver {ver}";
    }

    public GameHost()
    {
        if (_instance != null)
            throw new InvalidOperationException("Can NOT create multiple GameHost instance");
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposing)
            return;

        // TODO: dispose managed state (managed objects)
        // Sdl.
    }
}

// This is splited to do some unsafe job
public unsafe partial class GameHost
{
    public Window* window { get; private set; }

    public Surface* windowSurface { get; private set; }

    public Renderer* renderer { get; private set; }

    private void RegisterEvents()
    {
        windowManager = new WindowManager(*window);

        InputManager = new InputManager();

        Dependencies.Cache(InputManager);

        InputManager.RegisterHandlers(this);
    }

    private unsafe void SetUpWindowAndRenderer()
    {
        var size = new Vector2D<int>(640, 480);

        var windowFlag = GenerateWindowFlags(FullScreen: ConfigManager.FullScreen,
                                             exclusive: ConfigManager.Exclusive,
                                             resiable: false,
                                             allowHighDpi: true,
                                             alwaysOnTop: false);

        string title = GetWindowTitle();

        window = _sdl.CreateWindow(title,
            (int)Sdl.WindowposUndefinedMask, (int)Sdl.WindowposUndefinedMask,
            size.X, size.Y,
            (uint)windowFlag);

        Logger.Debug($"Display mode: {ConfigManager.RefreshRate}Hz");

        DisplayMode deisplayMode = new DisplayMode(null, size.X, size.Y, ConfigManager.RefreshRate, null);

        _sdl.SetWindowDisplayMode(window, deisplayMode);

        var rendererFlag = GetRendererFlags(accelerated: true,
                                            Vsync: ConfigManager.Vsync,
                                            targettexture: false);

        renderer = _sdl.CreateRenderer(window, -1, (uint)rendererFlag);
    }


    private void DoLoad()
    {
        Logger.Debug("Loading game in progress...");

        windowSurface = _sdl.GetWindowSurface(window);

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
        if (clearOnRender)
            _sdl.RenderClear(renderer);

        if (doFrontToBackPass)
        {
            // TODO: Front pass
            // buffer.Object.DrawOpaqueInteriorSubTree(Renderer, depthValue);
        }

        // // TODO
        // // Do render

        _sdl.RenderPresent(renderer);
        _sdl.UpdateWindowSurface(window);
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
}