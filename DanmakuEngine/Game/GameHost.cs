using DanmakuEngine.Arguments;
using DanmakuEngine.Configuration;
using DanmakuEngine.Logging;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace DanmakuEngine.Games;

public class GameHost : IDisposable
{
    private static GameHost Instence = null!;

    private IWindow window = null!;

    private Game game = null!;

    public void Run(Game game)
    {
        LoadConfig();

        this.game = game;

        CreateWindow();

        RegisterEvents();

        RunUntilExit();
    }

    private void LoadConfig()
    {
        using var argParser = new ArgumentParser(new ParamTemplate());
        using var argProvider = argParser.CreateArgumentProvider();

        var directory = argProvider.GetValue<string>("-log");

        Logger.SetLogDirectory(directory);

        ConfigManager.LoadFromArguments(argProvider);
    }

    private void CreateWindow()
    {
        var options = WindowOptions.Default;
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
        window.Update += (_) => Logger.Write($"FPS: {window.FramesPerSecond}\r", true);

        window.Render += OnRender;

        Console.CancelKeyPress += (_, _) => PerformExit();
    }

    public void RunUntilExit()
    {
        window.Run();

        PerformExit();
    }

    private void PerformExit()
    {
        window.Close();

        Console.CursorVisible = true;

        window.Dispose();
    }

    private void OnLoad()
    {
        //Set-up input context.
        IInputContext input = window.CreateInput();
        for (int i = 0; i < input.Keyboards.Count; i++)
        {
            input.Keyboards[i].KeyDown += KeyDown;
        }

        void KeyDown(IKeyboard arg1, Key arg2, int arg3)
        {
            //Check to close the window on escape.
            if (arg2 == Key.Escape)
            {
                window.Close();
            }
        }
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
        var name = game.Name;

        if (ConfigManager.DebugBuild)
            return name + $" - Debug {ver!.Build}";

        return name + $" - ver {ver}";
    }

    public GameHost()
    {
        if (Instence != null)
            throw new InvalidOperationException("Can NOT create multiple GameHost instence");
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