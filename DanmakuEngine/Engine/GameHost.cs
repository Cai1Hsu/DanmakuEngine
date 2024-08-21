using System.Diagnostics;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using DanmakuEngine.Allocations;
using DanmakuEngine.Arguments;
using DanmakuEngine.Bindables;
using DanmakuEngine.Configuration;
using DanmakuEngine.DearImgui;
using DanmakuEngine.Dependency;
using DanmakuEngine.Engine.Platform;
using DanmakuEngine.Engine.Platform.Environments;
using DanmakuEngine.Engine.Platform.Environments.Threading;
using DanmakuEngine.Engine.Platform.Windows;
using DanmakuEngine.Engine.SDLNative;
using DanmakuEngine.Engine.Sleeping;
using DanmakuEngine.Engine.Threading;
using DanmakuEngine.Engine.Windowing;
using DanmakuEngine.Extensions;
using DanmakuEngine.Games;
using DanmakuEngine.Games.Screens;
using DanmakuEngine.Graphics;
using DanmakuEngine.Graphics.Renderers;
using DanmakuEngine.Input;
using DanmakuEngine.Logging;
using DanmakuEngine.Scheduling;
using DanmakuEngine.Timing;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.SDL;
using Silk.NET.Vulkan;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.PixelFormats;
using Veldrid;
using Color = SixLabors.ImageSharp.Color;
using PixelFormat = Silk.NET.OpenGL.PixelFormat;
using PixelType = Silk.NET.OpenGL.PixelType;
using Renderer = DanmakuEngine.Graphics.Renderers.Renderer;

namespace DanmakuEngine.Engine;

public partial class GameHost : Time, IDisposable
{
    private static object _instanceLock = new();
    private static GameHost _instance = null!;

    private Sdl _sdl = null!;

    public Sdl2Window window { get; private set; } = null!;

    public Game Game { get; private set; } = null!;

    public ConfigManager ConfigManager { get; private set; } = null!;

    public InputManager InputManager { get; private set; } = null!;

    public DependencyContainer Dependencies { get; private set; } = null!;

    public Scheduler Scheduler => _root.Scheduler;

    public RootObject Root
    {
        get => _root;
        private set => _root = value;
    }

    private RootObject _root = null!;

    protected ScreenStack ScreenStack = null!;

    public MainThread MainThread { get; protected set; } = null!;

    public UpdateThread UpdateThread { get; protected set; } = null!;

    public RenderThread RenderThread { get; protected set; } = null!;

    public readonly Bindable<bool> MultiThreaded = new(true);

    public GraphicsBackend GraphicsBackend { get; set; } = GraphicsBackend.OpenGL;

    public RendererType RendererType { get; set; } = RendererType.Silk;

    public Renderer Renderer { get; private set; } = null!;

    public EngineState State { get; protected set; } = EngineState.Created;

    public GameViewport Viewport { get; private set; } = null!;

    public string DefaultWindowTitle
    {
        get
        {
            var ver = Assembly.GetExecutingAssembly().GetName().Version;
            var name = Game.Name;

            if (ConfigManager.DebugMode)
                name += "(debug mode)";

            if (ConfigManager.DebugBuild)
                return name + $" - dev {ver}";

            var title = name + $" - ver {ver}";

            return title;
        }
    }

    public GameHost()
    {
        ConfigManager = new ConfigManager(this is HeadlessGameHost);
    }

    public void Run(Game game, ArgumentProvider argProvider)
    {
        lock (_instanceLock)
        {
            Initialize();

            SetUpDependency();

            this.Game = game;
            Dependencies.Cache(Game);

            LoadConfig(argProvider);

            SetUpDebugConsole();

            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

            SetUpWindowAndRenderer();

            RegisterThreads();

            BootstrapGame();

            RunUntilExit();
        }
        PerformExit();
    }

    private void Initialize()
    {
        State = EngineState.Initializing;

        _instance = this;

        Time.ElapsedSeconds = 0;
        Time.UpdateDelta = 0;

        EngineTimer = null!;

        Logger.Debug($"====== DanmakuEngine ======");

        Logger.Debug($"{(
            ConfigManager.DebugBuild ? "Debug" : "Release"
        )} build version");

        if (this is HeadlessGameHost)
        {
            // Logger is loaded with debug flag here
            Logger.Debug("Running in headless mode.");
        }
    }

    private void BootstrapGame()
    {
        Logger.Debug("Loading game in progress...");

        Root = new()
        {
            Children =
            {
                // Inputs should be updated first
                (InputManager = new InputManager()),
                (ScreenStack = new(Root)),
            }
        };

        Dependencies.Cache(Root);
        Dependencies.Cache(ScreenStack);

        RegisterEvents();

        Viewport = new GameViewport();

        if (window is not null)
        {
            // Only window exists can we register events for it
            // But handlers still run, as long as we feed events manually
            InputManager.Register(this);

            Viewport.Size = new Vector2D<float>(window.Size.X, window.Size.Y);

            window.WindowSizeChanged += (w, h) =>
            {
                Viewport.UpdateSize(new Vector2D<float>(w, h));
            };
        }

        Dependencies.Cache(Viewport);
        Dependencies.Cache(InputManager);

        Game.prelude();
        ScreenStack.Push(Game.EntryScreen);

        Logger.Debug("Everything is ready, let's go!");
    }

    private long lastFrameFixedUpdateCount = 0;
    private double lastFrameTime = 0;
    private void PreUpdate()
    {
        bool didFixedUpdate = lastFrameFixedUpdateCount != FixedUpdateCount;

        double currentTime = EngineTimer.ElapsedSeconds;

        QueuedFixedUpdateCount = (int)((currentTime - LastFixedUpdateTimeWithErrors) / FixedUpdateDeltaNonScaled);
        if (DoFixedUpdate)
        {
            // Sometimes the Update ended after the next FixedUpdate should start
            // When we execute the next FixedUpdate, the ElapsedSeconds will be pull back to the last FixedUpdate
            // We want to fake a time for next frame to keep the time consistent
            var deltaNonScaled = (FixedUpdateDeltaNonScaled * (FixedUpdateCount + QueuedFixedUpdateCount)) - ElapsedSecondsNonScaled;
            var delta = (FixedUpdateDelta * (FixedUpdateCount + QueuedFixedUpdateCount)) - ElapsedSeconds;

            Debug.Assert(deltaNonScaled >= 0);
            Debug.Assert(delta >= 0);

            UpdateDeltaNonScaled = deltaNonScaled;
            UpdateDeltaNonScaledF = (float)deltaNonScaled;

            UpdateDelta = delta;
            UpdateDeltaF = (float)delta;
        }
        else
        {
            var timeSinceLastFixedUpdate = currentTime - RealLastFixedUpdateTime;

            var deltaNonScaled = didFixedUpdate
                ? timeSinceLastFixedUpdate
                : currentTime - lastFrameTime;

            Debug.Assert(timeSinceLastFixedUpdate >= 0);
            Debug.Assert(deltaNonScaled >= 0);

            var delta = deltaNonScaled;

            UpdateDeltaNonScaled = deltaNonScaled;
            UpdateDeltaNonScaledF = (float)deltaNonScaled;

            UpdateDelta = delta;
            UpdateDeltaF = (float)delta;

            ElapsedSecondsNonScaled += deltaNonScaled;
            ElapsedSeconds += delta;
        }

        lastFrameTime = currentTime;
        lastFrameFixedUpdateCount = FixedUpdateCount;
    }

    protected void Update()
    {
        PreUpdate();

        if (_root == null)
            return;

        int fixedUpdateCount = 0;

        while (DoFixedUpdate)
        {
            // never allow FixedUpdate blocks the game logic too heavily
            if (fixedUpdateCount > 5)
            {
                // if we are too far behind, just skip the update
                // And add the floored skipped frames to the count
                // This ensures the Time.ElapsedSeconds is always correct
                var skipped = QueuedFixedUpdateCount;
                FixedUpdateCount += skipped;
                QueuedFixedUpdateCount = 0;

                ElapsedSecondsNonScaled = FixedElapsedSecondsNonScaled;
                ElapsedSeconds = FixedElapsedSeconds;

#if DEBUG
                Logger.Warn($"Skipped {skipped} FixedUpdate frames");
#endif
                break;
            }

            _root.FixedUpdateSubtree();

            double engineElapsed = EngineTimer.ElapsedSeconds;

            // Must do this before the FixedElapsedSecondsNonScaled is updated
            RealFixedUpdateDelta = engineElapsed - RealLastFixedUpdateTime;
            ExcessFixedFrameTime = FixedUpdateDeltaNonScaled - RealFixedUpdateDelta;
            RealLastFixedUpdateTime = engineElapsed;

            FixedUpdateCount++;
            fixedUpdateCount++;
            QueuedFixedUpdateCount--;

            ElapsedSecondsNonScaled = FixedElapsedSecondsNonScaled;
            ElapsedSeconds = FixedElapsedSeconds;

            UpdateThread.OnFixedUpdate();
        }

        Debug.Assert(QueuedFixedUpdateCount == 0);

        if (fixedUpdateCount > 1)
            Logger.Warn($"FixedUpdate took {fixedUpdateCount} frames to catch up");

        Debug.Assert(ElapsedSecondsNonScaled >= FixedElapsedSecondsNonScaled);
        Debug.Assert(ElapsedSecondsNonScaled <= FixedElapsedSecondsNonScaled + FixedUpdateDeltaNonScaled);

        _root.UpdateSubTree();

        Imgui.Update();
    }

    // private DrawableContainer DrawRoot = null!;
    private bool doFrontToBackPass = false;
    private bool clearOnRender = false;

    protected void Render()
    {
        Renderer.BeginFrame();
        {
            Renderer.ClearScreen();

            Imgui.Render();
        }
        Renderer.EndFrame();

        Renderer.SwapBuffers();
    }

    protected virtual void SetUpDebugConsole()
    {
        if (ConfigManager.RunningTest)
            return;

        var enabled = ConfigManager.DebugMode;

        if (enabled && RuntimeInfo.IsWindows && !ConfigManager.HasConsole)
#pragma warning disable CA1416
            WindowsGameHost.CreateConsole();
#pragma warning restore CA1416

        var status = enabled ? "enabled" : "disabled";

        if (enabled)
            Logger.SetPrintLevel(LogLevel.Info);

        Logger.Debug($"Debug mode {status}");

        if (ConfigManager.DebugMode
           || ConfigManager.DebugBuild
           || ConfigManager.Headless)
        {
            if (ConfigManager.HasConsole)
            {
                Console.CancelKeyPress += (_, e) =>
                {
                    if (lastCancelPress is not null &&
                        AppTimer.ElapsedSeconds - lastCancelPress < 1)
                    {
                        Logger.Warn("Exiting...");
                    }
                    else
                    {
                        RequestClose();

                        e.Cancel = true;
                        lastCancelPress = AppTimer.ElapsedSeconds;
                        Logger.Warn("Press Ctrl+C again to force exit.");
                    }

                    Console.ResetColor();
                    Console.CursorVisible = true;
                };

                Console.CursorVisible = false;
            }
        }
    }

    private static double? lastCancelPress = null;

    private void SetUpDependency()
    {
        Dependencies = DependencyContainer.Reset();

        Debug.Assert(DependencyContainer.Instance != null);
        Debug.Assert(Dependencies != null);

        Dependencies.Cache(this);

        Dependencies.Cache((Time)this);

        Dependencies.Cache(IWaitHandler.WaitHandler);
    }

    private void LoadConfig(ArgumentProvider argProvider)
    {
        Dependencies.Cache(ConfigManager);

        using (argProvider)
        {
            ArgumentNullException.ThrowIfNull(argProvider);
            Dependencies.Cache(argProvider);

            ConfigManager.LoadFromArguments(argProvider);
        }

        DependencyContainer.AutoInject(Logger.GetLogger());

        doFrontToBackPass = ConfigManager.DebugMode;
        clearOnRender = ConfigManager.ClearScreen;
        DebugFpsHz = ConfigManager.FpsUpdateFrequency;

        MultiThreaded.Value = !ConfigManager.Singlethreaded;

        var envThreadingMode = Env.Get<ThreadingModeEnv, ThreadingMode>();

        if (envThreadingMode is not null)
        {
            MultiThreaded.Value = envThreadingMode is ThreadingMode.MultiThreaded;

            Logger.
#if DEBUG
            Warn
#else
            Debug
#endif
            ($"Overrided threading mode to {envThreadingMode} by environment");
        }
    }

    private void SetupSdl()
    {
        _sdl = SDL.Api;

        Silk.NET.SDL.Version ver = new();
        _sdl.GetVersion(ref ver);

        Logger.Debug($"SDL version: {ver.Major}.{ver.Minor}.{ver.Patch}");

        IWaitHandler.PreferSDL = false;
    }

    public void SetUpRenderer()
    {
        switch (RendererType)
        {
            case RendererType.Silk:
            {
                switch (GraphicsBackend)
                {
                    case GraphicsBackend.OpenGL:
                    case GraphicsBackend.OpenGLES:
                        break;

                    default:
                    {
                        RendererType = RendererType.Veldrid;

                        Logger.Warn("Silk.NET renderer is not supported for non-OpenGL backend, falling back to Veldrid renderer");
                    }
                    break;
                }
            }
            break;
        }

        Renderer = RendererType switch
        {
            RendererType.Silk => GLSilkRenderer.Create(window),
            RendererType.Veldrid => VeldridRenderer.Create(window, GraphicsBackend),

            // What the fuck? This should never happen
            _ => throw new PlatformNotSupportedException($"Renderer type {RendererType} is not supported")
        };

        Renderer.VSync = ConfigManager.Vsync;

        window.WindowSizeChanged += (h, w) =>
        {
            Renderer.Viewport(0, 0, h, w);
        };

        // Clear the screen
        Renderer.BeginFrame();
        Renderer.MakeCurrent();
        var window_size = window.Size;
        Renderer.Viewport(0, 0, window_size.X, window_size.Y);

        Renderer.SetClearColor(0.1f, 0.1f, 0.1f, 1);
        Renderer.ClearScreen();

        Renderer.EndFrame();
        Renderer.SwapBuffers();
    }

    public virtual void RegisterEvents()
    {
        if (window is not null)
        {
            window.WindowSizeChanged += Coordinate.OnResized;
        }
    }

    public void PerformExit()
    {
        try
        {
            threadRunner?.Stop();

            threadRunner?.WaitUntilAllThreadsExited();
        }
        finally
        {
            window?.Dispose();

            Imgui.Shutdown();

            Renderer?.Dispose();

            if (ConfigManager.HasConsole && !ConfigManager.RunningTest)
            {
                Console.CursorVisible = true;

                Console.ResetColor();
            }

            SDL.Quit();

            lock (_instanceLock)
            {
                if (_instance == this)
                    _instance = null!;
            }

            State = EngineState.Exited;
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposing)
            return;

        if (State is not EngineState.Exited)
            PerformExit();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public static bool HasConsole()
        => !RuntimeInfo.IsWindows
#pragma warning disable CA1416
        || WindowsGameHost.HasConsole();
#pragma warning restore CA1416

    private static WindowFlags getWindowFlags(bool fullScreen = false, bool exclusive = true, bool resiable = false, bool alwaysOnTop = false, GraphicsBackend backend = GraphicsBackend.OpenGL)
    {
        var flags = WindowFlags.None;

        if (fullScreen)
        {
            if (exclusive)
                flags |= WindowFlags.Fullscreen;
            else
                flags |= WindowFlags.FullscreenDesktop;
        }

        if (resiable)
            flags |= WindowFlags.Resizable;

        if (alwaysOnTop)
            flags |= WindowFlags.AlwaysOnTop;

        // TODO: Add an argument for this
        // This restricts the mouse to the window
        // flags |= WindowFlags.InputGrabbed;

        flags |= WindowFlags.InputFocus;

        flags |= WindowFlags.MouseCapture;
        flags |= WindowFlags.MouseFocus;

        switch (backend)
        {
            case GraphicsBackend.OpenGL:
                flags |= WindowFlags.Opengl;
                break;

            case GraphicsBackend.Vulkan:
                flags |= WindowFlags.Vulkan;
                break;

            case GraphicsBackend.Direct3D11:
                SDL.Api.SetHint("SDL_HINT_RENDER_DRIVER", "direct3d");
                // https://stackoverflow.com/a/53270255
                // Seems that we don't need any special flags for D3D11
                break;

            default:
                throw new PlatformNotSupportedException($"Graphics backend {backend} is not supported");
        }

        return flags;
    }

    public virtual void SetUpWindowAndRenderer()
    {
        SetupSdl();

        // TODO: load form config manager
        var size = new Vector2D<int>(1920, 1080);

        var flag = getWindowFlags(fullScreen: ConfigManager.FullScreen,
                                  exclusive: ConfigManager.Exclusive,
                                  resiable: false,
                                  alwaysOnTop: false,
                                  backend: GraphicsBackend);

        window = new Sdl2Window(DefaultWindowTitle,
                                (int)Sdl.WindowposUndefinedMask, (int)Sdl.WindowposUndefinedMask,
                                size.X, size.Y,
                                (uint)flag);

        Dependencies.Cache(window);

        Logger.Debug($"Created game window: {size.X}x{size.Y}");

        if (setDisplayMode(size, out var applied))
        {
            size = new Vector2D<int>(applied.W, applied.H);
        }

        // set the default value
        Coordinate.OnResized(size.X, size.Y);

        SetUpRenderer();

        Imgui.Initialize(window, Renderer);
    }
}

// This is splited to do some unsafe job
public unsafe partial class GameHost
{
    protected virtual IList<DisplayMode> GetDisplayModes(int display_index)
    {
        ArgumentOutOfRangeException
            .ThrowIfNegative(display_index, nameof(display_index));

        var display_count = _sdl.GetNumVideoDisplays();

        ArgumentOutOfRangeException
            .ThrowIfGreaterThanOrEqual(display_index, display_count, nameof(display_index));

        int nums = SDL.Api.GetNumDisplayModes(display_index);
        IList<DisplayMode> modes = [];

        for (int i = 0; i < nums; i++)
        {
            DisplayMode mode = new();

            if (SDL.Api.GetDisplayMode(display_index, i, ref mode) >= 0)
            {
                modes.Add(mode);

#if DEBUG
                Logger.Debug($"Display mode({i + 1}/{nums}): {mode.W}x{mode.H}@{mode.RefreshRate}Hz");
#endif
            }
#if DEBUG
            else
                Logger.Warn($"Failed to fetch display mode({i + 1}/{nums}): {SDL.Api.GetErrorS()}");
#endif
        }

        return modes;
    }

    private IList<DisplayMode> getDisplayModes()
    {
        var displays = _sdl.GetNumVideoDisplays();

        if (displays == 0)
        {
            Logger.Warn($"No avaliable monitor found.");
            Logger.Warn($"SDL Error:{_sdl.GetErrorS()}");

            return [];
        }

        if (displays > 1)
        {
            var main_name = _sdl.GetDisplayNameS(0);
            Logger.Warn($"Multi monitors are not supported, using main monitor({main_name}).");
        }

        return GetDisplayModes(0);
    }

    private bool setDisplayMode(Vector2D<int> window_size, out DisplayMode applied_mode)
    {
        // The reason why we have to pass the size instead of use window.Size is that
        // On wayland, when we make the window fullscreen, the window size will be changed
        // to the display size, which lead to our expected display mode to native display mode
        applied_mode = default;

        var modes = getDisplayModes();

        if (modes.Count == 0)
            return false;

        // Hope that no one get a display with refresh rate higher than 1000Hz
        int refresh_rate = 1000;

        if (Environment.CommandLine.Contains(" -refresh "))
            refresh_rate = ConfigManager.RefreshRate;

        Debug.Assert(refresh_rate > 0);

        DisplayMode expected = new(w: window_size.X,
                                   h: window_size.Y,
                                   refreshRate: refresh_rate);

        var matcheds = modes.Where(m => m.W == expected.W
                                    && m.H == expected.H)
                            .OrderBy(m => Math.Abs(m.RefreshRate - expected.RefreshRate));

        DisplayMode closest = default;

        if (matcheds.Any())
        {
            closest = matcheds.First();
        }
        else
        {
            Logger.Warn($"No avaliable display mode found for target size {expected.W}x{expected.H}. Using closest mode.");

            // TODO: currently only support primary display
            _sdl.GetClosestDisplayMode(0, in expected, &closest);
        }

        _sdl.SetWindowDisplayMode(window.Window, &closest);

        applied_mode = window.DisplayMode;

#if DEBUG
        Logger.Debug($"Selected display mode: {closest.W}x{closest.H}@{closest.RefreshRate}Hz, Applied: {applied_mode.W}x{applied_mode.H}@{applied_mode.RefreshRate}Hz");
#endif

        return true;
    }
}
