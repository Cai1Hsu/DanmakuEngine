using System.Reflection;
using System.Runtime;
using DanmakuEngine.Arguments;
using DanmakuEngine.Configuration;
using DanmakuEngine.Dependency;
using DanmakuEngine.Graphics;
using DanmakuEngine.Input;
using DanmakuEngine.Logging;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace DanmakuEngine.Games;

public class GameHost : IDisposable
{
    private static GameHost _instance = null!;

    public IWindow window { get; private set; } = null!;

    private GL _gl = null!;

    public Game Game { get; private set; } = null!;

    public ConfigManager ConfigManager { get; private set; } = null!;

    public InputManager InputManager { get; private set; } = null!;

    public DependencyContainer Dependencies { get; private set; } = null!;

    public void Run(Game game)
    {
        SetUpDependency();

        this.Game = game;
        Dependencies.CacheAndInject(Game);

        SetUpConsole();

        LoadConfig();

        GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

        CreateWindow();

        RegisterEvents();

        RunUntilExit();
    }

    protected virtual void SetUpConsole()
    {
        if (ConfigManager.HasConsole)
            Console.CursorVisible = false;
    }

    private void SetUpDependency() => Dependencies = new DependencyContainer(this);

    private void LoadConfig()
    {
        ConfigManager = new ConfigManager();
        Dependencies.CacheAndInject(ConfigManager);

        using var argParser = new ArgumentParser(new ParamTemplate());
        using var argProvider = argParser.CreateArgumentProvider();

        ConfigManager.LoadFromArguments(argProvider);

        doFrontToBackPass = ConfigManager.DebugMode;
        clearOnRender = ConfigManager.ClearScreen;
    }

    private void CreateWindow()
    {
        var options = WindowOptions.Default;

        var size = new Vector2D<int>(640, 480);

        // for windowed mode, we need to set the size to the size of the window
        options.Size = size;

        options.WindowBorder = WindowBorder.Fixed;
        options.WindowState = ConfigManager.FullScreen ? WindowState.Fullscreen : WindowState.Normal;
        options.VideoMode = new VideoMode(size, ConfigManager.RefreshRate);
        options.VSync = ConfigManager.Vsync;

        options.Title = GetWindowName();

        window = Window.Create(options);
    }

    private void RegisterEvents()
    {
        window.Load += OnLoad;
        window.Resize += OnResize;

        window.Update += OnUpdate;
        window.Render += OnRender;
        
        if (ConfigManager.HasConsole)
            window.Update += UpdateFps;

        window.Update += _ => OnRequesetedClose();
    }

    public void RunUntilExit()
    {
        window.Run();

        PerformExit();
    }

    public void PerformExit()
    {
        window.IsClosing = true;
        window.Close();

        window.Dispose();
        
        if (ConfigManager.HasConsole)
            Console.CursorVisible = true;
    }

    private void OnRequesetedClose()
    {
        if (!window.IsClosing)
            return;

        PerformExit();
    }

    private void OnLoad()
    {
        _gl = window.CreateOpenGL();

        //Set-up input context.
        InputManager = new InputManager(window.CreateInput());

        Dependencies.Cache(InputManager);
    }

    private void OnResize(Vector2D<int> size)
    {
        _gl.Viewport(0, 0, (uint)size.X, (uint)size.Y);
    }

    public double ActualFPS { get; private set; }
    public double RenderDelta { get; private set; }
    public double UpdateDelta { get; private set; }

    // private DrawableContainer DrawRoot = null!;

    private bool doFrontToBackPass = false;
    private bool clearOnRender = false;

    private void OnRender(double delta)
    {
        RenderDelta = delta;

        if (clearOnRender)
            _gl.Clear((uint)ClearBufferMask.ColorBufferBit);

        if (doFrontToBackPass)
        {
            _gl.Disable(EnableCap.Blend);

            _gl.Enable(EnableCap.DepthTest);

            // TODO: Front pass
            // buffer.Object.DrawOpaqueInteriorSubTree(Renderer, depthValue);

            _gl.Enable(EnableCap.Blend);

            _gl.DepthMask(false);
        }
        else
        {
            _gl.Disable(EnableCap.DepthTest);
        }

        // TODO
        // Do render
    }

    private DrawableContainer Root = null!;
    private void OnUpdate(double delta)
    {
        UpdateDelta = delta;

        if (window == null)
            return;

        if (Root == null)
            return;

        if (window.WindowState != WindowState.Minimized)
            Root.Size = new Vector2D<float>(window.Size.X, window.Size.Y);

        // Root.UpdateSubTree();
        // Root.UpdateSubTreeMasking(Root, Root.ScreenSpaceDrawQuad.AABBFloat);

        // using (var buffer = DrawRoots.GetForWrite())
        //     buffer.Object = Root.GenerateDrawNodeSubtree(buffer.Index, false);
    }

    private double count_time = 0;
    private int count_frame = 0;
    private void UpdateFps(double delta)
    {
        count_time += delta;
        count_frame++;

        if (count_time < 1)
            return;

        ActualFPS = count_frame / count_time;
        Logger.Write($"FPS: {ActualFPS:F2}\r", true);

        count_frame = 0;
        count_time = 0;
    }

    private string GetWindowName()
    {
        var ver = Assembly.GetExecutingAssembly().GetName().Version;
        var name = Game.Name;

        if (ConfigManager.DebugBuild)
            return name + $" - Debug {ver!.Build}";

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

        window?.Dispose();
    }
}