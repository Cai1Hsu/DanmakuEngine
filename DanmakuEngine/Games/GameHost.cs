using System.Runtime;
using DanmakuEngine.Arguments;
using DanmakuEngine.Configuration;
using DanmakuEngine.Dependency;
using DanmakuEngine.Input;
using DanmakuEngine.Logging;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace DanmakuEngine.Games;

public class GameHost : IDisposable, IInjectable
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

        LoadConfig();
        
        GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
        
        CreateWindow();

        RegisterEvents();

        RunUntilExit();
    }

    private void SetUpDependency() => Dependencies = new DependencyContainer(this);

    private void LoadConfig()
    {
        ConfigManager = new ConfigManager();
        Dependencies.CacheAndInject(ConfigManager);

        using var argParser = new ArgumentParser(new ParamTemplate());
        using var argProvider = argParser.CreateArgumentProvider();

        ConfigManager.LoadFromArguments(argProvider);
    }

    private void CreateWindow()
    {
        var options = WindowOptions.Default;

        // TODO
        options.Size = new Vector2D<int>(640, 480);

        options.WindowBorder = WindowBorder.Fixed;
        options.WindowState = ConfigManager.FullScreen ? WindowState.Fullscreen : WindowState.Normal;
        options.VideoMode = new VideoMode(ConfigManager.RefreshRate);
        options.VSync = ConfigManager.Vsync;

        options.Title = GetWindowName();

        window = Window.Create(options);
    }

    private void RegisterEvents()
    {
        Console.CursorVisible = false;

        window.Load += OnLoad;
        window.Update += OnUpdate;
        window.Update += delta => Logger.Write($"FPS: {1 / delta:F2}\r", true);

        window.Render += OnRender;

        Console.CancelKeyPress += (_, _) => PerformExit();
    }

    public void RunUntilExit()
    {
        window.Run();

        PerformExit();
    }

    public void PerformExit()
    {
        if (!window.IsClosing)
            window.Close();

        window.Dispose();

        Console.CursorVisible = true;
    }

    private void OnLoad()
    {
        _gl = window.CreateOpenGL();
        
        //Set-up input context.
        InputManager = new InputManager(window.CreateInput());
        
        Dependencies.CacheAndInject(InputManager);
    }

    private void OnRender(double time)
    {
        //Here all rendering should be done.
    }

    private void OnUpdate(double time)
    {
        //Here all updates to the program should be done.

        // Call Game.Update... 
    }

    private string GetWindowName()
    {
        var ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
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

    public void Inject(DependencyContainer container)
    {
        // Don't need
        // this.Dependencies = container;
    }
}