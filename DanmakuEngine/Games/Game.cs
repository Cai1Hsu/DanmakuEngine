using DanmakuEngine.Dependency;
using DanmakuEngine.Games.Screens;

namespace DanmakuEngine.Games;

public partial class Game : IInjectable
{
    public virtual string Name => @"Danmaku!";

    [Inject]
    private ScreenStack screens = null!;

    /// <summary>
    /// Prepare the game and start it
    /// </summary>
    public void begin()
    {
        AutoInject();

        this.Begin();
    }

    /// <summary>
    /// The game logic starts here
    /// </summary>
    protected virtual void Begin()
    {

    }
}
