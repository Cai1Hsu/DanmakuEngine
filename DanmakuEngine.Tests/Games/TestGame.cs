using DanmakuEngine.Games;
using DanmakuEngine.Games.Screens;

namespace DanmakuEngine.Tests.Games;

public partial class TestGame : Game
{
    public override string Name => $"{base.Name} TestGame";

    public override Screen EntryScreen => new TestScreen();

    private class TestScreen : Screen
    {
    }
}
