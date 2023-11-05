using DanmakuEngine.Dependency;
using DanmakuEngine.Games.Screens;
using DanmakuEngine.Games.Screens.LoadingScreen;

namespace DanmakuEngine.Games;

public class Game : IInjectable, ICacheHookable
{
    public readonly string Name = "Danmaku!";

    public ScreenStack screens = new();

    // [Inject]
    // private GameHost _host = null!;

    public Game()
    {
        
    }
    
    /// <summary>
    /// The game logic starts here
    /// </summary>
    public void Begin()
    {
        screens.Push(new LoadingScreen());
    }

    public void OnCache(DependencyContainer dependencies)
    {
        dependencies.Cache(screens);
    }
}