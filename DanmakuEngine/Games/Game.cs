using DanmakuEngine.Dependency;
using DanmakuEngine.Games.Screens;
using DanmakuEngine.Logging;

namespace DanmakuEngine.Games;

public partial class Game
{
    public virtual string Name => @"Danmaku!";

    [Inject]
    protected ScreenStack screens = null!;

    /// <summary>
    /// Prepare the game and start it
    /// </summary>
    public void begin()
    {
        if (this is IInjectable injectable)
            injectable.AutoInject();

        this.Begin();
    }

    /// <summary>
    /// The game logic starts here
    /// </summary>
    protected virtual void Begin()
    {

    }
}
