using System.Diagnostics;
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
using Silk.NET.OpenGL;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Drawing.Processing;
using PixelFormat = Silk.NET.OpenGL.PixelFormat;
using PixelType = Silk.NET.OpenGL.PixelType;
using SixLabors.ImageSharp.Advanced;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp.Memory;
using Color = SixLabors.ImageSharp.Color;
using DanmakuEngine.Scheduling;

namespace DanmakuEngine.Engine;

public partial class GameHost : Time, IDisposable
{
    private static GameHost _instance = null!;

    private Sdl _sdl = null!;

    public Game Game { get; private set; } = null!;

    public ConfigManager ConfigManager { get; private set; } = null!;

    public InputManager InputManager { get; private set; } = null!;

    public DependencyContainer Dependencies { get; private set; } = null!;

    public Scheduler Scheduler => Root.Scheduler;

    private readonly DrawableContainer Root = new DrawableContainer(null!);

    protected ScreenStack screens = null!;

    private ArgumentProvider argProvider = null!;

    public void Run(Game game, ArgumentProvider argProvider)
    {
        SetUpDependency();

        this.Game = game;
        Dependencies.Cache(Game);

        ArgumentNullException.ThrowIfNull(argProvider);

        this.argProvider = argProvider;
        Dependencies.Cache(argProvider);

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
        Dependencies = DependencyContainer.Reset();

        Debug.Assert(DependencyContainer.Instance != null);
        Debug.Assert(Dependencies != null);

        Dependencies.Cache(this);

        Dependencies.Cache((Time)this);
    }

    private void LoadConfig()
    {
        ConfigManager = new ConfigManager(this is HeadlessGameHost);
        Dependencies.Cache(ConfigManager);

        ConfigManager.DynamicLoadDefaultValues();
        ConfigManager.LoadFromArguments(argProvider);

        doFrontToBackPass = ConfigManager.DebugMode;
        clearOnRender = ConfigManager.ClearScreen;
        UpdateFrequency = ConfigManager.FpsUpdateFrequency;

        DependencyContainer.AutoInject(Logger.GetLogger());

        // Logger is loaded with debug flag here
        if (this is HeadlessGameHost)
            Logger.Debug("Running in headless mode.");
    }

    public virtual void SetUpSdl()
    {
        _sdl = Sdl.GetApi();

        _sdl.GLSetAttribute(GLattr.RedSize, 8);
        _sdl.GLSetAttribute(GLattr.GreenSize, 8);
        _sdl.GLSetAttribute(GLattr.BlueSize, 8);
        _sdl.GLSetAttribute(GLattr.AccumAlphaSize, 0);
        _sdl.GLSetAttribute(GLattr.DepthSize, 16);
        _sdl.GLSetAttribute(GLattr.StencilSize, 8);
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

        // TODO: Add argument for this
        // This restricts the mouse to the window
        // flags |= WindowFlags.InputGrabbed;

        flags |= WindowFlags.InputFocus;

        flags |= WindowFlags.MouseCapture;
        flags |= WindowFlags.MouseFocus;

        flags |= WindowFlags.Opengl;

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
            return name + $" - dev {ver}";

        var title = name + $" - ver {ver}";

        if (ConfigManager.DebugMode)
            title += "(debug mode)";

        return title;
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
        PerformExit();
    }
}

// This is splited to do some unsafe job
public unsafe partial class GameHost
{
    public Window* window { get; private set; }

    public Surface* windowSurface { get; private set; }

    public Renderer* renderer { get; private set; }

    public void* glContext { get; private set; }

    public SdlGlContext sdlGlContext { get; private set; } = null!;

    public GL gl { get; private set; } = null!;

    public virtual void RegisterEvents()
    {
        windowManager = new WindowManager(*window);

        InputManager = new InputManager();

        Dependencies.Cache(InputManager);

        InputManager.RegisterHandlers(this);

        windowManager.WindowSizeChanged += Coordinate.OnResized;
    }

    public virtual unsafe void SetUpWindowAndRenderer()
    {
        // TODO: load form comfig manager
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

        Logger.Debug($"Display mode: {ConfigManager.RefreshRate}Hz (This only applies to fullscreen mode)");

        DisplayMode deisplayMode = new DisplayMode(null, size.X, size.Y, ConfigManager.RefreshRate, null);

        _sdl.SetWindowDisplayMode(window, deisplayMode);

        var rendererFlag = GetRendererFlags(accelerated: true,
                                            Vsync: ConfigManager.Vsync,
                                            targettexture: false);

        // set the default value
        Coordinate.OnResized(size.X, size.Y);

        // we dont use sdl render api anymore
        renderer = _sdl.CreateRenderer(window, -1, (uint)rendererFlag);

        initlizeOpenGL();

        if (!ConfigManager.Vsync)
            _sdl.GLSetSwapInterval(0);
        else
            _sdl.GLSetSwapInterval(1);
    }

    private void initlizeOpenGL()
    {
        windowSurface = _sdl.GetWindowSurface(window);

        _sdl.GLSetAttribute(GLattr.ContextProfileMask, (int)GLprofile.Core);

        // Minimum OpenGL version for core profile:
        _sdl.GLSetAttribute(GLattr.ContextMajorVersion, 3);
        _sdl.GLSetAttribute(GLattr.ContextMinorVersion, 2);

        glContext = _sdl.GLCreateContext(window);

        if (glContext == null)
        {
            PerformExit();

            throw new Exception("Failed to create GLContext");
        }

        sdlGlContext = new SdlGlContext(_sdl);

        gl = GL.GetApi(sdlGlContext);

        if (gl == null)
            throw new Exception("Failed to get GL api");

        // We are using opengl on this *window*
        _sdl.GLMakeCurrent(window, glContext);
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

        screens = new(Root);

        Root.Add(screens);

        Dependencies.Cache(screens);

        Root.load();

        RegisterEvents();

        Logger.Debug("Everything is ready, let's go!");

        // we should do this in the update loop
        // otherwise first screen will block the update loop
        Scheduler.ScheduleTask(Game.begin);
    }

    protected void DoUpdate()
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

    protected void DoRender()
    {
        gl.Clear((uint)ClearBufferMask.ColorBufferBit | (uint)ClearBufferMask.DepthBufferBit);

        if (clearOnRender)
            _sdl.RenderClear(renderer);

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

        _sdl.GLSwapWindow(window);
    }

    protected void UpdateTime(double delta)
    {
        CurrentTime += delta;

        count_time += delta;
        count_frame++;

        fps_debug_time += delta;

        if (fps_debug_time > UpdateFrequency)
        {
            if (ConfigManager.HasConsole && ConfigManager.DebugMode)
            {
                // Prevent IO blocking
                Task.Run(() =>
                {
                    Logger.Write($"FPS: {ActualFPS:F2}\r", true);
                }).ContinueWith(_ => fps_debug_time = 0, TaskContinuationOptions.ExecuteSynchronously);
            }
        }

        if (count_time < 1)
            return;

        // fpsText.Mutate(x => x.Clear(Color.Transparent));
        // fpsText.Mutate(x => x.DrawText($"{ActualFPS:F2}fps", font, Color.White, new PointF(0, 0)));

        ActualFPS = count_frame / count_time;

        count_frame = 0;
        count_time = 0;
    }

    public void PerformExit()
    {
        HostTimer.Stop();

        // This helps HeadlessGameHost to stop
        if (_sdl != null)
        {
            _sdl.GLDeleteContext(glContext);
            _sdl.DestroyRenderer(renderer);
            _sdl.DestroyWindow(window);
        }

        if (ConfigManager.HasConsole)
        {
            Console.CursorVisible = true;

            Console.ResetColor();
        }
    }
}
