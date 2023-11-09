using DanmakuEngine.Dependency;
using DanmakuEngine.Games.Screens;
using DanmakuEngine.Games.Screens.Welcome;

namespace DanmakuEngine.Games;

public partial class Game : IInjectable
{
    public readonly string Name = "Danmaku!";

    [Inject]
    private ScreenStack screens = null!;

    /// <summary>
    /// The game logic starts here
    /// </summary>
    public void Begin()
    {
        AutoInject();

        screens.Push(new LoadingScreen());
    }
}