using System.Diagnostics;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using DanmakuEngine.Allocations;
using DanmakuEngine.Arguments;
using DanmakuEngine.Bindables;
using DanmakuEngine.Configuration;
using DanmakuEngine.Dependency;
using DanmakuEngine.Engine.Platform;
using DanmakuEngine.Engine.Platform.Windows;
using DanmakuEngine.Engine.Sleeping;
using DanmakuEngine.Engine.Threading;
using DanmakuEngine.Engine.Windowing;
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
                (ScreenStack = new(Root))
            }
        };

        Dependencies.Cache(Root);
        Dependencies.Cache(ScreenStack);

        RegisterEvents();

        if (window is not null)
        {
            InputManager = new InputManager();

            Dependencies.Cache(InputManager);

            InputManager.RegisterHandlers(this);
        }

        Game.prelude();
        ScreenStack.Push(Game.EntryScreen);

        Logger.Debug("Everything is ready, let's go!");
    }

    protected void Update()
    {
        if (_root == null)
            return;

        // if (window.WindowState != WindowState.Minimized)
        //     Root.Size = new Vector2D<float>(window.Size.X, window.Size.Y);
        _root.UpdateSubTree();
        // Root.UpdateSubTreeMasking(Root, Root.ScreenSpaceDrawQuad.AABBFloat);

        // using (var buffer = DrawRoots.GetForWrite())
        //     buffer.Object = Root.GenerateDrawNodeSubtree(buffer.Index, false);
    }

    // private DrawableContainer DrawRoot = null!;
    private bool doFrontToBackPass = false;
    private bool clearOnRender = false;

    protected void Render()
    {
        // Use Renderer.Clear
        // gl.Clear((uint)ClearBufferMask.ColorBufferBit | (uint)ClearBufferMask.DepthBufferBit);

        if (doFrontToBackPass)
        {
            // TODO: Front pass
            // buffer.Object.DrawOpaqueInteriorSubTree(Renderer, depthValue);
        }

        // TODO
        // Do render

        // private Image<Rgba32> fpsText = new Image<Rgba32>(128, 128);
        // private Font font = SystemFonts.CreateFont("Consolas", 22);

        // gl.BindVertexArray(_vao);
        // gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), (void*)0);
        // gl.EnableVertexAttribArray(0);
        // gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), (void*)(3 * sizeof(float)));
        // gl.EnableVertexAttribArray(1);

        // gl.UseProgram(shaderProgram);

        // int textureLocation = gl.GetUniformLocation(shaderProgram, "texture1");

        // gl.Uniform1(textureLocation, 0);

        // uint textureId = gl.GenTexture();

        // gl.ActiveTexture(TextureUnit.Texture0);
        // gl.BindTexture(GLEnum.Texture2D, textureId);

        // fpsText.ProcessPixelRows(accessor =>
        // {
        //     for (int y = 0; y < fpsText.Height; y++)
        //     {
        //         fixed (void* data = accessor.GetRowSpan(y))
        //         {
        //             gl.TexSubImage2D(GLEnum.Texture2D, 0, 0, y, (uint)accessor.Width, 1, PixelFormat.Rgba, PixelType.UnsignedByte, data);

        //             // gl.TexImage2D(GLEnum.Texture2D, 0, (int)InternalFormat.Rgba, (uint)accessor.Width, 1, PixelFormat.Rgba, PixelType.UnsignedByte, data);
        //         }
        //     }
        // });

        // gl.Enable(EnableCap.Texture2D);

        // gl.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, (void*)0);

        // Renderer.SwapBuffers();
        // _sdl.GLSwapWindow(window);
    }

    private int last_debug_fps = -1;
    protected void DebugTime()
    {
        int this_debug = (int)(EngineTimer.ElapsedMilliseconds / (1000 / DebugFpsHz));

        if (this_debug != last_debug_fps)
        {
            if (ConfigManager.HasConsole
             && ConfigManager.DebugMode)
            {
                Logger.Write($"FPS: {UpdateThread.AverageFramerate:F2}({1000 / UpdateThread.AverageFramerate:F2}±{UpdateThread.Jitter:F2}ms)          \r", true, false);
                last_debug_fps = this_debug;
            }
        }
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
                    RequestClose();

                    Console.ResetColor();
                    Console.CursorVisible = true;
                };

                Console.CursorVisible = false;
            }
        }
    }

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

        doFrontToBackPass = ConfigManager.DebugMode;
        clearOnRender = ConfigManager.ClearScreen;
        DebugFpsHz = ConfigManager.FpsUpdateFrequency;

        MultiThreaded.Value = !ConfigManager.Singlethreaded;

        DependencyContainer.AutoInject(Logger.GetLogger());
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

        // Clear the screen

        // FIXME: not work for silk renderer

        Renderer.BeginFrame();
        Renderer.MakeCurrent();

        Renderer.SetClearColor(0, 0, 0, 1);
        Renderer.ClearScreen();

        Renderer.EndFrame();
        Renderer.SwapBuffers();
    }

    public virtual void RegisterEvents()
    {
        if (window is not null)
        {
            window.WindowSizeChanged += Coordinate.OnResized;

            // window.WindowMinimized
            // window.AppDidenterbackground
            // window.AppWillenterforeground
        }

#if DEBUG
        Root.OnUpdate += _ => DebugTime();
#endif
    }

    public void PerformExit()
    {
        try
        {
            // FIXME what the fuck?
            threadRunner?.Stop();
        }
        finally
        {
            window?.Dispose();

            // TODO: Dispose renderer

            if (ConfigManager.HasConsole && !ConfigManager.RunningTest)
            {
                Console.CursorVisible = true;

                Console.ResetColor();
            }

            _sdl?.Quit();

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
        var size = new Vector2D<int>(640, 480);

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

        SetDisplayMode();

        // set the default value
        Coordinate.OnResized(size.X, size.Y);

        SetUpRenderer();
    }
}

// This is splited to do some unsafe job
public unsafe partial class GameHost
{
    private void SetDisplayMode()
    {
        var window_size = window.Size;

        var displays = _sdl.GetNumVideoDisplays();

        if (displays == 0)
        {
            Logger.Warn($"No avaliable monitor found.");
            Logger.Warn($"SDL Error:{_sdl.GetErrorS()}");

            return;
        }

        if (displays > 1)
        {
            var main_name = _sdl.GetDisplayNameS(0);
            Logger.Warn($"Multi monitors are not supported, using main monitor({main_name}).");
        }

        // check if main monitor is already for use.
        var main_monitor = new DisplayMode();
        if (_sdl.GetDesktopDisplayMode(0, ref main_monitor) >= 0)
        {
            if (main_monitor.W == window_size.X
             && main_monitor.H == window_size.Y)
            {
                _sdl.SetWindowDisplayMode(window.Window, main_monitor);

                var current = new DisplayMode();
                _sdl.GetWindowDisplayMode(window.Window, ref current);

                Logger.Debug($"Main monitor size matched, using current display mode({current.W}x{current.H}@{current.RefreshRate}Hz).");

                return;
            }
        }

        List<DisplayMode> modes = fetchModes();

        var refresh_rate = -1;

        if (displays < 0)
        {
            refresh_rate = Math.Max(refresh_rate, 60);

            Logger.Warn($"Failed to retrieve main monitor refresh rate. Using {refresh_rate}Hz as fallback");
        }
        else
        {
            int max_rate = modes.Max(m => m.RefreshRate);

            // TODO
            // determine whether the refresh_rate in config manageer is passed with arguments.
            // This is not a good way to check it, but it is the only way before we refactoring argument handling.
            if (Environment.CommandLine.Contains("-refresh"))
            {
                refresh_rate = Math.Min(ConfigManager.RefreshRate, max_rate);
            }
            else
            {
                refresh_rate = max_rate;
            }
        }

        Debug.Assert(refresh_rate > 0);

        DisplayMode expected = new(w: window_size.X,
                                   h: window_size.Y,
                                   refreshRate: refresh_rate);

        DisplayMode closest = new DisplayMode();

        _sdl.GetClosestDisplayMode(0, in expected, ref closest);

        _sdl.SetWindowDisplayMode(window.Window, closest);

        Logger.Debug($"Display mode: {closest.W}x{closest.H}@{closest.RefreshRate}Hz, fullscreen: {ConfigManager.FullScreen}");

        static List<DisplayMode> fetchModes()
        {
            StackValue<DisplayMode> first_mode = stackalloc DisplayMode[1];
            SDL.Api.GetDisplayMode(0, 0, first_mode);

            // The max refresh rate of the native display mode.
            int max_rate = first_mode.Value.RefreshRate;

            int nums = SDL.Api.GetNumDisplayModes(0);
            List<DisplayMode> modes = Enumerable.Repeat<DisplayMode>(new(), nums).ToList();

            for (int i = 0; i < nums; i++)
            {
                DisplayMode mode = new();

                // Refreshrate is handled incorrectly in SDL.
                // When calculating the refresh rate, it is not divided by 2 if the mode is interlaced and not multiplied by 2 if the mode is double.
                // This was fixed in SDL3 by me, but was not backported to SDL2.
                // So we are going to do some hacky workaround here.
                if (SDL.Api.GetDisplayMode(0, i, ref mode) >= 0)
                {
                    // Ignore meaningless values due to overflow
                    if (mode.W > first_mode.Value.W || mode.H > first_mode.Value.H)
                        continue;

                    // TODO: Fetch display modes directly from XRandR
                    // Hacky workaround to fix refresh rate calculation
                    if (RuntimeInfo.IsLinux // only X11 is affected
                        // This prevents our workaround from being applied to displays with a 60hz native resolution but has higher refresh rate in lower resolutions.
                        && max_rate > 119)
                    {
                        // see https://github.com/libsdl-org/SDL/pull/8933/files for more details
                        if (mode.RefreshRate > max_rate
                            || (mode.RefreshRate > 100 && mode.RefreshRate < 118))
                        {
                            mode.RefreshRate /= 2;
                        }
                    }

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
    }
}
