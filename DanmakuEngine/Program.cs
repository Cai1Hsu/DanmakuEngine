// A library to create STG game
// and magnificent *Danmaku*
using DanmakuEngine.Games;
using DanmakuEngine.Games.Platform;

using (var host = DesktopGameHost.GetSuitableHost())
{
    host.Run(new Game());
}