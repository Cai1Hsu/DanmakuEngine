using DanmakuEngine.Dependency;
using DanmakuEngine.Games.Screens;
using DanmakuEngine.Logging;

namespace DanmakuEngine.Games;

public abstract partial class Game
{
    public virtual string Name => @"Danmaku!";

    [Inject]
    protected ScreenStack screens = null!;

    public abstract Screen EntryScreen { get; }

    /// <summary>
    /// Prepare the game
    /// </summary>
    public void prelude()
    {
        if (this is IInjectable injectable)
            injectable.AutoInject();

        this.Prelude();
    }

    /// <summary>
    /// The game logic starts here
    /// </summary>
    protected virtual void Prelude()
    {
    }
}
