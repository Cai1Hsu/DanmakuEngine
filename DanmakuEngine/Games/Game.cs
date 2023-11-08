using DanmakuEngine.Dependency;
using DanmakuEngine.Games.Screens;
using DanmakuEngine.Games.Screens.Welcome;

namespace DanmakuEngine.Games;

public class Game : IInjectable
{
    public readonly string Name = "Danmaku!";

    [Inject]
    private ScreenStack screens = null!;

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
}