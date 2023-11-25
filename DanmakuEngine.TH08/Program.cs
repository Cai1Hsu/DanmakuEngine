using DanmakuEngine.Engine;
using DanmakuEngine.Engine.Platform;
using DanmakuEngine.TH08.Games;

using (var host = DesktopGameHost.GetSuitableHost())
{
    host.Run(new TH08Game());
}