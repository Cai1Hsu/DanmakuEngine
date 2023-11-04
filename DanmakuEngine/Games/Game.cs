using DanmakuEngine.Dependency;
using DanmakuEngine.Games.Screens;

namespace DanmakuEngine.Games;

public class Game : IInjectable, ICacheHookable
{
    public readonly string Name = "Danmaku!";

    public ScreenStack screens;

    [Inject]
    public GameHost _host = null!;

    public Game()
    {
        screens = new();
    }

    public void OnCache(DependencyContainer dependencies)
    {
        screens.Push(new MainMenu());

        dependencies.Cache(screens);
    }
}