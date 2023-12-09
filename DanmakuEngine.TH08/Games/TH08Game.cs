using DanmakuEngine.Games;
using DanmakuEngine.Games.Screens.Welcome;

namespace DanmakuEngine.TH08.Games;

public partial class TH08Game : Game
{
    public override string Name => @"TH08";

    protected override void Begin()
    {
        screens.Push(new LoadingScreen());
    }
}
